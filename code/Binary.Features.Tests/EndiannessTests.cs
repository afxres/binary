namespace Mikodev.Binary.Features.Tests;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;

public class EndiannessTests
{
    public enum Enum32 : int { }

    public enum Enum64 : long { }

    public enum EnumU32 : uint { }

    public enum EnumU64 : ulong { }

    public static IEnumerable<object[]> EnumData => new List<object[]>
    {
        new object[] { 4, (Enum32)0 },
        new object[] { 8, (Enum64)long.MinValue },
        new object[] { 4, (EnumU32)uint.MaxValue },
        new object[] { 8, (EnumU64)ulong.MaxValue },
    };

    public static IEnumerable<object[]> NumberData => new List<object[]>
    {
        new object[] { 4, 0 },
        new object[] { 8, 0L },
        new object[] { 4, 0U },
        new object[] { 8, 0UL },
        new object[] { 4, 2.0F },
        new object[] { 8, 3.0 },
    };

    [Theory(DisplayName = "Fallback Converter Info")]
    [MemberData(nameof(EnumData))]
    [MemberData(nameof(NumberData))]
    public void FallbackConverterBasicInfo<T>(int length, T data)
    {
        var generator = Generator
            .CreateDefaultBuilder()
            .AddPreviewFeaturesConverterCreators()
            .Build();
        var converter = generator.GetConverter<T>();
        var converterType = converter.GetType();
        Assert.Equal("RawConverter`2", converterType.Name);
        Assert.Contains("NativeEndianRawConverter`1", converterType.FullName);
        Assert.Equal(length, converter.Length);
        var buffer = converter.Encode(data);
        var result = converter.Decode(buffer);
        Assert.Equal(data, result);
    }

    [Theory(DisplayName = "Internal Native Endian Converter Info")]
    [MemberData(nameof(EnumData))]
    [MemberData(nameof(NumberData))]
    public void InternalNativeEndianConverterInfo<T>(int length, T data)
    {
        var creatorType = typeof(IConverter).Assembly.GetTypes().Single(x => x.Name is "RawConverterCreator");
        var creatorInvokeMethod = creatorType.GetMethods(BindingFlags.Static | BindingFlags.NonPublic).Single(x => x.Name.Contains("Invoke"));
        var creatorInvokeFunctor = (Func<Type, bool, IConverter>)Delegate.CreateDelegate(typeof(Func<Type, bool, IConverter>), creatorInvokeMethod);
        var converter = creatorInvokeFunctor.Invoke(typeof(T), true);
        var converterType = converter.GetType();
        Assert.Equal("RawConverter`2", converterType.Name);
        Assert.Contains("NativeEndianRawConverter`1", converterType.FullName);
        Assert.Equal(length, converter.Length);
        var buffer = converter.Encode(data);
        var result = converter.Decode(buffer);
        Assert.Equal(data, result);
    }

    [Theory(DisplayName = "Internal Little Endian Converter Info")]
    [MemberData(nameof(EnumData))]
    [MemberData(nameof(NumberData))]
    public void InternalLittleEndianConverterInfo<T>(int length, T data)
    {
        var creatorType = typeof(IConverter).Assembly.GetTypes().Single(x => x.Name is "RawConverterCreator");
        var creatorInvokeMethod = creatorType.GetMethods(BindingFlags.Static | BindingFlags.NonPublic).Single(x => x.Name.Contains("Invoke"));
        var creatorInvokeFunctor = (Func<Type, bool, IConverter>)Delegate.CreateDelegate(typeof(Func<Type, bool, IConverter>), creatorInvokeMethod);
        var converter = creatorInvokeFunctor.Invoke(typeof(T), false);
        var converterType = converter.GetType();
        Assert.Equal("RawConverter`2", converterType.Name);
        Assert.Contains("LittleEndianRawConverter`1", converterType.FullName);
        Assert.Equal(length, converter.Length);
        var buffer = converter.Encode(data);
        var result = converter.Decode(buffer);
        Assert.Equal(data, result);
    }
}
