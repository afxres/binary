using System;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Mikodev.Binary.Internal
{
    internal static partial class MemoryHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static ref byte EnsureLength(ReadOnlySpan<byte> span)
        {
            if (span.IsEmpty)
                ThrowHelper.ThrowNotEnoughBytes();
            return ref MemoryMarshal.GetReference(span);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static ref byte EnsureLength(ReadOnlySpan<byte> span, int length)
        {
            Debug.Assert((uint)length <= 16);
            if (span.Length < length)
                ThrowHelper.ThrowNotEnoughBytes();
            return ref MemoryMarshal.GetReference(span);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static T EnsureHandleEndian<T>(T item, bool swap) where T : unmanaged
        {
            if (typeof(T) == typeof(short))
                return !swap ? item : (T)(object)BinaryPrimitives.ReverseEndianness((short)(object)item);
            else if (typeof(T) == typeof(int))
                return !swap ? item : (T)(object)BinaryPrimitives.ReverseEndianness((int)(object)item);
            else if (typeof(T) == typeof(long))
                return !swap ? item : (T)(object)BinaryPrimitives.ReverseEndianness((long)(object)item);
            else
                throw new NotSupportedException();
        }
    }
}
