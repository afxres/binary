using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace Mikodev.Binary.Creators.Fallback
{
    internal sealed class DecimalConverter : Converter<decimal>
    {
        private static readonly AllocatorAction<int> WriteInt32LittleEndian = BinaryPrimitives.WriteInt32LittleEndian;

        public DecimalConverter() : base(sizeof(int) * 4) { }

        public override void Encode(ref Allocator allocator, decimal item)
        {
            ref var bits = ref Unsafe.As<decimal, int>(ref item);
            AllocatorHelper.Append(ref allocator, sizeof(int), Unsafe.Add(ref bits, 2), WriteInt32LittleEndian);
            AllocatorHelper.Append(ref allocator, sizeof(int), Unsafe.Add(ref bits, 3), WriteInt32LittleEndian);
            AllocatorHelper.Append(ref allocator, sizeof(int), Unsafe.Add(ref bits, 1), WriteInt32LittleEndian);
            AllocatorHelper.Append(ref allocator, sizeof(int), Unsafe.Add(ref bits, 0), WriteInt32LittleEndian);
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
