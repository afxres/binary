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
        internal static ref byte EnsureLength(ReadOnlySpan<byte> span, int length)
        {
            Debug.Assert(length is not 0);
            if ((uint)span.Length < (uint)length)
                ThrowHelper.ThrowNotEnoughBytes();
            return ref MemoryMarshal.GetReference(span);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static ref byte EnsureLength(ref ReadOnlySpan<byte> span, int length)
        {
            Debug.Assert(length is not 0);
            ref var source = ref MemoryMarshal.GetReference(span);
            var limits = span.Length;
            if ((uint)limits < (uint)length)
                ThrowHelper.ThrowNotEnoughBytes();
            span = MemoryMarshal.CreateReadOnlySpan(ref Unsafe.Add(ref source, length), limits - length);
            return ref source;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static ReadOnlySpan<byte> EnsureLengthReturnBuffer(ref ReadOnlySpan<byte> span, int length)
        {
            ref var source = ref MemoryMarshal.GetReference(span);
            var limits = span.Length;
            if ((uint)limits < (uint)length)
                ThrowHelper.ThrowNotEnoughBytes();
            span = MemoryMarshal.CreateReadOnlySpan(ref Unsafe.Add(ref source, length), limits - length);
            return MemoryMarshal.CreateReadOnlySpan(ref source, length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static T EnsureHandleEndian<T>(T item, bool swap) where T : unmanaged
        {
            if (typeof(T) == typeof(short))
                return swap is false ? item : (T)(object)BinaryPrimitives.ReverseEndianness((short)(object)item);
            else if (typeof(T) == typeof(int))
                return swap is false ? item : (T)(object)BinaryPrimitives.ReverseEndianness((int)(object)item);
            else if (typeof(T) == typeof(long))
                return swap is false ? item : (T)(object)BinaryPrimitives.ReverseEndianness((long)(object)item);
            else
                throw new NotSupportedException();
        }
    }
}
