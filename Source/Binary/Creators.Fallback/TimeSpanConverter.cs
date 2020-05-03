using Mikodev.Binary.Internal;
using System;

namespace Mikodev.Binary.Creators.Fallback
{
    internal sealed class TimeSpanConverter : Converter<TimeSpan>
    {
        public TimeSpanConverter() : base(sizeof(long)) { }

        public override void Encode(ref Allocator allocator, TimeSpan item)
        {
            MemoryHelper.EncodeLittleEndian(ref allocator, item.Ticks);
        }

        public override TimeSpan Decode(in ReadOnlySpan<byte> span)
        {
            return new TimeSpan(MemoryHelper.DecodeLittleEndian<long>(span));
        }
    }
}
