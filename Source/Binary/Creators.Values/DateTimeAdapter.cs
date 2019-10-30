using System;

namespace Mikodev.Binary.Creators.Values
{
    internal sealed class DateTimeAdapter : Adapter<DateTime, long>
    {
        public override long Of(DateTime item) => item.ToBinary();

        public override DateTime To(long item) => DateTime.FromBinary(item);
    }
}
