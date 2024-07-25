namespace Mikodev.Binary.Tests.Converters.Primitive;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Xunit;

public class NativeEndianOrLittleEndianInfoTests
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

    public static IEnumerable<object[]> EnumData =>
    [
        [4, (Enum32)0],
        [8, (Enum64)long.MinValue],
        [4, (EnumU32)uint.MaxValue],
        [8, (EnumU64)ulong.MaxValue],
    ];

    public static IEnumerable<object[]> NumberData =>
    [
        [1, (byte)1],
        [1, (sbyte)-1],
        [2, (short)-3],
        [2, (ushort)5],
        [2, Half.MinValue],
        [2, Half.MaxValue],
        [2, Half.NaN],
        [4, 0],
        [8, 0L],
        [4, 0U],
        [8, 0UL],
        [4, 2.0F],
        [8, 3.0],
    ];

    public static IEnumerable<object[]> IndexData()
    {
        yield return new object[] { 4, Index.Start };
        yield return new object[] { 4, Index.End };
        yield return new object[] { 4, Index.FromStart(1) };
        yield return new object[] { 4, Index.FromEnd(1) };
    }

    [Theory(DisplayName = "Fallback Converter Info")]
    [MemberData(nameof(EnumData))]
    [MemberData(nameof(NumberData))]
    [MemberData(nameof(IndexData))]
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
    [MemberData(nameof(IndexData))]
    public void InternalNativeEndianConverterInfo<T>(int length, T data)
    {
        var creatorName = typeof(T).IsEnum ? "DetectEndianEnumConverterCreator" : "DetectEndianConverterCreator";
        var creatorType = typeof(IConverter).Assembly.GetTypes().Single(x => x.Name == creatorName);
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
    [MemberData(nameof(IndexData))]
    public void InternalLittleEndianConverterInfo<T>(int length, T data)
    {
        var creatorName = typeof(T).IsEnum ? "DetectEndianEnumConverterCreator" : "DetectEndianConverterCreator";
        var creatorType = typeof(IConverter).Assembly.GetTypes().Single(x => x.Name == creatorName);
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

    public static IEnumerable<object[]> InvalidBlockData =>
    [
        [32, default(Block32)],
        [64, default(Block64)],
    ];

    [Theory(DisplayName = "Internal Little Endian Converter Invalid Type")]
    [MemberData(nameof(InvalidBlockData))]
    public void InternalLittleEndianConverterInvalidType<T>(int length, T data)
    {
        Assert.Equal(length, Unsafe.SizeOf<T>());
        var converterOpenType = typeof(IConverter).Assembly.GetTypes().Single(x => x.FullName?.EndsWith("LittleEndianConverter`1+Functions") is true);
        var converterType = converterOpenType.MakeGenericType(typeof(T));
        var decodeFunctor = (Decode<T>)Delegate.CreateDelegate(typeof(Decode<T>), converterType, "Decode");
        var encodeFunctor = (Encode<T>)Delegate.CreateDelegate(typeof(Encode<T>), converterType, "Encode");

        // invalid null reference, do not dereference!!!
        var alpha = Assert.Throws<NotSupportedException>(() => decodeFunctor.Invoke(ref Unsafe.NullRef<byte>()));
        Assert.Equal(new NotSupportedException().Message, alpha.Message);

        // invalid null reference, do not dereference!!!
        var bravo = Assert.Throws<NotSupportedException>(() => encodeFunctor.Invoke(ref Unsafe.NullRef<byte>(), data));
        Assert.Equal(new NotSupportedException().Message, bravo.Message);
    }
}
