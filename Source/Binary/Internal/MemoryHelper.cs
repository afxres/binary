using System;
using System.Runtime.CompilerServices;

namespace Mikodev.Binary.Internal
{
    internal static class MemoryHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static ref byte Add(ref byte source, int byteLength) => ref Unsafe.AddByteOffset(ref source, (IntPtr)byteLength);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static ref byte AsByte<T>(ref T source) where T : unmanaged => ref Unsafe.As<T, byte>(ref source);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Copy(ref byte target, ref byte source, int byteLength) => Unsafe.CopyBlockUnaligned(ref target, ref source, (uint)byteLength);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int SizeOf<T>() where T : unmanaged => Unsafe.SizeOf<T>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static T Get<T>(ref byte source) => Unsafe.ReadUnaligned<T>(ref source);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Set<T>(ref byte target, T item) => Unsafe.WriteUnaligned(ref target, item);
    }
}
