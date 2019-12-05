using System;
using System.Buffers.Binary;

namespace Mikodev.Binary.Creators.Internal
{
    internal sealed class DecimalConverter : Converter<decimal>
    {
        public DecimalConverter() : base(sizeof(int) * 4) { }

        public override void Encode(ref Allocator allocator, decimal item)
        {
            var bits = decimal.GetBits(item);
            for (var i = 0; i < bits.Length; i++)
                AllocatorHelper.Append(ref allocator, sizeof(int), bits[i], BinaryPrimitives.WriteInt32LittleEndian);
        }

        public override decimal Decode(in ReadOnlySpan<byte> span)
        {
            var bits = new int[4];
            for (var i = 0; i < bits.Length; i++)
                bits[i] = BinaryPrimitives.ReadInt32LittleEndian(span.Slice(sizeof(int) * i));
            return new decimal(bits);
        }
    }
}
