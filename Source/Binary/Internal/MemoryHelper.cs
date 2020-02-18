using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Mikodev.Binary.Internal
{
    internal static class MemoryHelper
    {
        [StructLayout(LayoutKind.Sequential, Size = 1)]
        private struct U1 { }

        [StructLayout(LayoutKind.Sequential, Size = 2)]
        private struct U2 { }

        [StructLayout(LayoutKind.Sequential, Size = 4)]
        private struct U4 { }

        [StructLayout(LayoutKind.Sequential, Size = 8)]
        private struct U8 { }

        [StructLayout(LayoutKind.Sequential, Size = 16)]
        private struct UX { }

        internal static void MemoryMoveForward3(ref byte source, uint length)
        {
            Debug.Assert(length > 0 && length < 32);
            if ((length & 16) != 0)
                Unsafe.As<byte, UX>(ref Unsafe.Add(ref source, -3)) = Unsafe.As<byte, UX>(ref Unsafe.Add(ref source, 0));
            if ((length & 8) != 0)
                Unsafe.As<byte, U8>(ref Unsafe.Add(ref source, (int)(length & 16) - 3)) = Unsafe.As<byte, U8>(ref Unsafe.Add(ref source, (int)(length & 16)));
            if ((length & 4) != 0)
                Unsafe.As<byte, U4>(ref Unsafe.Add(ref source, (int)(length & 24) - 3)) = Unsafe.As<byte, U4>(ref Unsafe.Add(ref source, (int)(length & 24)));
            if ((length & 2) != 0)
                Unsafe.As<byte, U2>(ref Unsafe.Add(ref source, (int)(length & 28) - 3)) = Unsafe.As<byte, U2>(ref Unsafe.Add(ref source, (int)(length & 28)));
            if ((length & 1) != 0)
                Unsafe.As<byte, U1>(ref Unsafe.Add(ref source, (int)(length & 30) - 3)) = Unsafe.As<byte, U1>(ref Unsafe.Add(ref source, (int)(length & 30)));
        }
    }
}
