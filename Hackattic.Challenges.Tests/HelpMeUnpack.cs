using System.Text.Json.Serialization;

// ReSharper disable UnusedType.Local
// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace Hackattic.Challenges;

using static HelpMeUnpack_BinaryConverter;

file sealed class HelpMeUnpack : IChallenge<ProblemSet, Solution>
{
    public string Name => "help_me_unpack";

    public Solution Solve(ProblemSet problemSet)
    {
        var decodedBytes = Convert.FromBase64String(problemSet.Bytes);

        Span<byte> bigEndianDoubleBuffer = stackalloc byte[8];
        decodedBytes.AsSpan(22, 8).CopyTo(bigEndianDoubleBuffer);
        bigEndianDoubleBuffer.Reverse();

        return new()
        {
            Int = ToInt32(new(decodedBytes, 0, 4)),
            UInt = ToUInt32(new(decodedBytes, 4, 4)),
            Short = ToInt16(new(decodedBytes, 8, 2)),
            Float = ToFloat(new(decodedBytes, 10, 4)),
            Double = ToDouble(new(decodedBytes, 14, 8)),
            BigEndianDouble = ToDouble(bigEndianDoubleBuffer)
        };
    }
}

file readonly struct ProblemSet
{
    [JsonConstructor]
    public ProblemSet(string bytes) => Bytes = bytes;

    [JsonPropertyName("bytes")]
    public string Bytes { get; }
}

file readonly struct Solution
{
    [JsonPropertyName("int")]
    public required int Int { get; init; }

    [JsonPropertyName("uint")]
    public required uint UInt { get; init; }

    [JsonPropertyName("short")]
    public required short Short { get; init; }

    [JsonPropertyName("float")]
    public required float Float { get; init; }

    [JsonPropertyName("double")]
    public required double Double { get; init; }

    [JsonPropertyName("big_endian_double")]
    public required double BigEndianDouble { get; init; }
}