using System;
using System.Buffers.Binary;

namespace Mikodev.Binary.Creators.Fallback
{
    internal sealed class TimeSpanConverter : Converter<TimeSpan>
    {
        private static readonly AllocatorAction<long> WriteInt64LittleEndian = BinaryPrimitives.WriteInt64LittleEndian;

        public TimeSpanConverter() : base(sizeof(long)) { }

        public override void Encode(ref Allocator allocator, TimeSpan item)
        {
            AllocatorHelper.Append(ref allocator, sizeof(long), item.Ticks, WriteInt64LittleEndian);
        }

        public override TimeSpan Decode(in ReadOnlySpan<byte> span)
        {
            return new TimeSpan(BinaryPrimitives.ReadInt64LittleEndian(span));
        }
    }
}
