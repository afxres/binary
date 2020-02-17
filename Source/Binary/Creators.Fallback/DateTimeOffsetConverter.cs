using System;
using System.Buffers.Binary;

namespace Mikodev.Binary.Creators.Fallback
{
    internal sealed class DateTimeOffsetConverter : Converter<DateTimeOffset>
    {
        private static readonly AllocatorAction<long> WriteInt64LittleEndian = BinaryPrimitives.WriteInt64LittleEndian;

        private static readonly AllocatorAction<short> WriteInt16LittleEndian = BinaryPrimitives.WriteInt16LittleEndian;

        public DateTimeOffsetConverter() : base(sizeof(long) + sizeof(short)) { }

        public override void Encode(ref Allocator allocator, DateTimeOffset item)
        {
            AllocatorHelper.Append(ref allocator, sizeof(long), item.Ticks, WriteInt64LittleEndian);
            AllocatorHelper.Append(ref allocator, sizeof(short), (short)(item.Offset.Ticks / TimeSpan.TicksPerMinute), WriteInt16LittleEndian);
        }

        public override DateTimeOffset Decode(in ReadOnlySpan<byte> span)
        {
            var head = BinaryPrimitives.ReadInt64LittleEndian(span);
            var tail = BinaryPrimitives.ReadInt16LittleEndian(span.Slice(sizeof(long)));
            return new DateTimeOffset(head, new TimeSpan(tail * TimeSpan.TicksPerMinute));
        }
    }
}
