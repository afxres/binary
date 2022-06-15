namespace Mikodev.Binary.Features.Instance;

using Mikodev.Binary.Features;
using Mikodev.Binary.Internal;
using System;
using System.Runtime.CompilerServices;

#if NET7_0_OR_GREATER
internal readonly struct DateTimeOffsetRawConverter : IRawConverter<DateTimeOffset>
{
    public static int Length => sizeof(long) + sizeof(short);

    public static DateTimeOffset Decode(ref byte source)
    {
        var origin = LittleEndian.Decode<long>(ref source);
        var offset = LittleEndian.Decode<short>(ref Unsafe.Add(ref source, sizeof(long)));
        return new DateTimeOffset(origin, new TimeSpan(offset * TimeSpan.TicksPerMinute));
    }

    public static void Encode(ref byte target, DateTimeOffset item)
    {
        LittleEndian.Encode(ref target, item.Ticks);
        LittleEndian.Encode(ref Unsafe.Add(ref target, sizeof(long)), (short)(item.Offset.Ticks / TimeSpan.TicksPerMinute));
    }
}
#endif
