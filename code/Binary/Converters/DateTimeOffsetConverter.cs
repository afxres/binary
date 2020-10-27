using Mikodev.Binary.Internal;
using System;
using System.Runtime.CompilerServices;

namespace Mikodev.Binary.Converters
{
    internal sealed class DateTimeOffsetConverter : Converter<DateTimeOffset>
    {
        public DateTimeOffsetConverter() : base(sizeof(long) + sizeof(short)) { }

        public override void Encode(ref Allocator allocator, DateTimeOffset item)
        {
            ref var target = ref Allocator.Assign(ref allocator, sizeof(long) + sizeof(short));
            MemoryHelper.EncodeLittleEndian(ref target, item.Ticks);
            MemoryHelper.EncodeLittleEndian(ref Unsafe.Add(ref target, sizeof(long)), (short)(item.Offset.Ticks / TimeSpan.TicksPerMinute));
        }

        public override DateTimeOffset Decode(in ReadOnlySpan<byte> span)
        {
            ref var source = ref MemoryHelper.EnsureLength(span, sizeof(long) + sizeof(short));
            var origin = MemoryHelper.DecodeLittleEndian<long>(ref source);
            var offset = MemoryHelper.DecodeLittleEndian<short>(ref Unsafe.Add(ref source, sizeof(long)));
            return new DateTimeOffset(origin, new TimeSpan(offset * TimeSpan.TicksPerMinute));
        }
    }
}
