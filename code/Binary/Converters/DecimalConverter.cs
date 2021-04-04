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
#if NET5_0_OR_GREATER
            var buffer = (stackalloc int[Limits]);
            _ = decimal.GetBits(item, buffer);
#else
            var source = System.Runtime.InteropServices.MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<decimal, int>(ref item), Limits);
            var buffer = (stackalloc int[Limits] { source[2], source[3], source[1], source[0] });
#endif
            for (var i = 0; i < Limits; i++)
                LittleEndian.Encode(ref Unsafe.Add(ref target, sizeof(int) * i), buffer[i]);
            return;
        }

        public override decimal Decode(in ReadOnlySpan<byte> span)
        {
            const int Limits = 4;
            ref var source = ref MemoryHelper.EnsureLength(span, sizeof(int) * 4);
            var buffer = (stackalloc int[Limits]);
            for (var i = 0; i < Limits; i++)
                buffer[i] = LittleEndian.Decode<int>(ref Unsafe.Add(ref source, sizeof(int) * i));
#if NET5_0_OR_GREATER
            return new decimal(buffer);
#else
            return new decimal(buffer[0], buffer[1], buffer[2], ((uint)buffer[3] & 0x8000_0000) is not 0, (byte)(buffer[3] >> 16));
#endif
        }
    }
}
