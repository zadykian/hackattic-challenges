using System.Numerics;

namespace Hackattic.Challenges;

internal static class HelpMeUnpack_BinaryConverter
{
    public static TInteger ToInteger<TInteger>(ReadOnlySpan<byte> bytes)
        where TInteger : IBinaryInteger<TInteger>
    {
        var result = TInteger.Zero;

        for (var i = 0; i < bytes.Length; i++)
        {
            result |= TInteger.CreateChecked(bytes[i]) << (i * 8);
        }

        return result;
    }

    public static unsafe TFloat ToFloat<TFloat>(ReadOnlySpan<byte> bytes, int exponentBitsCount)
        where TFloat : unmanaged, IBinaryFloatingPointIeee754<TFloat>
    {
        if (bytes.Length < sizeof(TFloat))
        {
            throw new ArgumentException($"Span is too small to represent a value of type {typeof(TFloat)}", nameof(bytes));
        }

        if (sizeof(TFloat) > sizeof(Int128))
        {
            throw new NotSupportedException();
        }

        var sign = (bytes[^1] & 0b1000_0000) == 0b0 ? TFloat.One : TFloat.NegativeOne;

        var mantissaBitsCount = sizeof(TFloat) * 8 - (1 + exponentBitsCount);
        
        var exponentMask = (Int128.One << exponentBitsCount) - 1;
        var exponentBin = (ToInteger<Int128>(bytes) >>> mantissaBitsCount) & exponentMask;

        var mantissaFraction = GetMantissaFraction<TFloat>(bytes, mantissaBitsCount);

        if (exponentBin == exponentMask) // exponentBin == 0b11..1111
        {
            return mantissaFraction == TFloat.Zero
                ? sign / TFloat.Zero // +- Infinity
                : TFloat.Zero / TFloat.Zero; // NaN
        }

        if (exponentBin == Int128.Zero && mantissaFraction == TFloat.Zero)
        {
            return sign * TFloat.Zero;
        }

        TFloat exponent;
        TFloat mantissa;

        // float16: exp = 05 bits -> bias = 2^(05-1) - 1 = 15
        // float32: exp = 08 bits -> bias = 2^(08-1) - 1 = 127
        // float64: exp = 11 bits -> bias = 2^(11-1) - 1 = 1023
        var bias = (Int128.One << (exponentBitsCount - 1)) - 1;

        if (exponentBin != 0) // Normalized
        {
            exponent = TFloat.CreateChecked(exponentBin - bias);
            mantissa = TFloat.One + mantissaFraction;
        }
        else
        {
            exponent = TFloat.CreateChecked(1 - bias);
            mantissa = mantissaFraction;
        }

        return sign * mantissa * TFloat.Exp2(exponent);
    }
    
    private static TFloat GetMantissaFraction<TFloat>(ReadOnlySpan<byte> bytes, int mantissaBitsCount)
        where TFloat : IBinaryFloatingPointIeee754<TFloat>
    {
        // 1. Read mantissa as UInt128 value from last N bits of bytes

        var mantissaMask = (UInt128.One << mantissaBitsCount) - 1;
        var mantissaBin = ToInteger<UInt128>(bytes) & mantissaMask;
    
        if (mantissaBin == 0)
        {
            return TFloat.Zero;
        }
    
        var result = TFloat.Zero;
    
        // 2. Interpret mantissa's binary representation as a sum:
        // 1/2 + 1/4 + 1/8 + ... + 1 / 2^n
    
        var singleBitMask = UInt128.One << (mantissaBitsCount - 1);
    
        for (var i = 0; i < mantissaBitsCount; i++)
        {
            if ((mantissaBin & singleBitMask) == singleBitMask)
            {
                var powerOfTwo = TFloat.NegativeOne * TFloat.CreateChecked(i + 1);
                result += TFloat.Exp2(powerOfTwo);
            }
            singleBitMask >>= 1;
        }
    
        return result;
    }
}