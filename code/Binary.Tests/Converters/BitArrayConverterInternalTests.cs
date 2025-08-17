namespace Mikodev.Binary.Tests.Converters;

using System;
using System.Linq;
using System.Reflection;
using Xunit;

public class BitArrayConverterInternalTests
{
    private delegate void TransformFunction(Span<byte> target, ReadOnlySpan<byte> source, int length);

    private readonly TransformFunction transform;

    public BitArrayConverterInternalTests()
    {
        var type = typeof(IConverter).Assembly.GetTypes().Single(x => x.Name is "BitArrayConverter");
        var transform = type.GetMethod("Transform", BindingFlags.Static | BindingFlags.NonPublic);
        this.transform = (TransformFunction)Delegate.CreateDelegate(typeof(TransformFunction), transform ?? throw new Exception());
    }

    [Theory(DisplayName = "Transform Trim Tail Bits")]
    [InlineData(new byte[] { 1 }, new byte[] { 0xFF }, 1)]
    [InlineData(new byte[] { 127 }, new byte[] { 0xFF }, 7)]
    [InlineData(new byte[] { 0 }, new byte[] { 0xAA }, 1)]
    [InlineData(new byte[] { 1 }, new byte[] { 0x55 }, 1)]
    [InlineData(new byte[] { 0b010 }, new byte[] { 0xAA }, 3)]
    [InlineData(new byte[] { 0b10101 }, new byte[] { 0x55 }, 5)]
    public void TransformTrimTailBits(byte[] expected, byte[] source, int length)
    {
        var actual = new byte[expected.Length];
        this.transform.Invoke(actual, source, length);
        Assert.Equal(expected, actual);
    }
}
