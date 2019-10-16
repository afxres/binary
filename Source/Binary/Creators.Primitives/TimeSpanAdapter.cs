using System;

namespace Mikodev.Binary.Creators.Primitives
{
    internal sealed class TimeSpanAdapter : Adapter<TimeSpan, long>
    {
        public override long OfValue(TimeSpan item) => item.Ticks;

        public override TimeSpan ToValue(long item) => new TimeSpan(ticks: item);
    }
}
