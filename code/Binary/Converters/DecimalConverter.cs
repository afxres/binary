using Mikodev.Binary.Internal;
using System;
using System.Runtime.CompilerServices;

namespace Mikodev.Binary.Converters
{
    internal sealed class DecimalConverter : Converter<decimal>
    {
        public DecimalConverter() : base(sizeof(int) * 4) { }

        public override void Encode(ref Allocator allocator, decimal item)
        {
            ref var target = ref Allocator.Assign(ref allocator, sizeof(int) * 4);
            ref var source = ref Unsafe.As<decimal, int>(ref item);
            MemoryHelper.EncodeLittleEndian(ref Unsafe.Add(ref target, sizeof(int) * 0), Unsafe.Add(ref source, 2));
            MemoryHelper.EncodeLittleEndian(ref Unsafe.Add(ref target, sizeof(int) * 1), Unsafe.Add(ref source, 3));
            MemoryHelper.EncodeLittleEndian(ref Unsafe.Add(ref target, sizeof(int) * 2), Unsafe.Add(ref source, 1));
            MemoryHelper.EncodeLittleEndian(ref Unsafe.Add(ref target, sizeof(int) * 3), Unsafe.Add(ref source, 0));
        }

        public override decimal Decode(in ReadOnlySpan<byte> span)
        {
            ref var source = ref MemoryHelper.EnsureLength(span, sizeof(int) * 4);
            var alpha = MemoryHelper.DecodeLittleEndian<int>(ref Unsafe.Add(ref source, sizeof(int) * 0));
            var bravo = MemoryHelper.DecodeLittleEndian<int>(ref Unsafe.Add(ref source, sizeof(int) * 1));
            var delta = MemoryHelper.DecodeLittleEndian<int>(ref Unsafe.Add(ref source, sizeof(int) * 2));
            var flags = MemoryHelper.DecodeLittleEndian<int>(ref Unsafe.Add(ref source, sizeof(int) * 3));
            return new decimal(alpha, bravo, delta, ((uint)flags & 0x8000_0000) is not 0, (byte)(flags >> 16));
        }
    }
}
