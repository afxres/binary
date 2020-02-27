using System;
using System.Buffers.Binary;

namespace Mikodev.Binary.Creators.Fallback
{
    internal sealed class TimeSpanConverter : Converter<TimeSpan>
    {
        public TimeSpanConverter() : base(sizeof(long)) { }

        public override void Encode(ref Allocator allocator, TimeSpan item)
        {
            Allocator.AppendLittleEndian(ref allocator, item.Ticks);
        }

        public override TimeSpan Decode(in ReadOnlySpan<byte> span)
        {
            return new TimeSpan(BinaryPrimitives.ReadInt64LittleEndian(span));
        }
    }
}
