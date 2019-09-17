using System;
using System.Runtime.CompilerServices;

namespace Mikodev.Binary.Internal
{
    internal static class Memory
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref byte Add(ref byte source, int byteCount) => ref Unsafe.AddByteOffset(ref source, (IntPtr)byteCount);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T Add<T>(ref T source, int itemCount) => ref Unsafe.Add(ref source, itemCount);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref byte AsByte<T>(ref T source) where T : unmanaged => ref Unsafe.As<T, byte>(ref source);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Copy(byte[] target, byte[] source, int byteCount) => Unsafe.CopyBlock(ref target[0], ref source[0], (uint)byteCount);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Copy(ref byte target, ref byte source, int byteCount) => Unsafe.CopyBlockUnaligned(ref target, ref source, (uint)byteCount);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int SizeOf<T>() where T : unmanaged => Unsafe.SizeOf<T>();
    }
}
