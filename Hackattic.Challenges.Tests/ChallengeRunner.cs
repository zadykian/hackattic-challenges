using System.Reflection;
using System.Text.Json;
using FluentAssertions;
using Hackattic.Challenges.ApiAccess;
using NUnit.Framework;

// ReSharper disable SuspiciousTypeConversion.Global

namespace Hackattic.Challenges;

public interface IChallenge
{
    string Name { get; }
}

public interface IChallenge<in TProblemSet, TSolution> : IChallenge
{
    ValueTask<TSolution> Solve(TProblemSet problemSet, CancellationToken token = default);
}

public sealed class ChallengeRunner : IDisposable
{
    private static readonly MethodInfo RunImplMethodInfo
        = typeof(ChallengeRunner).GetMethod(nameof(RunImpl), BindingFlags.NonPublic | BindingFlags.Instance)!;

    private readonly HttpClient httpClient = new()
    {
        BaseAddress = new("https://hackattic.com/challenges/")
    };

    [TestCaseSource(nameof(LoadChallenges))]
    [CancelAfter(10_000)]
    [Explicit]
    public async Task Run(IChallenge challenge, CancellationToken cancellationToken)
    {
        var genericArgs = challenge
            .GetType()
            .GetInterfaces()
            .First(intType => intType.IsGenericType && intType.GetGenericTypeDefinition() == typeof(IChallenge<,>))
            .GetGenericArguments();

        try
        {
            await (Task) RunImplMethodInfo.MakeGenericMethod(genericArgs).Invoke(this, [ challenge, cancellationToken ])!;
        }
        finally
        {
            switch (challenge)
            {
                case IDisposable disposable : disposable.Dispose(); break;
                case IAsyncDisposable disposable : await disposable.DisposeAsync(); break;
            }
        }
    }

    private async Task RunImpl<TProblemSet, TSolution>(
        IChallenge<TProblemSet, TSolution> challenge,
        CancellationToken cancellationToken
    )
    {
        var accessToken = AccessToken.Value();

        var problemSetResponse = await httpClient.GetAsync(
            $"{challenge.Name}/problem?access_token={accessToken}",
            cancellationToken
        );

        problemSetResponse.EnsureSuccessStatusCode();
        var responseContent = await problemSetResponse.Content.ReadAsStreamAsync(cancellationToken);

        var problemSet = await JsonSerializer.DeserializeAsync<TProblemSet>(
            responseContent,
            cancellationToken: cancellationToken
        );

        var solution = await challenge.Solve(problemSet!, cancellationToken);

        var submission = await httpClient.PostAsync(
            $"{challenge.Name}/solve?access_token={accessToken}",
            new StringContent(JsonSerializer.Serialize(solution)),
            cancellationToken
        );

        submission.EnsureSuccessStatusCode();
        var submissionResponseContent = await submission.Content.ReadAsStreamAsync(cancellationToken);

        var submissionResponse = await JsonSerializer.DeserializeAsync<JsonElement>(
            submissionResponseContent,
            cancellationToken: cancellationToken
        );

        var rejectionReason = submissionResponse.TryGetProperty("rejected", out var rejected)
            ? rejected.GetString()
            : string.Empty;

        rejectionReason
            .Should()
            .BeNullOrWhiteSpace(because: "submission should be accepted");
    }

    private static IEnumerable<TestCaseData> LoadChallenges()
        => Assembly
            .GetExecutingAssembly()
            .GetTypes()
            .Where(type =>
                !type.IsAbstract
                && type
                    .GetInterfaces()
                    .Any(intType => intType.IsGenericType && intType.GetGenericTypeDefinition() == typeof(IChallenge<,>))
            )
            .Select(Activator.CreateInstance)
            .Cast<IChallenge>()
            .Select(challenge => new TestCaseData(challenge) { TestName = challenge.Name });

    void IDisposable.Dispose() => httpClient.Dispose();
}
