using System.Text.Json.Serialization;

// ReSharper disable UnusedType.Local
// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace Hackattic.Challenges;

using static HelpMeUnpack_BinaryConverter;
using static HelpMeUnpack_Constants;

file sealed class HelpMeUnpack : IChallenge<ProblemSet, Solution>
{
    public string Name => "help_me_unpack";

    public Solution Solve(ProblemSet problemSet)
    {
        var decodedBytes = Convert.FromBase64String(problemSet.Bytes);

        Span<byte> bigEndianDoubleBuffer = stackalloc byte[sizeof(double)];
        decodedBytes.AsSpan(24, 8).CopyTo(bigEndianDoubleBuffer);
        bigEndianDoubleBuffer.Reverse();

        return new()
        {
            Int = ToInteger<int>(new(decodedBytes, 0, 4)),
            UInt = ToInteger<uint>(new(decodedBytes, 4, 4)),
            Short = ToInteger<short>(new(decodedBytes, 8, 4)),
            Float = ToFloat<float>(new(decodedBytes, 12, 4), Float32_Exponent_Bits),
            Double = ToFloat<double>(new(decodedBytes, 16, 8), Float64_Exponent_Bits),
            BigEndianDouble = ToFloat<double>(bigEndianDoubleBuffer, Float64_Exponent_Bits)
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