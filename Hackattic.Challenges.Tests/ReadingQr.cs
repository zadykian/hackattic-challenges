using System.Text.Json.Serialization;
using SixLabors.ImageSharp;

// ReSharper disable UnusedType.Local
// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace Hackattic.Challenges;

file sealed class ReadingQr : IChallenge<ProblemSet, Solution>, IDisposable
{
    private readonly HttpClient httpClient = new();

    public string Name => "reading_qr";

    public async ValueTask<Solution> Solve(ProblemSet problemSet, CancellationToken token = default)
    {
        var imageResponse = await httpClient.GetAsync(problemSet.ImageUrl, token);
        imageResponse.EnsureSuccessStatusCode();
        var imageStream = await imageResponse.Content.ReadAsStreamAsync(token);
        using var image = await Image.LoadAsync(imageStream, token);

        return default;
    }

    void IDisposable.Dispose() => httpClient.Dispose();
}

file readonly struct ProblemSet
{
    [JsonConstructor]
    public ProblemSet(string imageUrl) => ImageUrl = imageUrl;

    [JsonPropertyName("image_url")]
    public string ImageUrl { get; }
}

file readonly struct Solution
{
    [JsonPropertyName("code")]
    public required ulong Code { get; init; }
}