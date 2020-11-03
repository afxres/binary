using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Mikodev.Binary.Internal
{
    internal static partial class MemoryHelper
    {
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
        internal static int EncodeNumberLength(uint number)
        {
            Debug.Assert(number <= int.MaxValue);
            if ((number >> 7) is 0)
                return 1;
            else
                return 4;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void EncodeNumber(ref byte location, uint number, int numberLength)
        {
            Debug.Assert(number <= int.MaxValue);
            Debug.Assert(numberLength is 1 or 4);
            if (numberLength is 1)
                EncodeNativeEndian(ref location, (byte)number);
            else
                EncodeNumberEndian(ref location, (int)(number | 0x8000_0000U));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool EncodeNumberReduceBuffer(ref byte location, int remain, int length)
        {
            Debug.Assert(remain >= 0);
            Debug.Assert(length >= 0);
            const int Limits = 16;
            if (length > Limits || remain < ((-length) & 7))
                return false;
            for (var i = 0; i < length; i += 8)
                EncodeNativeEndian(ref Unsafe.Add(ref location, i + 1), DecodeNativeEndian<long>(ref Unsafe.Add(ref location, i + 4)));
            EncodeNumber(ref location, (uint)length, numberLength: 1);
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int DecodeNumber(ref byte location, ref int offset, int limits)
        {
            Debug.Assert(offset >= 0);
            Debug.Assert(limits >= 0);
            Debug.Assert(limits >= offset);
            if (limits == offset)
                ThrowHelper.ThrowNotEnoughBytes();
            ref var source = ref Unsafe.Add(ref location, offset);
            var header = (uint)DecodeNativeEndian<byte>(ref source);
            offset += 1;
            if ((header & 0x80U) is 0)
                return (int)header;
            if ((uint)(limits - offset) < 3U)
                ThrowHelper.ThrowNotEnoughBytes();
            var result = (uint)DecodeNumberEndian<int>(ref source);
            offset += 3;
            return (int)(result & 0x7FFF_FFFFU);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int DecodeNumberEnsureBuffer(ref byte location, ref int offset, int limits)
        {
            Debug.Assert(offset >= 0);
            Debug.Assert(limits >= 0);
            Debug.Assert(limits >= offset);
            var length = DecodeNumber(ref location, ref offset, limits);
            if ((uint)(limits - offset) < (uint)length)
                ThrowHelper.ThrowNotEnoughBytes();
            return length;
        }
    }
}
