using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using Hackattic.Challenges.Configuration;
using NUnit.Framework;

namespace Hackattic.Challenges;

public sealed class HelpMeUnpack : IDisposable
{
    private readonly HttpClient HttpClient = new()
    {
        BaseAddress = new("https://hackattic.com/challenges/")
    };

    [Test]
    [CancelAfter(10_000)]
    public async Task Run()
    {
        var accessToken = AccessToken.Value();
        var problemSetResponse = await HttpClient.GetAsync($"help_me_unpack/problem?access_token={accessToken}");
        problemSetResponse.EnsureSuccessStatusCode();

        var responseContent = await problemSetResponse.Content.ReadAsStreamAsync();
        var problemSet = await JsonSerializer.DeserializeAsync<ProblemSet>(responseContent);
        problemSet.Bytes.Should().NotBeNullOrWhiteSpace();

        var solution = await Solve(problemSet);

        var submission = await HttpClient.PostAsync(
            $"help_me_unpack/solve?access_token={accessToken}",
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

    private static async Task<Solution> Solve(ProblemSet problemSet)
    {
        await Task.CompletedTask;
        var decodedBytes = Convert.FromBase64String(problemSet.Bytes);
        return default;
    }

    void IDisposable.Dispose() => HttpClient.Dispose();

    private readonly struct ProblemSet
    {
        [JsonConstructor]
        public ProblemSet(string bytes) => Bytes = bytes;

        [JsonPropertyName("bytes")]
        public string Bytes { get; }
    }

    private readonly struct Solution
    {
        [JsonPropertyName("int")]
        public int Int { get; init; }

        [JsonPropertyName("uint")]
        public uint UnsignedInt { get; init; }

        [JsonPropertyName("short")]
        public short Short { get; init; }

        [JsonPropertyName("float")]
        public float Float { get; init; }

        [JsonPropertyName("double")]
        public double Double { get; init; }

        [JsonPropertyName("big_endian_double")]
        public double BigEndianDouble { get; init; }
    }
}