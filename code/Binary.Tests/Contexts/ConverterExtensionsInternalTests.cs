namespace Mikodev.Binary.Tests.Contexts;

using Mikodev.Binary.Tests.Internal;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xunit;

public class ConverterExtensionsInternalTests
{
    private delegate T DecodeBrotliInternal<out T>(ReadOnlySpan<byte> source, ArrayPool<byte> arrays);

    private delegate void EncodeBrotliInternal(ReadOnlySpan<byte> source, Span<byte> target, out int bytesWritten);

    private delegate byte[] EncodeBrotliInternal<T>(AllocatorAction<T> action, T item, ArrayPool<byte> pool);

    private abstract class FakeAbstractArrayPool<T> : ArrayPool<T>
    {
        public List<int> RentedMinimumLengths { get; } = [];

        public List<T[]> RentedArrays { get; } = [];

        public List<T[]> ReturnedArrays { get; } = [];

        public abstract T[] CreateArray(int minimumLength);

        public sealed override T[] Rent(int minimumLength)
        {
            var result = CreateArray(minimumLength);
            RentedMinimumLengths.Add(minimumLength);
            RentedArrays.Add(result);
            return result;
        }

        public sealed override void Return(T[] array, bool clearArray = false)
        {
            Assert.False(clearArray);
            // always clear array, prevent reuse after return
            Array.Clear(array);
            ReturnedArrays.Add(array);
        }
    }

    private class TestArrayPool<T> : FakeAbstractArrayPool<T>
    {
        public override T[] CreateArray(int minimumLength)
        {
            return new T[minimumLength];
        }
    }

    private static T GetInternalMethod<T>(string methodName) where T : Delegate
    {
        var invoke = typeof(T).GetMethodNotNull("Invoke", BindingFlags.Instance | BindingFlags.Public);
        var parameterTypes = invoke.GetParameters().Select(x => x.ParameterType).ToArray();
        var method = typeof(ConverterExtensions).GetMethods(BindingFlags.Static | BindingFlags.NonPublic).Single(x => x.Name.Contains(methodName) && x.GetParameters().Select(p => p.ParameterType).SequenceEqual(parameterTypes));
        return (T)Delegate.CreateDelegate(typeof(T), method);
    }

    private static DecodeBrotliInternal<T> GetDecodeBrotliInternalMethod<T>(Converter<T> converter)
    {
        var decodeDelegateDefinition = typeof(IConverter).Assembly.GetTypes().Single(x => x.Name is "DecodeReadOnlyDelegate`1");
        Assert.NotNull(decodeDelegateDefinition);
        var method = typeof(ConverterExtensions).GetMethod("DecodeBrotliInternal", BindingFlags.Static | BindingFlags.NonPublic);
        Assert.NotNull(method);
        var decodeType = decodeDelegateDefinition.MakeGenericType(typeof(T));
        var decode = Delegate.CreateDelegate(decodeType, converter, Converter.GetMethod(converter, "Decode"));
        var span = Expression.Parameter(typeof(ReadOnlySpan<byte>));
        var pool = Expression.Parameter(typeof(ArrayPool<byte>));
        var call = Expression.Call(method.MakeGenericMethod(typeof(T)), [Expression.Constant(decode), span, pool]);
        var lambda = Expression.Lambda<DecodeBrotliInternal<T>>(call, [span, pool]);
        return lambda.Compile();
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
        var action = GetDecodeBrotliInternalMethod(converter);
        var arrays = new TestArrayPool<byte>();
        var error = Assert.Throws<IOException>(() => action.Invoke([], arrays));
        var message = $"Brotli decode failed, status: {OperationStatus.NeedMoreData}";
        Assert.Equal(message, error.Message);
        Assert.Equal(arrays.RentedArrays.Count, arrays.ReturnedArrays.Count);
        Assert.Equal(arrays.RentedMinimumLengths.Count, arrays.ReturnedArrays.Count);
        Assert.Equal(64 * 1024, arrays.RentedArrays.Single().Length);
        Assert.Equal(64 * 1024, arrays.RentedMinimumLengths.Single());
        Assert.Equal(64 * 1024, arrays.ReturnedArrays.Single().Length);
    }

