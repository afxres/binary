namespace Mikodev.Binary.Benchmarks.ListDecodeTests;

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

public partial class ListDecodeBenchmarks
{
    private const int MaxLevels = 32;

    private const int NewLength = 64;

    [InlineArray(MaxLevels)]
    private struct InlineBuffer<T>
    {
#pragma warning disable IDE0044 // Add readonly modifier
#pragma warning disable IDE0051 // Remove unused private members
        private T item;
#pragma warning restore IDE0051 // Remove unused private members
#pragma warning restore IDE0044 // Add readonly modifier
    }

    private static List<E> GetPartialList<E>(Converter<E> converter, ref ReadOnlySpan<byte> span)
    {
        var buffer = new InlineBuffer<E>();
        var target = (Span<E>)buffer;
        var cursor = 0;
        while (cursor is not MaxLevels && span.Length is not 0)
            target[cursor++] = converter.DecodeAuto(ref span);
        var result = new List<E>(span.Length is 0 ? cursor : NewLength);
        CollectionsMarshal.SetCount(result, cursor);
        target.Slice(0, cursor).CopyTo(CollectionsMarshal.AsSpan(result));
        return result;
    }

    private static List<E> GetListRecursively<E>(Converter<E> converter, ref ReadOnlySpan<byte> span, int cursor)
    {
        if (cursor is MaxLevels || span.Length is 0)
        {
            var result = new List<E>(span.Length is 0 ? cursor : NewLength);
            CollectionsMarshal.SetCount(result, cursor);
            return result;
        }
        else
        {
            var intent = converter.DecodeAuto(ref span);
            var result = GetListRecursively(converter, ref span, cursor + 1);
            result[cursor] = intent;
            return result;
        }
    }

    private static List<E> GetList<E>(Converter<E> converter, ref ReadOnlySpan<byte> span, int capacity)
    {
        var result = new List<E>(capacity);
        while (span.Length is not 0)
            result.Add(converter.DecodeAuto(ref span));
        return result;
    }

    private static List<T> Decode<T>(Converter<T> converter, ReadOnlySpan<byte> span, int capacity)
    {
        if (span.Length is 0)
            return [];
        return GetList(converter, ref span, capacity);
    }

    private static List<T> DecodeStackBased<T>(Converter<T> converter, ReadOnlySpan<byte> span)
    {
        if (span.Length is 0)
            return [];
        return GetPartialList(converter, ref span);
    }

    private static List<T> DecodeRecursively<T>(Converter<T> converter, ReadOnlySpan<byte> span)
    {
        if (span.Length is 0)
            return [];
        return GetListRecursively(converter, ref span, 0);
    }
}
