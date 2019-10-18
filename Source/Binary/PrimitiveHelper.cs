using Mikodev.Binary.Internal;
using System;
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
        public static int EncodeNumberLength(int item)
        {
            if (((uint)item & 0xFFFF_FFC0U) == 0)
                return 1;
            else if (((uint)item & 0xFFFF_C000U) == 0)
                return 2;
            else
                return 4;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EncodeNumber(ref byte location, int numberLength, int item)
        {
            var data = (uint)item;
            if (numberLength == 1)
            {
                Memory.Add(ref location, 0) = (byte)data;
            }
            else if (numberLength == 2)
            {
                Memory.Add(ref location, 0) = (byte)((data >> 8) | 0x40);
                Memory.Add(ref location, 1) = (byte)data;
            }
            else
            {
                Memory.Add(ref location, 0) = (byte)((data >> 24) | 0x80);
                Memory.Add(ref location, 1) = (byte)(data >> 16);
                Memory.Add(ref location, 2) = (byte)(data >> 8);
                Memory.Add(ref location, 3) = (byte)data;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EncodeNumber(ref Allocator allocator, int item)
        {
            if (((uint)item & 0x8000_0000U) != 0)
                ThrowHelper.ThrowLengthPrefixOverflow();
            var prefixLength = EncodeNumberLength(item);
            ref var location = ref allocator.AllocateReference(prefixLength);
            EncodeNumber(ref location, prefixLength, item);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int DecodeNumberLength(byte head)
        {
            if ((head & 0x80) != 0)
                return 4;
            else if ((head & 0x40) != 0)
                return 2;
            else
                return 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int DecodeNumber(ref byte location, int prefixLength)
        {
            if (prefixLength == 4)
                return ((location & 0x7F) << 24) | (Memory.Add(ref location, 1) << 16) | (Memory.Add(ref location, 2) << 8) | Memory.Add(ref location, 3);
            var head = location & 0x3F;
            if (prefixLength == 2)
                return (head << 8) | Memory.Add(ref location, 1);
            return head;
        }

        public static int DecodeNumber(ref ReadOnlySpan<byte> span)
        {
            var spanLength = span.Length;
            if (spanLength == 0)
                goto fail;
            ref var location = ref MemoryMarshal.GetReference(span);
            var prefixLength = DecodeNumberLength(location);
            // check bounds via slice method
            span = span.Slice(prefixLength);
            return DecodeNumber(ref location, prefixLength);

        fail:
            ThrowHelper.ThrowLengthPrefixInvalidBytes();
            throw null;
        }
    }
}
