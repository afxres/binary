using Mikodev.Binary.Internal;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Mikodev.Binary
{
    public static class PrimitiveHelper
    {
        /* x = 0 or 1
         * 0b1xxx_xxxx fixed length 4 bytes
         * 0b00xx_xxxx variable length 1 bytes
         * 0b010x_xxxx variable length 2 bytes */

        [DebuggerStepThrough, MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ThrowLengthPrefixOverflow() => throw new ArgumentException("Length prefix overflow!");

        [DebuggerStepThrough, MethodImpl(MethodImplOptions.NoInlining)]
        internal static int ThrowLengthPrefixInvalidBytes() => throw new ArgumentException("Length prefix bytes invalid!");

        internal static void EncodeFixed4(ref byte bytes, uint item)
        {
            Memory.Add(ref bytes, 0) = (byte)((item >> 24) | 0x80);
            Memory.Add(ref bytes, 1) = (byte)(item >> 16);
            Memory.Add(ref bytes, 2) = (byte)(item >> 8);
            Memory.Add(ref bytes, 3) = (byte)item;
        }

        public static void EncodeLengthPrefix(ref Allocator allocator, uint item)
        {
            if ((item & 0x8000_0000) != 0)
            {
                ThrowLengthPrefixOverflow();
            }
            else if ((item & 0xFFFF_FFC0) == 0)
            {
                ref var location = ref allocator.AllocateReference(1);
                Memory.Add(ref location, 0) = (byte)item;
            }
            else if ((item & 0xFFFF_E000) == 0)
            {
                ref var location = ref allocator.AllocateReference(2);
                Memory.Add(ref location, 0) = (byte)((item >> 8) | 0x40);
                Memory.Add(ref location, 1) = (byte)item;
            }
            else
            {
                ref var location = ref allocator.AllocateReference(4);
                EncodeFixed4(ref location, item);
            }
        }

        public static int DecodeLengthPrefix(ref byte bytes, int byteCount)
        {
            var source = bytes;
            if ((source & 0x80) != 0)
            {
                if (byteCount < 4)
                    goto fail;
                return ((source & 0x7F) << 24) | (Memory.Add(ref bytes, 1) << 16) | (Memory.Add(ref bytes, 2) << 8) | Memory.Add(ref bytes, 3);
            }
            if ((source & 0x40) == 0)
            {
                return source & 0x3F;
            }
            if ((source & 0x20) == 0)
            {
                if (byteCount < 2)
                    goto fail;
                return ((source & 0x1F) << 8) | Memory.Add(ref bytes, 1);
            }

        fail:
            return ThrowLengthPrefixInvalidBytes();
        }

        public static int DecodeLengthPrefix(in ReadOnlySpan<byte> span)
        {
            var byteCount = span.Length;
            if (byteCount == 0)
                goto fail;
            ref var bytes = ref MemoryMarshal.GetReference(span);
            return DecodeLengthPrefix(ref bytes, byteCount);

        fail:
            return ThrowLengthPrefixInvalidBytes();
        }
    }
}
