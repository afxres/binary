namespace Mikodev.Binary.Tests.Contexts;

using Mikodev.Binary.Tests.Internal;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using Xunit;

public class ConverterExtensionsInternalTests
{
    private delegate T DecodeBrotliInternal<T>(Converter<T> converter, ReadOnlySpan<byte> source, ArrayPool<byte> arrays);

    private delegate void EncodeBrotliInternal(ReadOnlySpan<byte> source, Span<byte> target, out int bytesWritten);

    private delegate byte[] EncodeBrotliInternal<T>(Converter<T> converter, T item, ArrayPool<byte> pool);

    private class TestArrayPool<T> : ArrayPool<T>
    {
        public List<int> Rented { get; } = new List<int>();

        public List<int> Returned { get; } = new List<int>();

        public override T[] Rent(int minimumLength)
        {
            var result = new T[minimumLength];
            Rented.Add(result.Length);
            return result;
        }

        public override void Return(T[] array, bool clearArray = false)
        {
            Returned.Add(array.Length);
        }
    }

    private static T GetInternalMethod<T>(string methodName) where T : Delegate
    {
        var invoke = typeof(T).GetMethodNotNull("Invoke", BindingFlags.Instance | BindingFlags.Public);
        var parameterTypes = invoke.GetParameters().Select(x => x.ParameterType).ToArray();
        var method = typeof(ConverterExtensions).GetMethods(BindingFlags.Static | BindingFlags.NonPublic).Single(x => x.Name.Contains(methodName) && x.GetParameters().Select(p => p.ParameterType).SequenceEqual(parameterTypes));
        return (T)Delegate.CreateDelegate(typeof(T), method);
    }

    [Fact(DisplayName = "Encode Brotli Exception Test")]
    public void EncodeBrotliInternalExceptionTest()
    {
        var action = GetInternalMethod<EncodeBrotliInternal>("EncodeBrotliInternal");
        var error = Assert.Throws<IOException>(() => action.Invoke(default, default, out _));
        var message = "Brotli encode failed.";
        Assert.Equal(message, error.Message);
    }

    [Fact(DisplayName = "Decode Brotli Empty Data Test")]
    public void DecodeBrotliEmptyDataTest()
    {
        var generator = Generator.CreateDefault();
        var converter = generator.GetConverter<int>();
        var method = typeof(ConverterExtensions).GetMethod("DecodeBrotliInternal", BindingFlags.Static | BindingFlags.NonPublic);
        Assert.NotNull(method);
        var action = (DecodeBrotliInternal<int>)Delegate.CreateDelegate(typeof(DecodeBrotliInternal<int>), method.MakeGenericMethod(typeof(int)));
        var arrays = new TestArrayPool<byte>();
        var error = Assert.Throws<IOException>(() => action.Invoke(converter, Array.Empty<byte>(), arrays));
        var message = $"Brotli decode failed, status: {OperationStatus.NeedMoreData}";
        Assert.Equal(message, error.Message);
        Assert.Equal(arrays.Rented.Count, arrays.Returned.Count);
        Assert.Equal(64 * 1024, arrays.Rented.Single());
        Assert.Equal(64 * 1024, arrays.Returned.Single());
    }

    public static IEnumerable<object[]> DecodeBrotliArrayPoolRentReturnData()
    {
        var limits = 10_000_000;
        yield return new object[] { 0, new int[] { 64 * 1024 } };
        yield return new object[] { 1, new int[] { 64 * 1024 } };
        yield return new object[] { limits, new int[] { 1 << 16, 1 << 17, 1 << 18, 1 << 19, 1 << 20, 1 << 21, 1 << 22, 1 << 23, 1 << 24 } };
    }

    [Theory(DisplayName = "Decode Brotli Array Pool Test")]
    [MemberData(nameof(DecodeBrotliArrayPoolRentReturnData))]
    public void DecodeBrotliArrayPoolTest(int length, int[] rented)
    {
        var source = new byte[length];
        for (var i = 0; i < source.Length; i++)
            source[i] = (byte)i;
        var buffer = new byte[BrotliEncoder.GetMaxCompressedLength(length)];
        var status = BrotliEncoder.TryCompress(source, buffer, out var bytesWritten);
        Assert.True(status);
        var zipped = new ReadOnlySpan<byte>(buffer, 0, bytesWritten).ToArray();

        var generator = Generator.CreateDefault();
        var converter = generator.GetConverter<byte[]>();
        var method = typeof(ConverterExtensions).GetMethod("DecodeBrotliInternal", BindingFlags.Static | BindingFlags.NonPublic);
        Assert.NotNull(method);
        var action = (DecodeBrotliInternal<byte[]>)Delegate.CreateDelegate(typeof(DecodeBrotliInternal<byte[]>), method.MakeGenericMethod(typeof(byte[])));
        var arrays = new TestArrayPool<byte>();
        var result = action.Invoke(converter, zipped, arrays);
        Assert.Equal(source, result);
        Assert.Equal(arrays.Rented.Count, arrays.Returned.Count);
        Assert.Equal(rented, arrays.Rented);
        Assert.Equal(rented, arrays.Returned);
    }

