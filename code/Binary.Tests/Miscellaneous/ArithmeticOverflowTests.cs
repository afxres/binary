namespace Mikodev.Binary.Tests.Miscellaneous;

using Mikodev.Binary.Components;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Xunit;
using Xunit.Abstractions;

public class ArithmeticOverflowTests(ITestOutputHelper output)
{
    private readonly ITestOutputHelper output = output;

    private delegate void ArrayResizeDelegate<E>(ref E[] source, E data);

    private delegate void EncodeReadOnlySpanDelegate<E>(ref Allocator allocator, ReadOnlySpan<E> data);

    private sealed class FakeConverter<T>(int length) : Converter<T>(length)
    {
        public override void Encode(ref Allocator allocator, T? item) => throw new NotSupportedException();

        public override T Decode(in ReadOnlySpan<byte> span) => throw new NotSupportedException();
    }

    [Fact(DisplayName = "Array Resize Overflow")]
    public void ArrayResizeOverflowTest()
    {
        var type = typeof(IConverter).Assembly.GetTypes().Single(x => x.Name is "SpanLikeMethods");
        var method = type.GetMethods(BindingFlags.Static | BindingFlags.NonPublic).Single(x => x.Name.Contains("Expand"));
        var expand = (ArrayResizeDelegate<byte>)Delegate.CreateDelegate(typeof(ArrayResizeDelegate<byte>), method.MakeGenericMethod(typeof(byte)));
        var buffer = new byte[0x4000_0000];
        var error = Assert.Throws<OverflowException>(() => expand.Invoke(ref buffer, 0));
        this.output.WriteLine(error.StackTrace);
    }

    [Theory(DisplayName = "Get Converter Length Overflow")]
    [InlineData(new int[] { 0x4000_0000, 0x4000_0000 })]
    [InlineData(new int[] { 0x3000_0000, 0x3000_0000, 0x3000_0000 })]
    public void GetConverterLengthOverflow(int[] lengths)
    {
        var converters = lengths.Select(x => new FakeConverter<object>(x)).ToList();
        var error = Assert.Throws<OverflowException>(() => TupleObject.GetConverterLength(converters));
        Assert.Equal(new OverflowException().Message, error.Message);
    }

    [Theory(DisplayName = "Encode Native Endian Overflow Test")]
    [InlineData(default(int), 0x2000_0000)]
    [InlineData(default(long), 0x1000_0000)]
    public void EncodeNativeEndianOverflowTest<E>(E metadata, int dataLength)
    {
        Assert.Equal(metadata, default);
        var type = typeof(IConverter).Assembly.GetTypes().Single(x => x.Name is "SpanLikeNativeEndianMethods");
        var method = type.GetMethods(BindingFlags.Static | BindingFlags.NonPublic).Single(x => x.Name.Equals("Encode"));
        var invoke = (EncodeReadOnlySpanDelegate<E>)Delegate.CreateDelegate(typeof(EncodeReadOnlySpanDelegate<E>), method.MakeGenericMethod(typeof(E)));
        var error = Assert.Throws<OverflowException>(() =>
        {
            var allocator = new Allocator([], 0);
            // invalid null reference, do not dereference!!!
            var data = MemoryMarshal.CreateReadOnlySpan(ref Unsafe.NullRef<E>(), dataLength);
            invoke.Invoke(ref allocator, data);
        });
        this.output.WriteLine(error.StackTrace);
    }

    [Theory(DisplayName = "Encode Native Endian With Length Prefix Overflow Test")]
    [InlineData(default(int), 0x2000_0000)]
    [InlineData(default(long), 0x1000_0000)]
    public void EncodeNativeEndianWithLengthPrefixOverflowTest<E>(E metadata, int dataLength)
    {
        Assert.Equal(metadata, default);
        var type = typeof(IConverter).Assembly.GetTypes().Single(x => x.Name is "SpanLikeNativeEndianMethods");
        var method = type.GetMethods(BindingFlags.Static | BindingFlags.NonPublic).Single(x => x.Name.Equals("EncodeWithLengthPrefix"));
        var invoke = (EncodeReadOnlySpanDelegate<E>)Delegate.CreateDelegate(typeof(EncodeReadOnlySpanDelegate<E>), method.MakeGenericMethod(typeof(E)));
        var error = Assert.Throws<OverflowException>(() =>
        {
            var allocator = new Allocator([], 0);
            // invalid null reference, do not dereference!!!
            var data = MemoryMarshal.CreateReadOnlySpan(ref Unsafe.NullRef<E>(), dataLength);
            invoke.Invoke(ref allocator, data);
        });
        this.output.WriteLine(error.StackTrace);
    }
}
