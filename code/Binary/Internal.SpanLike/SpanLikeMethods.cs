namespace Mikodev.Binary.Internal.SpanLike;

using Mikodev.Binary.Internal.Sequence;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;

internal static class SpanLikeMethods
{
    internal static List<E> GetList<E>(Converter<E> converter, ReadOnlySpan<byte> span)
    {
        Debug.Assert(span.Length is not 0);
        var limits = span.Length;
        var length = converter.Length;
        var capacity = SequenceMethods.GetCapacity<E>(limits, length, SequenceMethods.FallbackCapacity);
        var result = new List<E>(capacity);
        var body = span;
        while (body.Length is not 0)
            result.Add(converter.DecodeAuto(ref body));
        return result;
    }

    internal static ImmutableArray<E> GetImmutableArray<E>(Converter<E> converter, ReadOnlySpan<byte> span)
    {
        Debug.Assert(span.Length is not 0);
        var limits = span.Length;
        var length = converter.Length;
        var capacity = SequenceMethods.GetCapacity<E>(limits, length, SequenceMethods.FallbackCapacity);
        var result = ImmutableArray.CreateBuilder<E>(capacity);
        var body = span;
        while (body.Length is not 0)
            result.Add(converter.DecodeAuto(ref body));
        return result.Count == result.Capacity ? result.MoveToImmutable() : result.ToImmutable();
    }

    internal static E[] GetArray<E>(Converter<E> converter, ReadOnlySpan<byte> span)
    {
        Debug.Assert(span.Length is not 0);
        var result = GetArray(converter, span, out var actual);
        if (result.Length != actual)
            Array.Resize(ref result, actual);
        return result;
    }

    internal static E[] GetArray<E>(Converter<E> converter, ReadOnlySpan<byte> span, out int actual)
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        static void Expand(ref E[] buffer, E item)
        {
            var cursor = buffer.Length;
            Array.Resize(ref buffer, checked(cursor * 2));
            buffer[cursor] = item;
        }

        Debug.Assert(span.Length is not 0);
        var limits = span.Length;
        var length = converter.Length;
        var capacity = SequenceMethods.GetCapacity<E>(limits, length, SequenceMethods.FallbackCapacity);
        var result = new E[capacity];
        var cursor = 0;
        var body = span;
        while (body.Length is not 0)
        {
            var item = converter.DecodeAuto(ref body);
            if ((uint)cursor < (uint)result.Length)
                result[cursor] = item;
            else
                Expand(ref result, item);
            cursor++;
        }
        actual = cursor;
        return result;
    }
}
