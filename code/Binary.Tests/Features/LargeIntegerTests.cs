namespace Mikodev.Binary.Tests.Features;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Reflection;
using Xunit;

#if NET7_0_OR_GREATER
public class LargeIntegerTests
{
    private static Converter<T> CreateConverter<T>(bool isNative)
    {
        var creatorType = typeof(IConverter).Assembly.GetTypes().Single(x => x.Name is "RawConverterCreator");
        var creatorInvokeMethod = creatorType.GetMethods(BindingFlags.Static | BindingFlags.NonPublic).Single(x => x.Name.Contains("Invoke"));
        var creatorInvokeFunctor = (Func<Type, bool, IConverter>)Delegate.CreateDelegate(typeof(Func<Type, bool, IConverter>), creatorInvokeMethod);
        var converter = (Converter<T>)creatorInvokeFunctor.Invoke(typeof(T), isNative);
        return converter;
    }

    private static void BinaryIntegerBasicTest<T>(int size, T item, Converter<T> converter, string rawConverterTypeName) where T : IBinaryInteger<T>
    {
        var converterType = converter.GetType();
        Assert.Equal("RawConverter`2", converterType.Name);
        var genericArguments = converterType.GetGenericArguments();
        var rawConverterType = genericArguments.Last();
        Assert.Equal(rawConverterTypeName, rawConverterType.Name);
        Assert.Equal(size, converter.Length);
        var buffer = converter.Encode(item);
        var result = converter.Decode(buffer);
        Assert.Equal(item, result);
        var expectedBuffer = new byte[size];
        var flag = item.TryWriteLittleEndian(expectedBuffer, out var bytesWritten);
        Assert.True(flag);
        Assert.Equal(size, bytesWritten);
        Assert.Equal(expectedBuffer, buffer);
    }

    public static IEnumerable<object[]> Int128Data => new List<object[]>
    {
        new object[] { 16, default(Int128) },
        new object[] { 16, Int128.MinValue },
        new object[] { 16, Int128.MaxValue },
        new object[] { 16, Int128.Zero },
        new object[] { 16, Int128.One },
        new object[] { 16, Int128.NegativeOne },
        new object[] { 16, Int128.Parse("11223344_55667788_99AABBCC_DDEEFF00".Replace("_", string.Empty), NumberStyles.HexNumber) },
    };

    [Theory(DisplayName = "Int128 Converter Basic Info")]
    [MemberData(nameof(Int128Data))]
    public void Int128EncodeDecode(int size, Int128 item)
    {
        var origin = Generator.CreateDefault().GetConverter<Int128>();
        var native = CreateConverter<Int128>(true);
        var little = CreateConverter<Int128>(false);
        BinaryIntegerBasicTest(size, item, origin, "NativeEndianRawConverter`1");
        BinaryIntegerBasicTest(size, item, native, "NativeEndianRawConverter`1");
        BinaryIntegerBasicTest(size, item, little, "LittleEndianRawConverter`1");
    }

    public static IEnumerable<object[]> UInt128Data => new List<object[]>
    {
        new object[] { 16, default(UInt128) },
        new object[] { 16, UInt128.MinValue },
        new object[] { 16, UInt128.MaxValue },
        new object[] { 16, UInt128.Zero },
        new object[] { 16, UInt128.One },
        new object[] { 16, UInt128.Parse("11223344_55667788_99AABBCC_DDEEFF00".Replace("_", string.Empty), NumberStyles.HexNumber) },
    };

    [Theory(DisplayName = "UInt128 Converter Basic Info")]
    [MemberData(nameof(UInt128Data))]
    public void UInt128EncodeDecode(int size, UInt128 item)
    {
        var origin = Generator.CreateDefault().GetConverter<UInt128>();
        var native = CreateConverter<UInt128>(true);
        var little = CreateConverter<UInt128>(false);
        BinaryIntegerBasicTest(size, item, origin, "NativeEndianRawConverter`1");
        BinaryIntegerBasicTest(size, item, native, "NativeEndianRawConverter`1");
        BinaryIntegerBasicTest(size, item, little, "LittleEndianRawConverter`1");
    }
}
#endif
