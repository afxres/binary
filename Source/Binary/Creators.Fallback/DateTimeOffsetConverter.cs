using Mikodev.Binary.Internal;
using System;

namespace Mikodev.Binary.Creators.Fallback
{
    internal sealed class DateTimeOffsetConverter : Converter<DateTimeOffset>
    {
        public DateTimeOffsetConverter() : base(sizeof(long) + sizeof(short)) { }

        public override void Encode(ref Allocator allocator, DateTimeOffset item)
        {
            MemoryHelper.EncodeLittleEndian(ref allocator, item.Ticks);
            MemoryHelper.EncodeLittleEndian(ref allocator, (short)(item.Offset.Ticks / TimeSpan.TicksPerMinute));
        }

        public override DateTimeOffset Decode(in ReadOnlySpan<byte> span)
        {
            var head = MemoryHelper.DecodeLittleEndian<long>(span);
            var tail = MemoryHelper.DecodeLittleEndian<short>(span.Slice(sizeof(long)));
            return new DateTimeOffset(head, new TimeSpan(tail * TimeSpan.TicksPerMinute));
        }
    }
}
