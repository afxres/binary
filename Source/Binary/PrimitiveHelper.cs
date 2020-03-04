using Mikodev.Binary.Internal;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Mikodev.Binary
{
    public static partial class PrimitiveHelper
    {
        /* x = 0 or 1
         * 0b1xxx_xxxx fixed length 4 bytes
         * 0b01xx_xxxx variable length 2 bytes
         * 0b00xx_xxxx variable length 1 bytes */

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int EncodeNumberLength(uint number)
        {
            Debug.Assert(number >= 0 && number <= int.MaxValue);
            if (number < 0x40)
                return 1;
            if (number < 0x4000)
                return 2;
            else
                return 4;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int DecodeNumberLength(byte header)
        {
            var result = (uint)header >> 6;
            if (result > 1U)
                result = 3U;
            return (int)(result + 1U);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void EncodeNumberFixed4(ref byte location, uint number)
        {
            Debug.Assert(number >= 0 && number <= int.MaxValue);
            Unsafe.Add(ref location, 0) = (byte)((number >> 24) | 0x80);
            Unsafe.Add(ref location, 1) = (byte)(number >> 16);
            Unsafe.Add(ref location, 2) = (byte)(number >> 8);
            Unsafe.Add(ref location, 3) = (byte)number;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void EncodeNumber(ref byte location, int length, uint number)
        {
            Debug.Assert(number >= 0 && number <= int.MaxValue);
            Debug.Assert(length == 1 || length == 2 || length == 4);
            if (length == 1)
            {
                Unsafe.Add(ref location, 0) = (byte)number;
            }
            else if (length == 2)
            {
                Unsafe.Add(ref location, 0) = (byte)((number >> 8) | 0x40);
                Unsafe.Add(ref location, 1) = (byte)number;
            }
            else
            {
                EncodeNumberFixed4(ref location, number);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int DecodeNumber(ref byte location, int length)
        {
            Debug.Assert(length == 1 || length == 2 || length == 4);
            if (length == 1)
                return location & 0x3F;
            if (length == 2)
                return ((location & 0x3F) << 8) | Unsafe.Add(ref location, 1);
            else
                return ((location & 0x7F) << 24) | (Unsafe.Add(ref location, 1) << 16) | (Unsafe.Add(ref location, 2) << 8) | Unsafe.Add(ref location, 3);
        }

        public static void EncodeNumber(ref Allocator allocator, int number)
        {
            if (number < 0)
                ThrowHelper.ThrowArgumentNumberInvalid();
            var numberLength = EncodeNumberLength((uint)number);
            ref var target = ref Allocator.Assign(ref allocator, numberLength);
            EncodeNumber(ref target, numberLength, (uint)number);
        }

        public static int DecodeNumber(ref ReadOnlySpan<byte> span)
        {
            if (span.IsEmpty)
                return ThrowHelper.ThrowNotEnoughBytes<int>();
            ref var source = ref MemoryMarshal.GetReference(span);
            var numberLength = DecodeNumberLength(source);
            // check bounds via slice method
            span = span.Slice(numberLength);
            return DecodeNumber(ref source, numberLength);
        }
    }
}
