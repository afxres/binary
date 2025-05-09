namespace Mikodev.Binary.Benchmarks.ListDecodeTests;

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public partial class ListDecodeBenchmarks
{
    private static List<E> GetListRecursively<E>(Converter<E> converter, ref ReadOnlySpan<byte> span, int cursor)
    {
        if (span.Length is 0)
        {
            var result = new List<E>(cursor);
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

    private static List<T> DecodeRecursively<T>(Converter<T> converter, ReadOnlySpan<byte> span)
    {
        if (span.Length is 0)
            return [];
        return GetListRecursively(converter, ref span, 0);
    }
}
