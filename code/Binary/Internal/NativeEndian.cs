using System;
using System.Runtime.CompilerServices;

namespace Mikodev.Binary.Internal
{
    internal static class NativeEndian
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Encode<T>(ref byte location, T item) where T : unmanaged
        {
            Unsafe.WriteUnaligned(ref location, item);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static T Decode<T>(ref byte location) where T : unmanaged
        {
            return Unsafe.ReadUnaligned<T>(ref location);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Encode<T>(ref Allocator allocator, T item) where T : unmanaged
        {
            Encode(ref Allocator.Assign(ref allocator, Unsafe.SizeOf<T>()), item);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static T Decode<T>(ReadOnlySpan<byte> span) where T : unmanaged
        {
            return Decode<T>(ref MemoryHelper.EnsureLength(span, Unsafe.SizeOf<T>()));
        }
    }
}
