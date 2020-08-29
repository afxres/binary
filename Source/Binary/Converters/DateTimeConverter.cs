using Mikodev.Binary.Internal;
using System;

namespace Mikodev.Binary.Converters
{
    internal sealed class DateTimeConverter : Converter<DateTime>
    {
        public DateTimeConverter() : base(sizeof(long)) { }

        public override void Encode(ref Allocator allocator, DateTime item)
        {
            MemoryHelper.EncodeLittleEndian(ref allocator, item.ToBinary());
        }

        public override DateTime Decode(in ReadOnlySpan<byte> span)
        {
            return DateTime.FromBinary(MemoryHelper.DecodeLittleEndian<long>(span));
        }
    }
}
