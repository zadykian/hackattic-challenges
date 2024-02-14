namespace Hackattic.Challenges;

internal static class HelpMeUnpack_BinaryConverter
{
    public static int ToInt32(ReadOnlySpan<byte> bytes)
        => bytes[0] | bytes[1] << 8 | bytes[2] << 16 | bytes[3] << 24;

    public static uint ToUInt32(ReadOnlySpan<byte> bytes)
        => (uint) ToInt32(bytes);

    public static short ToInt16(ReadOnlySpan<byte> bytes)
        => (short) (bytes[0] | bytes[1] << 8);

    public static float ToFloat(ReadOnlySpan<byte> bytes)
    {
        // s:1, e:8, m:23

        var sign = (bytes[3] & 0b1000_0000) == 0b0 ? 1 : -1;

        var exponentBin =
            (bytes[3] & 0b0111_1111) << 1 | // 7 least significant bits from 4th byte
            (bytes[2] & 0b1000_0000) >> 7 ; // 1 most significant bit from 3rd byte

        var mantissaFraction = GetMantissaFractionFloat(bytes);

        if (exponentBin == 0b1111_1111)
        {
            return mantissaFraction == 0
                ? sign / 0f // +- Infinity
                : 0f / 0f;  // NaN
        }

        int exponent;
        float mantissa;
        if (exponentBin is not 0b0000_0000) // Normalized
        {
            exponent = exponentBin - 127;
            mantissa = 1 + mantissaFraction;
        }
        else
        {
            exponent = 1 - 127;
            mantissa = mantissaFraction;
        }

        return sign * mantissa * (float) Math.Pow(2, exponent);
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
            if ((mantissaBin & mask) == mask)
            {
                result += 1f / (2 << i);
            }
            mask >>= 1;
        }

        return result;
    }

    public static double ToDouble(ReadOnlySpan<byte> bytes)
    {
        // s:1, e:11, m: 52
        
        return default; // todo
    }
}