    public static IEnumerable<object[]> EncodeBrotliArrayPoolRentReturnData()
    {
        var limits = 10_000_000;
        yield return new object[] { 0, new int[] { 1 << 20, BrotliEncoder.GetMaxCompressedLength(0) } };
        yield return new object[] { 1, new int[] { 1 << 20, BrotliEncoder.GetMaxCompressedLength(1) } };
        yield return new object[] { limits, new int[] { 1 << 20, BrotliEncoder.GetMaxCompressedLength(limits) } };
    }

    [Theory(DisplayName = "Encode Brotli Array Pool Test")]
    [MemberData(nameof(EncodeBrotliArrayPoolRentReturnData))]
    public void EncodeBrotliArrayPoolTest(int length, int[] rented)
    {
        var source = new byte[length];
        for (var i = 0; i < source.Length; i++)
            source[i] = (byte)i;

        var generator = Generator.CreateDefault();
        var converter = generator.GetConverter<byte[]>();
        var method = typeof(ConverterExtensions).GetMethods(BindingFlags.Static | BindingFlags.NonPublic).Where(x => x.Name is "EncodeBrotliInternal" && x.IsGenericMethod).Single();
        Assert.NotNull(method);
        var action = (EncodeBrotliInternal<byte[]>)Delegate.CreateDelegate(typeof(EncodeBrotliInternal<byte[]>), method.MakeGenericMethod(typeof(byte[])));
        var arrays = new TestArrayPool<byte>();
        var result = action.Invoke(converter, source, arrays);
        Assert.Equal(arrays.Rented.Count, arrays.Returned.Count);
        Assert.Equal(rented, arrays.Rented);
        Assert.Equal(rented, arrays.Returned.Select(x => x).Reverse());
    }

    [Fact(DisplayName = "Decode Brotli Input Overflow Test")]
    public void DecodeBrotliSourceOverflowTest()
    {
        var generator = Generator.CreateDefault();
        var converter = generator.GetConverter<byte[]>();
        var buffer = new byte[0x4000_0000];
        var error = Assert.Throws<OverflowException>(() => ConverterExtensions.DecodeBrotli(converter, buffer));
        Assert.Equal(new OverflowException().Message, error.Message);
    }

