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

        internal static void EncodeFixed4(ref byte bytes, uint item)
        {
            Memory.Add(ref bytes, 0) = (byte)((item >> 24) | 0x80);
            Memory.Add(ref bytes, 1) = (byte)(item >> 16);
            Memory.Add(ref bytes, 2) = (byte)(item >> 8);
            Memory.Add(ref bytes, 3) = (byte)item;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void EncodeLengthPrefix(ref byte location, int prefixLength, uint item)
        {
            if (prefixLength == 1)
            {
                Memory.Add(ref location, 0) = (byte)item;
            }
            else if (prefixLength == 2)
            {
                Memory.Add(ref location, 0) = (byte)((item >> 8) | 0x40);
                Memory.Add(ref location, 1) = (byte)item;
            }
            else
            {
                EncodeFixed4(ref location, item);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int EncodePrefixLength(uint length)
        {
            if ((length & 0xFFFF_FFC0) == 0)
                return 1;
            else if ((length & 0xFFFF_C000) == 0)
                return 2;
            else
                return 4;
        }

        public static void EncodeLengthPrefix(ref Allocator allocator, uint item)
        {
            if ((item & 0x8000_0000) != 0)
                ThrowHelper.ThrowLengthPrefixOverflow();
            var prefixLength = EncodePrefixLength(item);
            ref var location = ref allocator.AllocateReference(prefixLength);
            EncodeLengthPrefix(ref location, prefixLength, item);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int DecodeLengthPrefix(ref byte location, int prefixLength)
        {
            if (prefixLength == 4)
                return ((location & 0x7F) << 24) | (Memory.Add(ref location, 1) << 16) | (Memory.Add(ref location, 2) << 8) | Memory.Add(ref location, 3);
            var head = location & 0x3F;
            if (prefixLength == 2)
                return (head << 8) | Memory.Add(ref location, 1);
            return head;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int DecodePrefixLength(byte head)
        {
            if ((head & 0x80) != 0)
                return 4;
            else if ((head & 0x40) != 0)
                return 2;
            else
                return 1;
        }

        public static int DecodeLengthPrefix(in ReadOnlySpan<byte> span)
        {
            var byteCount = span.Length;
            if (byteCount == 0)
                goto fail;
            ref var location = ref MemoryMarshal.GetReference(span);
            var prefixLength = DecodePrefixLength(location);
            if ((uint)byteCount < (uint)prefixLength)
                goto fail;
            return DecodeLengthPrefix(ref location, prefixLength);

        fail:
            ThrowHelper.ThrowLengthPrefixInvalidBytes();
            throw null;
        }
    }
}
