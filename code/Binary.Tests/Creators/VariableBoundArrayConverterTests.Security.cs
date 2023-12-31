namespace Mikodev.Binary.Tests.Creators;

using System;
using System.Collections.Generic;
using Xunit;

public partial class VariableBoundArrayConverterTests
{
    private class FakeType { }

    private class FakeConverter<T>(int length) : Converter<T>(length)
    {
        public override void Encode(ref Allocator allocator, T? item) => throw new NotSupportedException();

        public override T Decode(in ReadOnlySpan<byte> span) => throw new NotSupportedException();
    }

    [Theory(DisplayName = "Not Enough Bytes Test")]
    [InlineData(new[] { 1, 1 }, 0, 0)]
    [InlineData(new[] { 1, 1, 1 }, 0, 0)]
    [InlineData(new[] { 1, 1, 1 }, 1, 0)]
    [InlineData(new[] { 1, 1, 1 }, 2, 1)]
    [InlineData(new[] { 3, 4, 5 }, 0, 59)]
    [InlineData(new[] { 7, 8, 9 }, 4, 2015)]
    public void NotEnoughBytesTest(IReadOnlyList<int> lengths, int converterLength, int remainingLength)
    {
        var allocator = new Allocator();
        foreach (var i in lengths)
            Converter.Encode(ref allocator, i);
        foreach (var _ in lengths)
            Converter.Encode(ref allocator, 0);
        Allocator.Expand(ref allocator, remainingLength);
        var buffer = allocator.ToArray();

        var generator = Generator.CreateDefaultBuilder().AddConverter(new FakeConverter<FakeType>(converterLength)).Build();
        var arrayType = typeof(FakeType).MakeArrayType(lengths.Count);
        var converter = generator.GetConverter(arrayType);
        Assert.StartsWith("VariableBoundArrayConverter", converter.GetType().Name);
        var error = Assert.Throws<ArgumentException>(() => converter.Decode(buffer));
        Assert.Equal("Not enough bytes or byte sequence invalid.", error.Message);
    }

    [Theory(DisplayName = "Array Length Overflow Test")]
    [InlineData(new[] { 0x1_0000, 0x1_0000 })]
    [InlineData(new[] { 0x8000, 0x8000, 0x2 })]
    [InlineData(new[] { 0x2, 0x2, 0x4000, 0x8000 })]
    public void ArrayLengthOverflowTest(IReadOnlyList<int> lengths)
    {
        var allocator = new Allocator();
        foreach (var i in lengths)
            Converter.Encode(ref allocator, i);
        foreach (var _ in lengths)
            Converter.Encode(ref allocator, 0);
        var buffer = allocator.ToArray();

        var generator = Generator.CreateDefaultBuilder().AddConverter(new FakeConverter<FakeType>(0)).Build();
        var arrayType = typeof(FakeType).MakeArrayType(lengths.Count);
        var converter = generator.GetConverter(arrayType);
        Assert.StartsWith("VariableBoundArrayConverter", converter.GetType().Name);
        var error = Assert.Throws<OverflowException>(() => converter.Decode(buffer));
        Assert.Equal(new OverflowException().Message, error.Message);
    }
}