    public static IEnumerable<object[]> DecodeBrotliArrayPoolRentReturnData()
    {
        var limits = 10_000_000;
        yield return [0, new int[] { 64 * 1024 }];
        yield return [1, new int[] { 64 * 1024 }];
        yield return [limits, new int[] { 1 << 16, 1 << 17, 1 << 18, 1 << 19, 1 << 20, 1 << 21, 1 << 22, 1 << 23, 1 << 24 }];
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
        var action = GetDecodeBrotliInternalMethod(converter);
        var arrays = new TestArrayPool<byte>();
        var result = action.Invoke(zipped, arrays);
        Assert.Equal(source, result);
        Assert.Equal(arrays.RentedArrays.Count, arrays.ReturnedArrays.Count);
        Assert.Equal(arrays.RentedMinimumLengths.Count, arrays.ReturnedArrays.Count);
        Assert.Equal(rented, arrays.RentedArrays.Select(x => x.Length).ToList());
        Assert.Equal(rented, arrays.ReturnedArrays.Select(x => x.Length).ToList());
    }

    [Fact(DisplayName = "Decode Brotli Force Clear Array After Return To Array Pool Test")]
    public void DecodeBrotliForceClearAfterReturnToArrayPoolTest()
    {
        var length = 256 * 1024;
        var source = new byte[length];
        for (var i = 0; i < source.Length; i++)
            source[i] = (byte)i;
        var buffer = new byte[BrotliEncoder.GetMaxCompressedLength(length)];
        var status = BrotliEncoder.TryCompress(source, buffer, out var bytesWritten);
        Assert.True(status);
        var zipped = new ReadOnlySpan<byte>(buffer, 0, bytesWritten).ToArray();

        var generator = Generator.CreateDefault();
        var converter = generator.GetConverter<byte[]>();
        var action = GetDecodeBrotliInternalMethod(converter);
        var arrays = new TestArrayPool<byte>();
        var result = action.Invoke(zipped, arrays);
        Assert.Equal(source, result);
        Assert.Equal(3, arrays.RentedMinimumLengths.Count);
        Assert.Equal(3, arrays.RentedArrays.Count);
        Assert.Equal(3, arrays.ReturnedArrays.Count);

        var expectedLengths = new int[] { 64 * 1024, 128 * 1024, 256 * 1024 };
        for (var i = 0; i < expectedLengths.Length; i++)
        {
            var expectedLength = expectedLengths[i];
            var rented = arrays.RentedArrays[i];
            var returned = arrays.ReturnedArrays[i];
            Assert.Equal(expectedLength, arrays.RentedMinimumLengths[i]);
            Assert.Equal(expectedLength, rented.Length);
            Assert.Equal(expectedLength, returned.Length);
            Assert.True(ReferenceEquals(rented, returned));

            // ensure all returned arrays are cleared
            var data = 0L;
            foreach (var x in returned)
                data += x;
            Assert.Equal(0L, data);
        }
    }

