using System;
using System.Buffers.Binary;

namespace Mikodev.Binary.Creators.Fallback
{
    internal sealed class DateTimeConverter : Converter<DateTime>
    {
        public DateTimeConverter() : base(sizeof(long)) { }

        public override void Encode(ref Allocator allocator, DateTime item)
        {
            Allocator.AppendLittleEndian(ref allocator, item.ToBinary());
        }

        public override DateTime Decode(in ReadOnlySpan<byte> span)
        {
            return DateTime.FromBinary(BinaryPrimitives.ReadInt64LittleEndian(span));
        }
    }
}
