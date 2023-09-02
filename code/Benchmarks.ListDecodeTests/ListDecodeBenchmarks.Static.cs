namespace Mikodev.Binary.Benchmarks.ListDecodeTests;

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public partial class ListDecodeBenchmarks
{
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
