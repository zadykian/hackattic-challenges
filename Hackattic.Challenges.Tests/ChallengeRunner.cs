using System.Reflection;
using System.Text.Json;
using FluentAssertions;
using Hackattic.Challenges.ApiAccess;
using NUnit.Framework;

namespace Hackattic.Challenges;

public interface IChallenge
{
    string Name { get; }
}

public interface IChallenge<in TProblemSet, out TSolution> : IChallenge
{
    TSolution Solve(TProblemSet problemSet);
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
    public Task Run(IChallenge challenge)
    {
        var genericArgs = challenge
            .GetType()
            .GetInterfaces()
            .First(intType => intType.IsGenericType && intType.GetGenericTypeDefinition() == typeof(IChallenge<,>))
            .GetGenericArguments();

        return (Task) RunImplMethodInfo.MakeGenericMethod(genericArgs).Invoke(this, [ challenge ])!;
    }

    private async Task RunImpl<TProblemSet, TSolution>(IChallenge<TProblemSet, TSolution> challenge)
    {
        var accessToken = AccessToken.Value();

        var problemSetResponse = await httpClient.GetAsync(
            $"{challenge.Name}/problem?access_token={accessToken}"
        );

        problemSetResponse.EnsureSuccessStatusCode();
        var responseContent = await problemSetResponse.Content.ReadAsStreamAsync();
        var problemSet = await JsonSerializer.DeserializeAsync<TProblemSet>(responseContent);

        var solution = challenge.Solve(problemSet!);

        var submission = await httpClient.PostAsync(
            $"{challenge.Name}/solve?access_token={accessToken}",
            new StringContent(JsonSerializer.Serialize(solution))
        );

        submission.EnsureSuccessStatusCode();
        var submissionResponseContent = await submission.Content.ReadAsStreamAsync();
        var submissionResponse = await JsonSerializer.DeserializeAsync<JsonElement>(submissionResponseContent);

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
