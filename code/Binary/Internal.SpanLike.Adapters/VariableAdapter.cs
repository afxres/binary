namespace Mikodev.Binary.Internal.SpanLike.Adapters;

using System;

internal sealed class VariableAdapter<T> : SpanLikeAdapter<T>
{
    private readonly Converter<T> converter;

    public VariableAdapter(Converter<T> converter) => this.converter = converter;

    public override void Encode(ref Allocator allocator, ReadOnlySpan<T> item)
    {
        var converter = this.converter;
        foreach (var i in item)
            converter.EncodeAuto(ref allocator, i);
    }

    public override MemoryBuffer<T> Decode(ReadOnlySpan<byte> span)
    {
        if (span.Length is 0)
            return new MemoryBuffer<T>(Array.Empty<T>(), 0);
        const int Capacity = 8;
        var memory = new MemoryBuffer<T>(Capacity);
        var body = span;
        var converter = this.converter;
        while (body.Length is not 0)
            MemoryBuffer<T>.Add(ref memory, converter.DecodeAuto(ref body));
        return memory;
    }
}
