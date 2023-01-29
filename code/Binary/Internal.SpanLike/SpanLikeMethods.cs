namespace Mikodev.Binary.Internal.SpanLike;

using Mikodev.Binary.Internal.Sequence;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

internal static class SpanLikeMethods
{
    internal static List<E> GetList<E>(Converter<E> converter, ReadOnlySpan<byte> span)
    {
        var limits = span.Length;
        if (limits is 0)
            return new List<E>();
        var length = converter.Length;
        var capacity = SequenceMethods.GetCapacity<E>(limits, length, SequenceMethods.FallbackCapacity);
        var result = new List<E>(capacity);
        var intent = span;
        while (intent.Length is not 0)
            result.Add(converter.DecodeAuto(ref intent));
        return result;
    }

    internal static ImmutableArray<E> GetImmutableArray<E>(Converter<E> converter, ReadOnlySpan<byte> span)
    {
        var limits = span.Length;
        if (limits is 0)
            return ImmutableArray<E>.Empty;
        var length = converter.Length;
        var capacity = SequenceMethods.GetCapacity<E>(limits, length, SequenceMethods.FallbackCapacity);
        var result = ImmutableArray.CreateBuilder<E>(capacity);
        var intent = span;
        while (intent.Length is not 0)
            result.Add(converter.DecodeAuto(ref intent));
        return result.Count == result.Capacity ? result.MoveToImmutable() : result.ToImmutable();
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

        var limits = span.Length;
        if (limits is 0)
        {
            actual = 0;
            return Array.Empty<E>();
        }

        var length = converter.Length;
        var capacity = SequenceMethods.GetCapacity<E>(limits, length, SequenceMethods.FallbackCapacity);
        var result = new E[capacity];
        var cursor = 0;
        var intent = span;
        while (intent.Length is not 0)
        {
            var item = converter.DecodeAuto(ref intent);
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
