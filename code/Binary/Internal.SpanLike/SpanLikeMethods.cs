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

    private static E[] GetArrayRecursively<E>(Converter<E> converter, ref ReadOnlySpan<byte> span, int cursor)
    {
        if (cursor is MaxLevels || span.Length is 0)
        {
            return new E[span.Length is 0 ? cursor : NewLength];
        }
        else
        {
            var intent = converter.DecodeAuto(ref span);
            var result = GetArrayRecursively(converter, ref span, cursor + 1);
            result[cursor] = intent;
            return result;
        }
    }

    internal static List<E> GetList<E>(Converter<E> converter, ReadOnlySpan<byte> span)
    {
        var limits = span.Length;
        if (limits is 0)
            return new List<E>();
        var intent = span;
        var length = converter.Length;
        var result = length is 0
            ? GetListRecursively(converter, ref intent, 0)
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
            return Array.Empty<E>();
        }

        var intent = span;
        var length = converter.Length;
        var result = length is 0
            ? GetArrayRecursively(converter, ref intent, 0)
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
