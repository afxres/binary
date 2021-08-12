namespace Mikodev.Binary.Converters;

using Mikodev.Binary.Internal;
using System;

internal sealed class TimeSpanConverter : Converter<TimeSpan>
{
    public TimeSpanConverter() : base(sizeof(long)) { }

    public override void Encode(ref Allocator allocator, TimeSpan item)
    {
        LittleEndian.Encode(ref allocator, item.Ticks);
    }

    public override TimeSpan Decode(in ReadOnlySpan<byte> span)
    {
        return new TimeSpan(LittleEndian.Decode<long>(span));
    }
}
