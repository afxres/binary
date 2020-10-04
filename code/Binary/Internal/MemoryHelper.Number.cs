using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Mikodev.Binary.Internal
{
    internal static partial class MemoryHelper
    {
        /* 0b1xxx_xxxx constant length 4 bytes
         * 0b01xx_xxxx variable length 2 bytes
         * 0b00xx_xxxx variable length 1 bytes */

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int EncodeNumberLength(uint number)
        {
            Debug.Assert(number <= int.MaxValue);
            number >>= 6;
            if (number is 0)
                return 1;
            number >>= 8;
            if (number is 0)
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
        internal static void EncodeNumberEndian<T>(ref byte location, T item) where T : unmanaged
        {
            Unsafe.WriteUnaligned(ref location, EnsureHandleEndian(item, BitConverter.IsLittleEndian));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static T DecodeNumberEndian<T>(ref byte location) where T : unmanaged
        {
            return EnsureHandleEndian(Unsafe.ReadUnaligned<T>(ref location), BitConverter.IsLittleEndian);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void EncodeNumber(ref byte location, uint number, int numberLength)
        {
            Debug.Assert(number <= int.MaxValue);
            Debug.Assert(numberLength is 1 or 2 or 4);
            if (numberLength is 1)
                EncodeNativeEndian(ref location, (byte)number);
            else if (numberLength is 2)
                EncodeNumberEndian(ref location, (short)(ushort)(number | 0x4000));
            else
                EncodeNumberEndian(ref location, (int)(number | 0x8000_0000));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int DecodeNumber(ref byte location, int numberLength)
        {
            Debug.Assert(numberLength is 1 or 2 or 4);
            if (numberLength is 1)
                return DecodeNativeEndian<byte>(ref location) & 0x3F;
            else if (numberLength is 2)
                return DecodeNumberEndian<short>(ref location) & 0x3FFF;
            else
                return DecodeNumberEndian<int>(ref location) & 0x7FFF_FFFF;
        }
    }
}
