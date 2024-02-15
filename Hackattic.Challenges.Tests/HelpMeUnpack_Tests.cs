using FluentAssertions;
using NUnit.Framework;

namespace Hackattic.Challenges;

using static HelpMeUnpack_Constants;

public sealed class HelpMeUnpack_Tests
{
    [TestCase(0)]
    [TestCase(1)]
    [TestCase(-1)]
    [TestCase(13)]
    [TestCase(2 << 16)]
    [TestCase(-(2 << 16))]
    [TestCase(int.MaxValue)]
    [TestCase(int.MinValue)]
    public void Int32(int number)
    {
        var binary = BitConverter.GetBytes(number);
        var actual = HelpMeUnpack_BinaryConverter.ToInteger<int>(binary);
        actual.Should().Be(number);
    }

    [TestCase(1u)]
    [TestCase(2u << 16)]
    [TestCase(uint.MaxValue)]
    [TestCase(uint.MinValue)]
    public void UInt32(uint number)
    {
        var binary = BitConverter.GetBytes(number);
        var actual = HelpMeUnpack_BinaryConverter.ToInteger<uint>(binary);
        actual.Should().Be(number);
    }

    [TestCase(1u)]
    [TestCase(2ul << 48)]
    [TestCase(ulong.MinValue)]
    [TestCase(ulong.MaxValue)]
    public void UInt64(ulong number)
    {
        var binary = BitConverter.GetBytes(number);
        var actual = HelpMeUnpack_BinaryConverter.ToInteger<ulong>(binary);
        actual.Should().Be(number);
    }

    [Test]
    public void UInt64_LessBytes()
    {
        const int value = 10;
        var binary = BitConverter.GetBytes(value);
        var actual = HelpMeUnpack_BinaryConverter.ToInteger<ulong>(binary);
        actual.Should().Be(value);
    }

    [TestCaseSource(nameof(Float16_TestCases))]
    public void Float16(Half number)
    {
        var binary = BitConverter.GetBytes(number);
        var actual = HelpMeUnpack_BinaryConverter.ToFloat<Half>(binary, Float16_Exponent_Bits);
        actual.Should().Be(number);
    }

    private static IEnumerable<Half> Float16_TestCases()
    {
        yield return (Half) 0f;
        yield return (Half) 1f;
        yield return (Half) (-1f);
        yield return (Half) 15213.0f;
        yield return Half.NegativeZero;
        yield return Half.Pi;
        yield return Half.Epsilon;
        yield return Half.MinValue;
        yield return Half.MaxValue;
        yield return Half.NegativeInfinity;
        yield return Half.PositiveInfinity;
        yield return Half.NaN;
    }

    [TestCase(0f)]
    [TestCase(1f)]
    [TestCase(-1f)]
    [TestCase(15213.0f)]
    [TestCase(float.NegativeZero)]
    [TestCase(float.Pi)]
    [TestCase(float.Epsilon)]
    [TestCase(float.MinValue)]
    [TestCase(float.MaxValue)]
    [TestCase(float.NegativeInfinity)]
    [TestCase(float.PositiveInfinity)]
    [TestCase(float.NaN)]
    public void Float32(float number)
    {
        var binary = BitConverter.GetBytes(number);
        var actual = HelpMeUnpack_BinaryConverter.ToFloat<float>(binary, Float32_Exponent_Bits);
        actual.Should().Be(number);
    }

    [TestCase(0d)]
    [TestCase(1d)]
    [TestCase(-1d)]
    [TestCase(15213.0d)]
    [TestCase(double.NegativeZero)]
    [TestCase(double.Pi)]
    [TestCase(double.Epsilon)]
    [TestCase(double.MinValue)]
    [TestCase(double.MaxValue)]
    [TestCase(double.NegativeInfinity)]
    [TestCase(double.PositiveInfinity)]
    [TestCase(double.NaN)]
    public void Float64(double number)
    {
        var binary = BitConverter.GetBytes(number);
        var actual = HelpMeUnpack_BinaryConverter.ToFloat<double>(binary, Float64_Exponent_Bits);
        actual.Should().Be(number);
    }
}