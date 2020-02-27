using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace Mikodev.Binary.Creators.Fallback
{
    internal sealed class DecimalConverter : Converter<decimal>
    {
        public DecimalConverter() : base(sizeof(int) * 4) { }

        public override void Encode(ref Allocator allocator, decimal item)
        {
            ref var bits = ref Unsafe.As<decimal, int>(ref item);
            Allocator.AppendLittleEndian(ref allocator, Unsafe.Add(ref bits, 2));
            Allocator.AppendLittleEndian(ref allocator, Unsafe.Add(ref bits, 3));
            Allocator.AppendLittleEndian(ref allocator, Unsafe.Add(ref bits, 1));
            Allocator.AppendLittleEndian(ref allocator, Unsafe.Add(ref bits, 0));
        }

        public override decimal Decode(in ReadOnlySpan<byte> span)
        {
            var alpha = BinaryPrimitives.ReadInt32LittleEndian(span.Slice(sizeof(int) * 0));
            var bravo = BinaryPrimitives.ReadInt32LittleEndian(span.Slice(sizeof(int) * 1));
            var delta = BinaryPrimitives.ReadInt32LittleEndian(span.Slice(sizeof(int) * 2));
            var flags = BinaryPrimitives.ReadInt32LittleEndian(span.Slice(sizeof(int) * 3));
            return new decimal(alpha, bravo, delta, ((uint)flags & 0x8000_0000) != 0, (byte)(flags >> 16));
        }
    }
}
