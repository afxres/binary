namespace Mikodev.Binary.Converters;

using System;
using System.Buffers;
using System.Buffers.Binary;

internal sealed class DateOnlyConverter : Converter<DateOnly>
{
    private static readonly SpanAction<byte, int> EncodeAction = BinaryPrimitives.WriteInt32LittleEndian;

    public DateOnlyConverter() : base(sizeof(int)) { }

    public override void Encode(ref Allocator allocator, DateOnly item)
    {
        Allocator.Append(ref allocator, sizeof(int), item.DayNumber, EncodeAction);
    }

    public override DateOnly Decode(in ReadOnlySpan<byte> span)
    {
        return DateOnly.FromDayNumber(BinaryPrimitives.ReadInt32LittleEndian(span));
    }
}
