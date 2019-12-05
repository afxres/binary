using System;
using System.Buffers.Binary;

namespace Mikodev.Binary.Creators.Internal
{
    internal sealed class DateTimeOffsetConverter : Converter<DateTimeOffset>
    {
        public DateTimeOffsetConverter() : base(sizeof(long) + sizeof(short)) { }

        public override void Encode(ref Allocator allocator, DateTimeOffset item)
        {
            AllocatorHelper.Append(ref allocator, sizeof(long), item.Ticks, BinaryPrimitives.WriteInt64LittleEndian);
            AllocatorHelper.Append(ref allocator, sizeof(short), (short)(item.Offset.Ticks / TimeSpan.TicksPerMinute), BinaryPrimitives.WriteInt16LittleEndian);
        }

        public override DateTimeOffset Decode(in ReadOnlySpan<byte> span)
        {
            var head = BinaryPrimitives.ReadInt64LittleEndian(span);
            var tail = BinaryPrimitives.ReadInt16LittleEndian(span.Slice(sizeof(long)));
            return new DateTimeOffset(head, new TimeSpan(tail * TimeSpan.TicksPerMinute));
        }
    }
}
