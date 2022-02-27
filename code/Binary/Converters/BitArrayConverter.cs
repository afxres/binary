namespace Mikodev.Binary.Converters;

using Mikodev.Binary.Internal;
using System;
using System.Buffers.Binary;
using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;

internal sealed class BitArrayConverter : Converter<BitArray?>
{
    private static readonly Func<BitArray, int[]> FieldFunction;

    static BitArrayConverter()
    {
        var field = CommonModule.GetField(typeof(BitArray), "m_array", BindingFlags.Instance | BindingFlags.NonPublic);
        var parameter = Expression.Parameter(typeof(BitArray), "array");
        var expression = Expression.Lambda<Func<BitArray, int[]>>(Expression.Field(parameter, field), parameter);
        FieldFunction = expression.Compile();
    }

    private static void EncodeInternal(Span<byte> target, ReadOnlySpan<int> source, int length)
    {
        var bounds = length >> 5;
        for (var i = 0; i < bounds; i++, target = target.Slice(4))
            BinaryPrimitives.WriteInt32LittleEndian(target, source[i]);
        var remain = length & 31;
        if (remain is 0)
            return;
        var offset = 32 - remain;
        var buffer = (uint)source[bounds];
        buffer <<= offset;
        buffer >>= offset;
        var limits = (remain + 7) >> 3;
        for (var i = 0; i < limits; i++)
            target[i] = (byte)(buffer >> (i * 8));
    }

    private static void DecodeInternal(Span<int> target, ReadOnlySpan<byte> source, int length)
    {
        var bounds = length >> 5;
        for (var i = 0; i < bounds; i++, source = source.Slice(4))
            target[i] = BinaryPrimitives.ReadInt32LittleEndian(source);
        var remain = length & 31;
        if (remain is 0)
            return;
        var offset = 32 - remain;
        var buffer = (uint)0;
        var limits = (remain + 7) >> 3;
        for (var i = 0; i < limits; i++)
            buffer |= (uint)source[i] << (i * 8);
        buffer <<= offset;
        buffer >>= offset;
        target[bounds] = (int)buffer;
    }

    public override void Encode(ref Allocator allocator, BitArray? item)
    {
        if (item is null)
            return;
        var length = item.Count;
        var source = FieldFunction.Invoke(item);
        var padding = 8 - (length & 7);
        Converter.Encode(ref allocator, padding);
        if (length is 0)
            return;
        var required = (int)(((uint)length + 7U) >> 3);
        var target = MemoryMarshal.CreateSpan(ref Allocator.Assign(ref allocator, required), required);
        EncodeInternal(target, source, length);
    }

    public override BitArray? Decode(in ReadOnlySpan<byte> span)
    {
        if (span.Length is 0)
            return null;
        var body = span;
        var padding = Converter.Decode(ref body);
        if (padding is 0)
            return new BitArray(0);
        if (padding > 7 || body.Length is 0)
            throw new ArgumentException("Invalid bit array bytes.");
        var length = (body.Length << 3) - padding;
        var result = new BitArray(length);
        var target = FieldFunction.Invoke(result);
        DecodeInternal(target, body, length);
        return result;
    }
}
