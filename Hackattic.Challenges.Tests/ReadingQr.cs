using System.Text.Json.Serialization;

// ReSharper disable UnusedType.Local
// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace Hackattic.Challenges;

file sealed class ReadingQr : IChallenge<ProblemSet, Solution>
{
    public string Name => "reading_qr";
    
    public ValueTask<Solution> Solve(ProblemSet problemSet, CancellationToken token = default)
    {
        return default;
    }
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
    public required string NumericCode { get; init; }
}