    public static IEnumerable<object[]> EncodeBrotliArrayPoolRentReturnData()
    {
        var limits = 10_000_000;
        yield return [0, new int[] { 1 << 16, BrotliEncoder.GetMaxCompressedLength(0) }];
        yield return [1, new int[] { 1 << 16, BrotliEncoder.GetMaxCompressedLength(1) }];
        yield return [limits, new int[] { 1 << 16, BrotliEncoder.GetMaxCompressedLength(limits) }];
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
        var method = typeof(ConverterExtensions).GetMethods(BindingFlags.Static | BindingFlags.NonPublic).Single(x => x.Name is "EncodeBrotliInternal" && x.IsGenericMethod);
        Assert.NotNull(method);
        var action = (EncodeBrotliInternal<byte[]>)Delegate.CreateDelegate(typeof(EncodeBrotliInternal<byte[]>), method.MakeGenericMethod(typeof(byte[])));
        var arrays = new TestArrayPool<byte>();
        var result = action.Invoke(converter.Encode, source, arrays);
        var actual = new byte[length];
        var status = BrotliDecoder.TryDecompress(result, actual, out var bytesWritten);
        Assert.True(status);
        Assert.Equal(length, bytesWritten);
        Assert.Equal(source, actual);
        Assert.Equal(arrays.RentedArrays.Count, arrays.ReturnedArrays.Count);
        Assert.Equal(arrays.RentedMinimumLengths.Count, arrays.ReturnedArrays.Count);
        Assert.Equal(rented, arrays.RentedArrays.Select(x => x.Length).ToList());
        Assert.Equal(rented, arrays.ReturnedArrays.Select(x => x.Length).Reverse().ToList());
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

    private class TestInvalidZeroSizeReturnsArrayPool<T> : FakeAbstractArrayPool<T>
    {
        public override T[] CreateArray(int minimumLength)
        {
            return [];
        }
    }

    [Fact(DisplayName = "Decode Brotli With Invalid Zero Size Returns Array Pool")]
    public void DecodeBrotliWithInvalidZeroSizeReturnsArrayPoolTest()
    {
        var length = 256;
        var source = new byte[length];
        for (var i = 0; i < source.Length; i++)
            source[i] = (byte)i;
        var buffer = new byte[BrotliEncoder.GetMaxCompressedLength(length)];
        var status = BrotliEncoder.TryCompress(source, buffer, out var bytesWritten);
        Assert.True(status);
        var zipped = new ReadOnlySpan<byte>(buffer, 0, bytesWritten).ToArray();

        var arrays = new TestInvalidZeroSizeReturnsArrayPool<byte>();
        var generator = Generator.CreateDefault();
        var converter = generator.GetConverter<byte[]>();
        var action = GetDecodeBrotliInternalMethod(converter);
        var error = Assert.Throws<ArgumentOutOfRangeException>(() => action.Invoke(zipped, arrays));
        Assert.Equal(new ArgumentOutOfRangeException().Message, error.Message);
        Assert.Equal(64 * 1024, arrays.RentedMinimumLengths.Single());
        Assert.Equal(0, arrays.ReturnedArrays.Select(x => x.Length).Single());
    }

    private class TestDoubleSizeReturnsArrayPool<T> : FakeAbstractArrayPool<T>
    {
        public override T[] CreateArray(int minimumLength)
        {
            return new T[minimumLength * 2];
        }
    }

    [Fact(DisplayName = "Decode Brotli With Double Size Returns Array Pool")]
    public void DecodeBrotliWithDoubleSizeReturnsArrayPoolTest()
    {
        var length = 1024 * 1024;
        var source = new byte[length];
        for (var i = 0; i < source.Length; i++)
            source[i] = (byte)i;
        var buffer = new byte[BrotliEncoder.GetMaxCompressedLength(length)];
        var status = BrotliEncoder.TryCompress(source, buffer, out var bytesWritten);
        Assert.True(status);
        var zipped = new ReadOnlySpan<byte>(buffer, 0, bytesWritten).ToArray();

        var arrays = new TestDoubleSizeReturnsArrayPool<byte>();
        var generator = Generator.CreateDefault();
        var converter = generator.GetConverter<byte[]>();
        var action = GetDecodeBrotliInternalMethod(converter);
        var result = action.Invoke(zipped, arrays);
        Assert.Equal(source, result);
        Assert.Equal(new[] { 64 * 1024, 256 * 1024, 1024 * 1024 }, arrays.RentedMinimumLengths);
        Assert.Equal(new[] { 128 * 1024, 512 * 1024, 2 * 1024 * 1024 }, arrays.RentedArrays.Select(x => x.Length).ToList());
        Assert.Equal(new[] { 128 * 1024, 512 * 1024, 2 * 1024 * 1024 }, arrays.ReturnedArrays.Select(x => x.Length).ToList());
    }
}
