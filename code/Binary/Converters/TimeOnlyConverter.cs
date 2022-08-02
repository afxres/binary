namespace Mikodev.Binary.Converters;

using Mikodev.Binary.Internal;
using System;

internal sealed class TimeOnlyConverter : Converter<TimeOnly>
{
    public TimeOnlyConverter() : base(sizeof(long)) { }

    public override void Encode(ref Allocator allocator, TimeOnly item)
    {
        LittleEndian.Encode(ref allocator, item.Ticks);
    }

    public override TimeOnly Decode(in ReadOnlySpan<byte> span)
    {
        return new TimeOnly(LittleEndian.Decode<long>(span));
    }
}