    [Fact(DisplayName = "Decode Brotli Output Overflow Test")]
    public void DecodeBrotliTargetOverflowTest()
    {
        var source =
            """
            CBFFFF3F002400E2B14072EFFFF8FFFF078004401C1680EEFD1FFFFFFF00900088C302D0BDFFE3FF
            FF1F001200715800BAF77FFCFFFF034002200E0B40F7FE8FFFFF7F004800C46101E8DEFFF1FFFF0F
            000980382C00DDFB3FFEFFFF012001108705A07BFFC7FFFF3F002400E2B00074EFFFF8FFFF078004
            401C1680EEFD1FFFFFFF00900088C302D0BDFFE3FFFF1F001200715800BAF77FFCFFFF034002200E
            0B40F7FE8FFFFF7F004800C46101E8DEFFF1FFFF0F000980382C00DDFB3FFEFFFF012001108705A0
            7BFFC7FFFF3F002400E2B00074EFFFF8FFFF078004401C1680EEFD1FFFFFFF00900088C302D0BDFF
            E3FFFF1F001200715800BAF77FFCFFFF034002200E0B40F7FE8FFFFF7F004800C46101E8DEFFF1FF
            FF0F000980382C00DDFB3FFEFFFF012001108705A07BFFC7FFFF3F002400E2B00074EFFFF8FFFF07
            8004401C1680EEFD1FFFFFFF00900088C302D0BDFFE3FFFF1F001200715800BAF77FFCFFFF034002
            200E0B40F7FE8FFFFF7F004800C46101E8DEFFF1FFFF0F000980382C00DDFB3FFEFFFF0120011087
            05A07BFFC7FFFF3F002400E2B00074EFFFF8FFFF078004401C1680EEFD1FFFFFFF00900088C302D0
            BDFFE3FFFF1F001200715800BAF77FFCFFFF034002200E0B40F7FE8FFFFF7F004800C46101E8DEFF
            F1FFFF0F000980382C00DDFB3FFEFFFF012001108705A07BFFC7FFFF3F002400E2B00074EFFFF8FF
            FF078004401C1680EEFD1FFFFFFF00900088C302D0BDFFE3FFFF1F001200715800BAF77FFCFFFF03
            4002200E0B40F7FE8FFFFF7F004800C46101E8DEFFF1FFFF0F000980382C00DDFB3FFEFFFF012001
            108705A07BFFC7FFFF3F002400E2B00074EFFFF8FFFF078004401C1680EEFD1FFFFFFF00900088C3
            02D0BDFFE3FFFF1F001200715800BAF77FFCFFFF034002200E0B40F7FE8FFFFF7F004800C46101E8
            DEFFF1FFFF0F000980382C00DDFB3FFEFFFF012001108705A07BFFC7FFFF3F002400E2B00074EFFF
            F8FFFF078004401C1680EEFD1FFFFFFF00900088C302D0BDFFE3FFFF1F001200715800BAF77FFCFF
            FF034002200E0B40F7FE8FFFFF7F004800C46101E8DEFFF1FFFF0F000980382C00DDFB3FFEFFFF01
            2001108705A07BFFC7FFFF3F002400E2B00074EFFFF8FFFF078004401C1680EEFD1FFFFFFF009000
            88C302D0BDFFE3FFFF1F001200715800BAF77FFCFFFF034002200E0B40F7FE8FFFFF7F004800C461
            01E8DEFFF1FFFF0F000980382C00DDFB3FFEFFFF012001108705A07BFFC7FFFF3F002400E2B00074
            EFFFF8FFFF078004401C1680EEFD1FFFFFFF00900088C302D0BDFFE3FFFF1F001200715800BAF77F
            FCFFFF034002200E0B40F7FE8FFFFF7F004800C46101E8DEFFF1FFFF0F000980382C00DDFB3FFEFF
            FF012001108705A07BFFC7FFFF3F002400E2B00074EFFFF8FFFF078004401C1680EEFD1FFFFFFF00
            900088C302D0BDFFE3FFFF1F001200715800BAF77FFCFFFF034002200E0B40F7FE8FFFFF7F004800
            C46101E8DEFFF1FFFF0F000980382C00DDFB3FFEFFFF012001108705A07BFFC7FFFF3F002400E2B0
            0074EFFFF8FFFF078004401C1680EEFD1FFFFFFF00900088C302D0BDFFE3FFFF1F001200715800BA
            F77FFCFFFF034002200E0B40F7FE8FFFFF7F004800C46101E8DEFFF1FFFF0F000980382C00DDFB3F
            FEFFFF012001108705A07BFFC7FFFF3F002400E2B00074EFFFF8FFFF078004401C1680EEFD1FFFFF
            FF00900088C302D0BDFFE3FFFF1F001200715800BAF77FFCFFFF034002200E0B40F7FE8FFFFF7F00
            4800C46101E8DEFFF1FFFF0F000980382C00DDFB3FFEFFFF012001108705A07BFFC7FFFF3F002400
            E2B00074EFFFF8FFFF078004401C1680EEFD1FFFFFFF00900088C302D0BDFFE3FFFF1F0012007158
            00BAF77FFCFFFF034002200E0B40F7FE8FFFFF7F004800C46101E8DEFFF1FFFF0F000980382C00DD
            FB3FFEFFFF012001108705A07BFFC7FFFF3F002400E2B00074EFFFF8FFFF078004401C1680EEFD1F
            FFFFFF00900088C302D0BDFFE3FFFF1F001200715800BAF77FFCFFFF034002200E0B40F7FE8FFFFF
            7F004800C46101E8DEFFF1FFFF0F000980382C00DDFB3FFEFFFF012001108705A07BFFC7FFFF3F00
            2400E2B00074EFFFF8FFFF078004401C1680EEFD1FFFFFFF00900088C302D0BDFFE3FFFF1F001200
            715800BAF77FFCFFFF034002200E0B40F7FE8FFFFF7F004800C46101E8DEFFF1FFFF0F000980382C
            00DDFB3FFEFFFF012001108705A07BFF0700800003
            """;
        var buffer = Convert.FromHexString(source.ReplaceLineEndings(string.Empty));
        Assert.Equal(1621, buffer.Length);
        var generator = Generator.CreateDefault();
        var converter = generator.GetConverter<byte[]>();
        var error = Assert.Throws<OverflowException>(() => ConverterExtensions.DecodeBrotli(converter, buffer));
        Assert.Equal(new OverflowException().Message, error.Message);
    }
}
