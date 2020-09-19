using System;
using System.Runtime.CompilerServices;

namespace Mikodev.Binary.Internal
{
    internal static partial class MemoryHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void EncodeLittleEndian<T>(ref byte location, T item) where T : unmanaged
        {
            Unsafe.WriteUnaligned(ref location, EnsureHandleEndian(item, BitConverter.IsLittleEndian is false));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static T DecodeLittleEndian<T>(ref byte location) where T : unmanaged
        {
            return EnsureHandleEndian(Unsafe.ReadUnaligned<T>(ref location), BitConverter.IsLittleEndian is false);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void EncodeLittleEndian<T>(ref Allocator allocator, T item) where T : unmanaged
        {
            EncodeLittleEndian(ref Allocator.Assign(ref allocator, Unsafe.SizeOf<T>()), item);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static T DecodeLittleEndian<T>(ReadOnlySpan<byte> span) where T : unmanaged
        {
            return DecodeLittleEndian<T>(ref EnsureLength(span, Unsafe.SizeOf<T>()));
        }
    }
}
