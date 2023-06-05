namespace Mikodev.Binary.Tests.Miscellaneous;

using Mikodev.Binary.Components;
using Mikodev.Binary.Tests.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Xunit;
using Xunit.Abstractions;

public class ArithmeticOverflowTests
{
    private readonly ITestOutputHelper output;

    public ArithmeticOverflowTests(ITestOutputHelper output)
    {
        this.output = output;
    }

    private delegate void ArrayResizeDelegate<E>(ref E[] source, E data);

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

    private delegate void FeaturesConstantEncoderEncodeDelegate<E>(ref Allocator allocator, ReadOnlySpan<E> span);

    [Fact(DisplayName = "Features Constant Encoder Encode Overflow")]
    public void FeaturesConstantEncoderEncodeOverflowTest()
    {
        var generator = Generator.CreateDefault();
        var converter = generator.GetConverter<TimeSpan[]>();
        var flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var forward = ReflectionExtensions.GetFieldValueNotNull(converter, "encoder", flags);
        Assert.Equal("ConstantForwardEncoder`3", forward.GetType().Name);
        var encoder = ReflectionExtensions.GetFieldValueNotNull(forward, "encoder", flags);
        Assert.Equal("ConstantEncoder`2", encoder.GetType().Name);
        var method = (FeaturesConstantEncoderEncodeDelegate<TimeSpan>)Delegate.CreateDelegate(typeof(FeaturesConstantEncoderEncodeDelegate<TimeSpan>), encoder, "Encode");

        var error = Assert.Throws<OverflowException>(() =>
        {
            // invalid reference, do not dereference!!!
            var source = MemoryMarshal.CreateReadOnlySpan(ref Unsafe.NullRef<TimeSpan>(), 0x2000_0000);
            var allocator = new Allocator();
            method.Invoke(ref allocator, source);
        });
        this.output.WriteLine(error.StackTrace);
    }

    private readonly struct FakeValue { }

    private sealed class FakeValueConverter : Converter<FakeValue>
    {
        public FakeValueConverter(int length) : base(length) { }

        public override void Encode(ref Allocator allocator, FakeValue item) => throw new NotSupportedException();

        public override FakeValue Decode(in ReadOnlySpan<byte> span) => throw new NotSupportedException();
    }

    private delegate void Encode<T>(ref Allocator allocator, T? item);

    [Fact(DisplayName = "Constant Encoder Encode With Length Prefix Overflow")]
    public void ConstantEncoderEncodeWithLengthPrefixOverflowTest()
    {
        const int ArrayLength = 32768;
        const int ConverterLength = 65536;
        var generator = Generator.CreateDefaultBuilder().AddConverter(new FakeValueConverter(ConverterLength)).Build();
        var converter = generator.GetConverter<FakeValue[]>();
        var encoder = ReflectionExtensions.GetFieldValueNotNull(converter, "encoder", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.Equal("ConstantEncoder`3", encoder.GetType().Name);
        var method = (Encode<FakeValue[]>)Delegate.CreateDelegate(typeof(Encode<FakeValue[]>), encoder, "EncodeWithLengthPrefix");

        var error = Assert.Throws<OverflowException>(() =>
        {
            // invalid reference, do not dereference!!!
            var source = new FakeValue[ArrayLength];
            var allocator = new Allocator();
            method.Invoke(ref allocator, source);
        });
        this.output.WriteLine(error.StackTrace);
    }

    private sealed class UnsafeRawListData<T>
    {
        public T[] Data = null!;

        public int Size;
    }

    [Fact(DisplayName = "Constant Forward Encoder Encode With Length Prefix Overflow")]
    public void ConstantForwardEncoderEncodeWithLengthPrefixTest()
    {
        var generator = Generator.CreateDefault();
        var converter = generator.GetConverter<List<DateOnly>>();
        var encoder = ReflectionExtensions.GetFieldValueNotNull(converter, "encoder", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.Equal("ConstantForwardEncoder`3", encoder.GetType().Name);
        var method = (Encode<List<DateOnly>>)Delegate.CreateDelegate(typeof(Encode<List<DateOnly>>), encoder, "EncodeWithLengthPrefix");

        var error = Assert.Throws<OverflowException>(() =>
        {
            var source = new List<DateOnly>();
            Unsafe.As<UnsafeRawListData<DateOnly>>(source).Size = 0x4000_0000;
            var allocator = new Allocator();
            method.Invoke(ref allocator, source);
        });
        this.output.WriteLine(error.StackTrace);
    }

    private sealed class FakeConverter<T> : Converter<T>
    {
        public FakeConverter(int length) : base(length) { }

        public override void Encode(ref Allocator allocator, T? item) => throw new NotSupportedException();

        public override T Decode(in ReadOnlySpan<byte> span) => throw new NotSupportedException();
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
}
