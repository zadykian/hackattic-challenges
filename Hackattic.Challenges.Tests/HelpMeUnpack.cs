using System.Text.Json.Serialization;

// ReSharper disable UnusedType.Local
// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace Hackattic.Challenges;

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

    private static int ToInt32(ReadOnlySpan<byte> bytes)
        => bytes[0] | bytes[1] << 8 | bytes[2] << 16 | bytes[3] << 24;

    private static uint ToUInt32(ReadOnlySpan<byte> bytes)
        => (uint) ToInt32(bytes);

    private static short ToInt16(ReadOnlySpan<byte> bytes)
        => (short) (bytes[0] | bytes[1] << 8);

    private static float ToFloat(ReadOnlySpan<byte> bytes)
    {
        // s:1, e:8, m:23

        var sign = (bytes[3] & 0b1000_0000) == 0b0 ? 1 : -1;

        var exponentBin =
            (bytes[3] & 0b0111_1111) << 1 | // 7 least significant bits from 4th byte
            (bytes[2] & 0b1000_0000) >> 7 ; // 1 most significant bit from 3rd byte

        var mantissaFraction = GetMantissaFractionFloat(bytes);

        var isNormalized = exponentBin is not 0b0000_0000 and not 0b1111_1111;

    }
    
    private static float GetMantissaFractionFloat(ReadOnlySpan<byte> bytes)
    {
        // 1. Read mantissa as Int32 value from last 23 bits of bytes [ ww, xx, yy, zz ]
        var mantissaBin = (bytes[2] & 0b0111_1111) << 16 | bytes[1] << 8 | bytes[0] << 0 ;

        float result = 0;

        if (mantissaBin == 0)
        {
            return result;
        }

        // 2. Interpret mantissa's binary representation as a sum:
        // 1/2 + 1/4 + 1/8 + ... + 1 / 2^n
        
        var mask = 1 << 22;
        for (var i = 0; i < 23; i++)
        {
            if ((mantissaBin & mask) == 1)
            {
                result += 1f / (2 << i);
            }
            mask >>= 1;
        }

        return result;
    }

    private static double ToDouble(ReadOnlySpan<byte> bytes)
    {
        // s:1, e:11, m: 52
        
        return default; // todo
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