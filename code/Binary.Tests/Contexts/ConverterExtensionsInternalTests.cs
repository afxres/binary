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
        public List<T[]> Rented { get; } = new List<T[]>();

        public List<T[]> Returned { get; } = new List<T[]>();

        public override T[] Rent(int minimumLength)
        {
            var result = new T[minimumLength];
            Rented.Add(result);
            return result;
        }

        public override void Return(T[] array, bool clearArray = false)
        {
            Returned.Add(array);
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
        Assert.Equal(64 * 1024, arrays.Rented.Single().Length);
        Assert.Equal(64 * 1024, arrays.Returned.Single().Length);
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
        Assert.Equal(rented, arrays.Rented.Select(x => x.Length));
        Assert.Equal(rented, arrays.Returned.Select(x => x.Length));
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
        Assert.Equal(rented, arrays.Rented.Select(x => x.Length));
        Assert.Equal(rented, arrays.Returned.Select(x => x.Length).Reverse());
    }
}
