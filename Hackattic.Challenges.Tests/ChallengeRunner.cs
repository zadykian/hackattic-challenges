using System.Reflection;
using System.Text.Json;
using FluentAssertions;
using Hackattic.Challenges.ApiAccess;
using NUnit.Framework;

namespace Hackattic.Challenges;

public interface IChallenge;

public interface IChallenge<in TProblemSet, TSolution> : IChallenge
{
    string Name { get; }

    Task<TSolution> Solve(TProblemSet problemSet);
}

public sealed class ChallengeRunner : IDisposable
{
    private readonly HttpClient httpClient = new()
    {
        BaseAddress = new("https://hackattic.com/challenges/")
    };

    [TestCaseSource(nameof(LoadChallenges))]
    [CancelAfter(10_000)]
    public async Task Run<TProblemSet, TSolution>(IChallenge<TProblemSet, TSolution> challenge)
    {
        var accessToken = AccessToken.Value();

        var problemSetResponse = await httpClient.GetAsync(
            $"{challenge.Name}/problem?access_token={accessToken}"
        );

        problemSetResponse.EnsureSuccessStatusCode();

        var responseContent = await problemSetResponse.Content.ReadAsStreamAsync();
        var problemSet = await JsonSerializer.DeserializeAsync<TProblemSet>(responseContent);

        var solution = await challenge.Solve(problemSet!);

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

        rejectionReason.Should().BeNullOrWhiteSpace();
    }

    private static IEnumerable<IChallenge> LoadChallenges()
        => Assembly
            .GetExecutingAssembly()
            .GetTypes()
            .Where(type =>
                !type.IsAbstract
                && type.GetInterfaces().Any(intType =>
                    intType.IsGenericType
                    && intType.GetGenericTypeDefinition() == typeof(IChallenge<,>)
                )
            )
            .Select(Activator.CreateInstance)
            .Cast<IChallenge>();

    void IDisposable.Dispose() => httpClient.Dispose();
}