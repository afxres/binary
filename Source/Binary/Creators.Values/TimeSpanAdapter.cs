using System;

namespace Mikodev.Binary.Creators.Values
{
    internal sealed class TimeSpanAdapter : Adapter<TimeSpan, long>
    {
        public override long Of(TimeSpan item) => item.Ticks;

        public override TimeSpan To(long item) => new TimeSpan(ticks: item);
    }
}
