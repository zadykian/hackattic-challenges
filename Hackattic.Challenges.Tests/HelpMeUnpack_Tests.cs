using FluentAssertions;
using NUnit.Framework;

namespace Hackattic.Challenges;

public sealed class HelpMeUnpack_Tests
{
    [TestCase(0)]
    [TestCase(1)]
    [TestCase(-1)]
    [TestCase(2 << 16)]
    [TestCase(-(2 << 16))]
    [TestCase(int.MaxValue)]
    [TestCase(int.MinValue)]
    public void Int(int number)
    {
        var binary = BitConverter.GetBytes(number);
        var actual = HelpMeUnpack_BinaryConverter.ToInt32(binary);
        actual.Should().Be(number);
    }

    [TestCase(1u)]
    [TestCase(2u << 16)]
    [TestCase(uint.MaxValue)]
    [TestCase(uint.MinValue)]
    public void UInt(uint number)
    {
        var binary = BitConverter.GetBytes(number);
        var actual = HelpMeUnpack_BinaryConverter.ToUInt32(binary);
        actual.Should().Be(number);
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
    public void Float(float number)
    {
        var binary = BitConverter.GetBytes(number);
        var actual = HelpMeUnpack_BinaryConverter.ToFloat(binary);
        actual.Should().Be(number);
    }
}