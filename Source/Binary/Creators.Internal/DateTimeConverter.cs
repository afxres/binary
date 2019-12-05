using System;
using System.Buffers.Binary;

namespace Mikodev.Binary.Creators.Internal
{
    internal sealed class DateTimeConverter : Converter<DateTime>
    {
        public DateTimeConverter() : base(sizeof(long)) { }

        public override void Encode(ref Allocator allocator, DateTime item)
        {
            AllocatorHelper.Append(ref allocator, sizeof(long), item.ToBinary(), BinaryPrimitives.WriteInt64LittleEndian);
        }

        public override DateTime Decode(in ReadOnlySpan<byte> span)
        {
            return DateTime.FromBinary(BinaryPrimitives.ReadInt64LittleEndian(span));
        }
    }
}
