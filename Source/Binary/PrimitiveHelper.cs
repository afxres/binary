﻿using Mikodev.Binary.Internal;
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
            if ((header & 0x80) != 0)
                return 4;
            if ((header & 0x40) != 0)
                return 2;
            else
                return 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void EncodeNumberFixed4(ref byte location, uint number)
        {
            Debug.Assert(number >= 0 && number <= int.MaxValue);
            Memory.Add(ref location, 0) = (byte)((number >> 24) | 0x80);
            Memory.Add(ref location, 1) = (byte)(number >> 16);
            Memory.Add(ref location, 2) = (byte)(number >> 8);
            Memory.Add(ref location, 3) = (byte)number;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void EncodeNumber(ref byte location, int length, uint number)
        {
            Debug.Assert(number >= 0 && number <= int.MaxValue);
            Debug.Assert(length == 1 || length == 2 || length == 4);
            if (length == 1)
            {
                Memory.Add(ref location, 0) = (byte)number;
            }
            else if (length == 2)
            {
                Memory.Add(ref location, 0) = (byte)((number >> 8) | 0x40);
                Memory.Add(ref location, 1) = (byte)number;
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
                return ((location & 0x3F) << 8) | Memory.Add(ref location, 1);
            else
                return ((location & 0x7F) << 24) | (Memory.Add(ref location, 1) << 16) | (Memory.Add(ref location, 2) << 8) | Memory.Add(ref location, 3);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EncodeNumber(ref Allocator allocator, int number)
        {
            if (number < 0)
                ThrowHelper.ThrowNumberNegative();
            var length = EncodeNumberLength((uint)number);
            ref var location = ref allocator.AllocateReference(length);
            EncodeNumber(ref location, length, (uint)number);
        }

        public static int DecodeNumber(ref ReadOnlySpan<byte> span)
        {
            if (span.IsEmpty)
                ThrowHelper.ThrowNotEnoughBytes();
            ref var location = ref MemoryMarshal.GetReference(span);
            var length = DecodeNumberLength(location);
            // check bounds via slice method
            span = span.Slice(length);
            return DecodeNumber(ref location, length);
        }
    }
}