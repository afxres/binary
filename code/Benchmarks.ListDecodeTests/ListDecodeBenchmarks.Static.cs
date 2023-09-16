namespace Mikodev.Binary.Benchmarks.ListDecodeTests;

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

public partial class ListDecodeBenchmarks
{
    [InlineArray(32)]
    private struct InlineBuffer<T>
    {
#pragma warning disable IDE0044 // Add readonly modifier
#pragma warning disable IDE0051 // Remove unused private members
        private T data;
#pragma warning restore IDE0051 // Remove unused private members
#pragma warning restore IDE0044 // Add readonly modifier
    }

    private static List<T> Decode<T>(Converter<T> converter, ReadOnlySpan<byte> span)
    {
        if (span.Length is 0)
            return new List<T>();
        const int FallbackCapacity = 8;
        var intent = span;
        var result = new List<T>(FallbackCapacity);
        while (intent.Length is not 0)
            result.Add(converter.DecodeAuto(ref intent));
        return result;
    }

    private static List<T> DecodeStackBased<T>(Converter<T> converter, ReadOnlySpan<byte> span)
    {
        if (span.Length is 0)
            return new List<T>();
        var buffer = new InlineBuffer<T>();
        var target = (Span<T>)buffer;
        var cursor = 0;
        var intent = span;
        while (intent.Length is not 0)
            target[cursor++] = converter.DecodeAuto(ref intent);
        var result = new List<T>();
        CollectionsMarshal.SetCount(result, cursor);
        target.Slice(0, cursor).CopyTo(CollectionsMarshal.AsSpan(result));
        return result;
    }

    private static List<T> DecodeRecursively<T>(Converter<T> converter, ReadOnlySpan<byte> span)
    {
        if (span.Length is 0)
            return new List<T>();
        return DecodeRecursively(converter, ref span, 0);
    }

    private static List<T> DecodeRecursively<T>(Converter<T> converter, ref ReadOnlySpan<byte> span, int cursor)
    {
        if (span.Length is 0)
        {
            var result = new List<T>();
            CollectionsMarshal.SetCount(result, cursor);
            return result;
        }
        else
        {
            var intent = converter.DecodeAuto(ref span);
            var result = DecodeRecursively(converter, ref span, cursor + 1);
            result[cursor] = intent;
            return result;
        }
    }
}
