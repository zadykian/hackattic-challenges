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

        problemSet.Bytes.Should().NotBeEmpty();
    }

    void IDisposable.Dispose() => HttpClient.Dispose();

    private readonly struct ProblemSet
    {
        [JsonConstructor]
        public ProblemSet(string bytes) => Bytes = bytes;

        public string Bytes { get; }
    }
}