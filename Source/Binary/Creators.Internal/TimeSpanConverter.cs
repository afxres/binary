using System;
using System.Buffers.Binary;

namespace Mikodev.Binary.Creators.Internal
{
    internal sealed class TimeSpanConverter : Converter<TimeSpan>
    {
        public TimeSpanConverter() : base(sizeof(long)) { }

        public override void Encode(ref Allocator allocator, TimeSpan item)
        {
            AllocatorHelper.Append(ref allocator, sizeof(long), item.Ticks, BinaryPrimitives.WriteInt64LittleEndian);
        }

        public override TimeSpan Decode(in ReadOnlySpan<byte> span)
        {
            return new TimeSpan(BinaryPrimitives.ReadInt64LittleEndian(span));
        }
    }
}
