using System;

namespace Mikodev.Binary.Creators.Values
{
    internal sealed class DateTimeOffsetAdapter : Adapter<DateTimeOffset, (long Ticks, short Offset)>
    {
        public override (long Ticks, short Offset) Of(DateTimeOffset item) => (item.Ticks, (short)(item.Offset.Ticks / TimeSpan.TicksPerMinute));

        public override DateTimeOffset To((long Ticks, short Offset) item) => new DateTimeOffset(item.Ticks, new TimeSpan(item.Offset * TimeSpan.TicksPerMinute));
    }
}
