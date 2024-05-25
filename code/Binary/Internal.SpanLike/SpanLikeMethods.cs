namespace Mikodev.Binary.Internal.SpanLike;

using Mikodev.Binary.Internal.Sequence;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

internal static class SpanLikeMethods
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

    private static E[] GetPartialArray<E>(Converter<E> converter, ref ReadOnlySpan<byte> span)
    {
        var buffer = new InlineBuffer<E>();
        var target = (Span<E>)buffer;
        var cursor = 0;
        while (cursor is not MaxLevels && span.Length is not 0)
            target[cursor++] = converter.DecodeAuto(ref span);
        var result = new E[span.Length is 0 ? cursor : NewLength];
        target.Slice(0, cursor).CopyTo(new Span<E>(result));
        return result;
    }

    internal static List<E> GetList<E>(Converter<E> converter, ReadOnlySpan<byte> span)
    {
        var limits = span.Length;
        if (limits is 0)
            return [];
        var intent = span;
        var length = converter.Length;
        var result = length is 0
            ? GetPartialList(converter, ref intent)
            : new List<E>(SequenceContext.GetCapacity<E>(limits, length));
        while (intent.Length is not 0)
            result.Add(converter.DecodeAuto(ref intent));
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

        var limits = span.Length;
        if (limits is 0)
        {
            actual = 0;
            return [];
        }

        var intent = span;
        var length = converter.Length;
        var result = length is 0
            ? GetPartialArray(converter, ref intent)
            : new E[SequenceContext.GetCapacity<E>(limits, length)];
        var cursor = length is 0
            ? Math.Min(result.Length, MaxLevels)
            : 0;
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
