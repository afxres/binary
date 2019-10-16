using System;

namespace Mikodev.Binary.Creators.Primitives
{
    internal sealed class DateTimeAdapter : Adapter<DateTime, long>
    {
        public override long OfValue(DateTime item) => item.ToBinary();

        public override DateTime ToValue(long item) => DateTime.FromBinary(item);
    }
}
