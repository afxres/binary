﻿namespace Mikodev.Binary.Creators.Isolated.Variables;

using Mikodev.Binary;
using Mikodev.Binary.Internal;
using System;
using System.Buffers.Binary;
using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

internal sealed class BitArrayConverter : Converter<BitArray?>
{
    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "m_array")]
    private static extern ref int[]? AccessFunction(BitArray array);

    private static uint FilterFunction(uint buffer, int remain)
    {
        Debug.Assert((uint)remain < 32);
        var offset = 32 - remain;
        buffer <<= offset;
        buffer >>= offset;
        return buffer;
    }

    private static void EncodeContents(Span<byte> target, ReadOnlySpan<int> source, int length)
    {
        var bounds = length >> 5;
        for (var i = 0; i < bounds; i++, target = target.Slice(4))
            BinaryPrimitives.WriteInt32LittleEndian(target, source[i]);
        var remain = length & 31;
        if (remain is 0)
            return;
        var buffer = FilterFunction((uint)source[bounds], remain);
        var limits = (remain + 7) >> 3;
        for (var i = 0; i < limits; i++)
            target[i] = (byte)(buffer >> (i * 8));
    }

    private static void DecodeContents(Span<int> target, ReadOnlySpan<byte> source, int length)
    {
        var bounds = length >> 5;
        for (var i = 0; i < bounds; i++, source = source.Slice(4))
            target[i] = BinaryPrimitives.ReadInt32LittleEndian(source);
        var remain = length & 31;
        if (remain is 0)
            return;
        var buffer = (uint)0;
        var limits = (remain + 7) >> 3;
        for (var i = 0; i < limits; i++)
            buffer |= (uint)source[i] << (i * 8);
        target[bounds] = (int)FilterFunction(buffer, remain);
    }

    public override void Encode(ref Allocator allocator, BitArray? item)
    {
        if (item is null)
            return;
        var length = item.Count;
        var margin = (-length) & 7;
        Debug.Assert((uint)margin < 8);
        Converter.Encode(ref allocator, margin);
        if (length is 0)
            return;
        var required = (int)(((uint)length + 7U) >> 3);
        var buffer = MemoryMarshal.CreateSpan(ref Allocator.Assign(ref allocator, required), required);
        var source = AccessFunction(item);
        EncodeContents(buffer, source, length);
    }

    public override BitArray? Decode(in ReadOnlySpan<byte> span)
    {
        if (span.Length is 0)
            return null;
        var cursor = span;
        var margin = Converter.Decode(ref cursor);
        Debug.Assert(margin >= 0);
        if (margin is 0 && cursor.Length is 0)
            return new BitArray(0);
        if (margin >= 8 || cursor.Length is 0)
            ThrowHelper.ThrowNotEnoughBytes();
        var length = checked((int)(((ulong)cursor.Length << 3) - (uint)margin));
        var result = new BitArray(length);
        var target = AccessFunction(result);
        DecodeContents(target, cursor, length);
        return result;
    }
}
