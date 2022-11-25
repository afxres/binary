namespace Mikodev.Binary.Tests.Features;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Xunit;

public class EndiannessTests
{
    private delegate T Decode<T>(ref byte source);

    private delegate void Encode<T>(ref byte target, T item);

    public enum Enum32 : int { }

    public enum Enum64 : long { }

    public enum EnumU32 : uint { }

    public enum EnumU64 : ulong { }

    [StructLayout(LayoutKind.Explicit, Size = 32)]
    public struct Block32 { }

    [StructLayout(LayoutKind.Explicit, Size = 64)]
    public struct Block64 { }

    public static IEnumerable<object[]> EnumData => new List<object[]>
    {
        new object[] { 4, (Enum32)0 },
        new object[] { 8, (Enum64)long.MinValue },
        new object[] { 4, (EnumU32)uint.MaxValue },
        new object[] { 8, (EnumU64)ulong.MaxValue },
    };

    public static IEnumerable<object[]> NumberData => new List<object[]>
    {
        new object[] { 1, (byte)1 },
        new object[] { 1, (sbyte)-1 },
        new object[] { 2, (short)-3 },
        new object[] { 2, (ushort)5 },
        new object[] { 2, Half.MinValue },
        new object[] { 2, Half.MaxValue },
        new object[] { 2, Half.NaN },
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
        var generator = Generator.CreateDefault();
        var converter = generator.GetConverter<T>();
        var converterType = converter.GetType();
        Assert.Equal("NativeEndianConverter`1", converterType.Name);
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
        var creatorType = typeof(IConverter).Assembly.GetTypes().Single(x => x.Name is "DetectEndianConverterCreator");
        var creatorInvokeMethod = creatorType.GetMethods(BindingFlags.Static | BindingFlags.NonPublic).Single(x => x.Name.Contains("Invoke"));
        var creatorInvokeFunctor = (Func<Type, bool, IConverter>)Delegate.CreateDelegate(typeof(Func<Type, bool, IConverter>), creatorInvokeMethod);
        var converter = (Converter<T>)creatorInvokeFunctor.Invoke(typeof(T), true);
        var converterType = converter.GetType();
        Assert.Equal("NativeEndianConverter`1", converterType.Name);
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
        var creatorType = typeof(IConverter).Assembly.GetTypes().Single(x => x.Name is "DetectEndianConverterCreator");
        var creatorInvokeMethod = creatorType.GetMethods(BindingFlags.Static | BindingFlags.NonPublic).Single(x => x.Name.Contains("Invoke"));
        var creatorInvokeFunctor = (Func<Type, bool, IConverter>)Delegate.CreateDelegate(typeof(Func<Type, bool, IConverter>), creatorInvokeMethod);
        var converter = (Converter<T>)creatorInvokeFunctor.Invoke(typeof(T), false);
        var converterType = converter.GetType();
        Assert.Equal("LittleEndianConverter`1", converterType.Name);
        Assert.Equal(length, converter.Length);
        var buffer = converter.Encode(data);
        var result = converter.Decode(buffer);
        Assert.Equal(data, result);
    }

    public static IEnumerable<object[]> InvalidBlockData => new List<object[]>
    {
        new object[] { 32, default(Block32) },
        new object[] { 64, default(Block64) },
    };

    [Theory(DisplayName = "Internal Little Endian Converter Invalid Type")]
    [MemberData(nameof(InvalidBlockData))]
    public void InternalLittleEndianConverterInvalidType<T>(int length, T data)
    {
        Assert.Equal(length, Unsafe.SizeOf<T>());
        var converterOpenType = typeof(IConverter).Assembly.GetTypes().Single(x => x.FullName?.EndsWith("LittleEndianConverter`1+Functions") is true);
        var converterType = converterOpenType.MakeGenericType(typeof(T));
        var decodeFunctor = (Decode<T>)Delegate.CreateDelegate(typeof(Decode<T>), converterType, "Decode");
        var encodeFunctor = (Encode<T>)Delegate.CreateDelegate(typeof(Encode<T>), converterType, "Encode");

        // Invalid null reference, do not dereference!!!
        var alpha = Assert.Throws<NotSupportedException>(() => decodeFunctor.Invoke(ref Unsafe.NullRef<byte>()));
        Assert.Equal(new NotSupportedException().Message, alpha.Message);

        // Invalid null reference, do not dereference!!!
        var bravo = Assert.Throws<NotSupportedException>(() => encodeFunctor.Invoke(ref Unsafe.NullRef<byte>(), data));
        Assert.Equal(new NotSupportedException().Message, bravo.Message);
    }
}
