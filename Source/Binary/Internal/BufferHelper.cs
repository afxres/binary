using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Mikodev.Binary.Internal
{
    internal static class BufferHelper
    {
        [ThreadStatic]
        private static byte[] buffer;

        private static byte[] CreateBuffer()
        {
            Debug.Assert(buffer is null);
            const int Length = 1 << 16;
            var result = new byte[Length];
            buffer = result;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void AssignCopyForward3<T>(ref byte source) where T : unmanaged
        {
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref source, -3), Unsafe.ReadUnaligned<T>(ref source));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static byte[] GetBuffer()
        {
            return buffer ?? CreateBuffer();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void MemoryMoveForward3(ref byte source, uint length)
        {
            Debug.Assert(length < 32);
            for (var i = 0U; i < (length & 0xF8); i += 8)
                AssignCopyForward3<long>(ref Unsafe.Add(ref source, (int)i));
            if ((length & 4) != 0)
                AssignCopyForward3<uint>(ref Unsafe.Add(ref source, (int)(length & 24)));
            for (var i = length & 28; i < length; i++)
                AssignCopyForward3<byte>(ref Unsafe.Add(ref source, (int)i));
        }
    }
}
