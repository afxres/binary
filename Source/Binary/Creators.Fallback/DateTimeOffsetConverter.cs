using System;
using System.Buffers.Binary;

namespace Mikodev.Binary.Creators.Fallback
{
    internal sealed class DateTimeOffsetConverter : Converter<DateTimeOffset>
    {
        public DateTimeOffsetConverter() : base(sizeof(long) + sizeof(short)) { }

        public override void Encode(ref Allocator allocator, DateTimeOffset item)
        {
            Allocator.AppendLittleEndian(ref allocator, item.Ticks);
            Allocator.AppendLittleEndian(ref allocator, (short)(item.Offset.Ticks / TimeSpan.TicksPerMinute));
        }

        public override DateTimeOffset Decode(in ReadOnlySpan<byte> span)
        {
            var head = BinaryPrimitives.ReadInt64LittleEndian(span);
            var tail = BinaryPrimitives.ReadInt16LittleEndian(span.Slice(sizeof(long)));
            return new DateTimeOffset(head, new TimeSpan(tail * TimeSpan.TicksPerMinute));
        }
    }
}
