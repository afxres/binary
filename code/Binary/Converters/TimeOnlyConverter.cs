namespace Mikodev.Binary.Converters;

using System;
using System.Buffers;
using System.Buffers.Binary;

internal sealed class TimeOnlyConverter : Converter<TimeOnly>
{
    private static readonly SpanAction<byte, long> EncodeAction = BinaryPrimitives.WriteInt64LittleEndian;

    public TimeOnlyConverter() : base(sizeof(long)) { }

    public override void Encode(ref Allocator allocator, TimeOnly item)
    {
        Allocator.Append(ref allocator, sizeof(long), item.Ticks, EncodeAction);
    }

    public override TimeOnly Decode(in ReadOnlySpan<byte> span)
    {
        return new TimeOnly(BinaryPrimitives.ReadInt64LittleEndian(span));
    }
}
