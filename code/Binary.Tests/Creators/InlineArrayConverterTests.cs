namespace Mikodev.Binary.Tests.Creators;

using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Xunit;

public class InlineArrayConverterTests
{
    [InlineArray(10)]
    private struct InlineInt32Array
    {
        public int InstanceField;

        public static int StaticField;

        public static double StaticDoubleField;
    }

    [InlineArray(4)]
    private struct InlineGenericArray<T>
    {
        internal T? InternalField;

        internal static T? InternalStaticField;

        internal static T?[]? InternalStaticArrayField;
    }

    private static void TestAll<T, E>(T item, E[] expected, int converterLength)
    {
        var generator = Generator.CreateDefault();
        var converter = generator.GetConverter<T>();
        var converterType = converter.GetType();
        Assert.Equal("InlineArrayConverter`2", converterType.Name);
        Assert.Equal(converterLength, converter.Length);

        var buffer = converter.Encode(item);
        var bufferExpected = generator.Encode(expected);
        Assert.Equal(bufferExpected, buffer);

        var result = converter.Decode(buffer);
        var bufferResult = converter.Encode(result);
        Assert.Equal(bufferExpected, bufferResult);
    }

    [Fact(DisplayName = "Integration Test")]
    public void IntegrationTest()
    {
        var a = new InlineInt32Array();
        var b = Enumerable.Range(0, 10).ToArray();
        new ReadOnlySpan<int>(b).CopyTo(a);
        TestAll(a, b, 40);

        var c = new InlineGenericArray<string>();
        var d = Enumerable.Range(0, 4).Select(x => x.ToString()).ToArray();
        new ReadOnlySpan<string?>(d).CopyTo(c);
        TestAll(c, d, 0);
    }

    private class FakeConverter<T>(int length) : Converter<T>(length)
    {
        public override void Encode(ref Allocator allocator, T? item) => throw new NotSupportedException();

        public override T Decode(in ReadOnlySpan<byte> span) => throw new NotSupportedException();
    }

    [Fact(DisplayName = "Converter Length Overflow Test")]
    public void ConverterLengthOverflowTest()
    {
        var a = Generator.CreateDefaultBuilder().AddConverter(new FakeConverter<int>(0x1000_0000)).Build();
        var b = a.GetConverter<int>();
        Assert.Equal(0x1000_0000, b.Length);
        var c = a.GetConverter<InlineGenericArray<int>>();
        Assert.Equal(0x4000_0000, c.Length);

        var h = Generator.CreateDefaultBuilder().AddConverter(new FakeConverter<double>(0x2000_0000)).Build();
        var i = h.GetConverter<double>();
        Assert.Equal(0x2000_0000, i.Length);
        var error = Assert.Throws<OverflowException>(() => h.GetConverter<InlineGenericArray<double>>());
        Assert.Equal(new OverflowException().Message, error.Message);
    }
}
