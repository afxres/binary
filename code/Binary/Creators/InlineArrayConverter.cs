namespace Mikodev.Binary.Creators;

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

internal sealed class InlineArrayConverter<T, E> : Converter<T> where T : struct
{
    private readonly int length;

    private readonly Converter<E> converter;

    public InlineArrayConverter(Converter<E> converter, int length) : base(checked(converter.Length * length))
    {
        this.converter = converter;
        this.length = length;
    }

    public override void Encode(ref Allocator allocator, T item)
    {
        var buffer = MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<T, E>(ref item), this.length);
        var converter = this.converter;
        for (var i = 0; i < buffer.Length; i++)
            converter.EncodeAuto(ref allocator, buffer[i]);
        return;
    }

    public override T Decode(in ReadOnlySpan<byte> span)
    {
        var body = span;
        var result = default(T);
        var buffer = MemoryMarshal.CreateSpan(ref Unsafe.As<T, E>(ref result), this.length);
        var converter = this.converter;
        for (var i = 0; i < buffer.Length; i++)
            buffer[i] = converter.DecodeAuto(ref body);
        return result;
    }
}
