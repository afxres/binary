namespace Mikodev.Binary.Tests.Miscellaneous;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;

public class NativeEndianTests
{
    private delegate bool IsNativeEndianConverterInternalDelegate(IConverter converter, bool isLittleEndian);

    private static IsNativeEndianConverterInternalDelegate GetIsNativeEndianConverterInternalDelegate()
    {
        var type = typeof(IConverter).Assembly.GetTypes().Single(x => x.Name is "NativeEndian");
        var method = type.GetMethods(BindingFlags.Static | BindingFlags.NonPublic).Single(x => x.Name.Contains("Invoke"));
        var result = Assert.IsType<IsNativeEndianConverterInternalDelegate>(Delegate.CreateDelegate(typeof(IsNativeEndianConverterInternalDelegate), method), exactMatch: false);
        return result;
    }

    [Fact(DisplayName = "Is Native Endian Converter Internal Integration Test")]
    public void IsNativeEndianConverterInternalIntegrationTest()
    {
        var generator = Generator.CreateDefault();
        var method = GetIsNativeEndianConverterInternalDelegate();
        Assert.False(method.Invoke(null!, false));
        Assert.False(method.Invoke(generator.GetConverter<string>(), true));
        Assert.False(method.Invoke(generator.GetConverter<List<int>>(), true));
        var a = generator.GetConverter<int>();
        var b = generator.GetConverter<Range>();
        Assert.Equal("LittleEndianConverter`1", a.GetType().Name);
        Assert.Equal("RepeatLittleEndianConverter`2", b.GetType().Name);
        Assert.True(method.Invoke(a, true));
        Assert.True(method.Invoke(b, true));
    }

    private sealed class FakeConverter<T>(int length) : Converter<T>(length)
    {
        public override void Encode(ref Allocator allocator, T? item) => throw new NotSupportedException();

        public override T Decode(in ReadOnlySpan<byte> span) => throw new NotSupportedException();
    }

    [Fact(DisplayName = "Is Native Endian Converter Single Byte Element Test")]
    public void IsNativeEndianConverterSingleByteElementTest()
    {
        var generator = Generator.CreateDefault();
        var method = GetIsNativeEndianConverterInternalDelegate();

        // one byte wide elements carry no byte order, the copying fast path must stay enabled on big endian machines,
        // otherwise byte arrays would fall back to the per element path and allocate buffer sizes differently
        var singleByteConverters = new IConverter[] { generator.GetConverter<byte>(), generator.GetConverter<sbyte>(), generator.GetConverter<bool>() };
        foreach (var converter in singleByteConverters)
        {
            Assert.Equal(1, converter.Length);
            Assert.True(method.Invoke(converter, false));
            Assert.True(method.Invoke(converter, true));
        }

        // two bytes wide and above do depend on the machine byte order
        var multiByteConverters = new IConverter[] { generator.GetConverter<char>(), generator.GetConverter<short>(), generator.GetConverter<Half>(), generator.GetConverter<int>(), generator.GetConverter<long>(), generator.GetConverter<double>() };
        foreach (var converter in multiByteConverters)
        {
            Assert.True(converter.Length > 1);
            Assert.False(method.Invoke(converter, false));
            Assert.True(method.Invoke(converter, true));
        }

        // copying relies on the size of the element type, so an arbitrary converter reporting a one byte length
        // must never qualify, the fast path would copy the whole element instead of the reported length
        var fake = new FakeConverter<int>(1);
        Assert.Equal(1, fake.Length);
        Assert.False(method.Invoke(fake, false));
        Assert.False(method.Invoke(fake, true));
    }

    public static IEnumerable<object[]> ArrayData()
    {
        yield return [new byte[] { 0x00, 0x7F, 0x80, 0xFF }];
        yield return [new sbyte[] { 0, 1, -1, sbyte.MinValue, sbyte.MaxValue }];
        yield return [new bool[] { true, false, true }];
        yield return [Array.Empty<byte>()];
        yield return [new char[] { 'a', '一', '\0' }];
        yield return [new short[] { 0, -1, short.MinValue }];
        yield return [new int[] { 0, -1, int.MinValue }];
        yield return [new Half[] { (Half)1.5F, (Half)(-2.5F) }];
    }

    [Theory(DisplayName = "Array Wire Equals Element Wire Test")]
    [MemberData(nameof(ArrayData))]
    public void ArrayWireEqualsElementWireTest<T>(T[] items)
    {
        // the copying fast path must produce exactly the same bytes as the per element path does,
        // this holds on little endian and big endian machines alike
        var generator = Generator.CreateDefault();
        var converter = generator.GetConverter<T>();
        var allocator = new Allocator();
        foreach (var item in items)
            converter.Encode(ref allocator, item);
        var payload = allocator.AsSpan().ToArray();

        var arrayConverter = generator.GetConverter<T[]>();
        Assert.Equal(payload, arrayConverter.Encode(items));

        // the length prefix counts bytes instead of elements, which is what keeps the wire identical
        // when a big endian machine swaps the copying fast path for the per element path
        var autoAllocator = new Allocator();
        arrayConverter.EncodeAuto(ref autoAllocator, items);
        var data = autoAllocator.AsSpan();
        var count = Converter.Decode(ref data);
        Assert.Equal(payload.Length, count);
        Assert.Equal(payload, data.ToArray());
    }
}
