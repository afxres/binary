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
            const int Limits = 4;
            ref var target = ref Allocator.Assign(ref allocator, sizeof(int) * Limits);
            var buffer = (stackalloc int[Limits]);
            _ = decimal.GetBits(item, buffer);
            for (var i = 0; i < Limits; i++)
                LittleEndian.Encode(ref Unsafe.Add(ref target, sizeof(int) * i), buffer[i]);
            return;
        }

        public override decimal Decode(in ReadOnlySpan<byte> span)
        {
            const int Limits = 4;
            ref var source = ref MemoryHelper.EnsureLength(span, sizeof(int) * Limits);
            var buffer = (stackalloc int[Limits]);
            for (var i = 0; i < Limits; i++)
                buffer[i] = LittleEndian.Decode<int>(ref Unsafe.Add(ref source, sizeof(int) * i));
            return new decimal(buffer);
        }
    }
}
