namespace Mikodev.Binary.Converters;

using Mikodev.Binary.Internal;
using System;

#if NET6_0_OR_GREATER
internal sealed class DateOnlyConverter : Converter<DateOnly>
{
    public DateOnlyConverter() : base(sizeof(int)) { }

    public override void Encode(ref Allocator allocator, DateOnly item)
    {
        LittleEndian.Encode(ref allocator, item.DayNumber);
    }

    public override DateOnly Decode(in ReadOnlySpan<byte> span)
    {
        return DateOnly.FromDayNumber(LittleEndian.Decode<int>(span));
    }
}
#endif
