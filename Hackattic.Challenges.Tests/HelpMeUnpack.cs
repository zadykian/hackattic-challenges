using System.Text.Json.Serialization;

// ReSharper disable UnusedType.Global
// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace Hackattic.Challenges;

public sealed class HelpMeUnpack : IChallenge<ProblemSet, Solution>
{
    public string Name => "help_me_unpack";

    async Task<Solution> IChallenge<ProblemSet, Solution>.Solve(ProblemSet problemSet)
    {
        await Task.CompletedTask;
        var decodedBytes = Convert.FromBase64String(problemSet.Bytes);

        var intBytes = decodedBytes[..4];

        return new()
        {
            Int = 0,
            UnsignedInt = 0,
            Short = 0,
            Float = 0,
            Double = 0,
            BigEndianDouble = 0
        };
    }
}

public readonly struct ProblemSet
{
    [JsonConstructor]
    public ProblemSet(string bytes) => Bytes = bytes;

    [JsonPropertyName("bytes")]
    public string Bytes { get; }
}

public readonly struct Solution
{
    [JsonPropertyName("int")]
    public required int Int { get; init; }

    [JsonPropertyName("uint")]
    public required uint UnsignedInt { get; init; }

    [JsonPropertyName("short")]
    public required short Short { get; init; }

    [JsonPropertyName("float")]
    public required float Float { get; init; }

    [JsonPropertyName("double")]
    public required double Double { get; init; }

    [JsonPropertyName("big_endian_double")]
    public required double BigEndianDouble { get; init; }
}