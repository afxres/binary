using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Mikodev.Binary.Internal
{
    internal static partial class MemoryHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void EncodeNativeEndian<T>(ref byte location, T item) where T : unmanaged
        {
            Unsafe.WriteUnaligned(ref location, item);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static T DecodeNativeEndian<T>(ref byte location) where T : unmanaged
        {
            return Unsafe.ReadUnaligned<T>(ref location);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void EncodeNativeEndian<T>(ref Allocator allocator, T item) where T : unmanaged
        {
            EncodeNativeEndian(ref Allocator.Assign(ref allocator, Unsafe.SizeOf<T>()), item);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static T DecodeNativeEndian<T>(ReadOnlySpan<byte> span) where T : unmanaged
        {
            if (span.Length < Unsafe.SizeOf<T>())
                return ThrowHelper.ThrowNotEnoughBytes<T>();
            return DecodeNativeEndian<T>(ref MemoryMarshal.GetReference(span));
        }
    }
}
