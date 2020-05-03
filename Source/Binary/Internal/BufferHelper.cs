using System;
using System.Runtime.CompilerServices;

namespace Mikodev.Binary.Internal
{
    internal sealed class BufferHelper
    {
        private static readonly BufferHelper GlobalSharedInstance = new BufferHelper(Array.Empty<byte>());

        [ThreadStatic]
        private static BufferHelper ThreadStaticInstance;

        private readonly byte[] buffer;

        private bool borrow;

        private BufferHelper(byte[] buffer) => this.buffer = buffer;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Span<byte> Intent(BufferHelper buffer) => new Span<byte>(buffer.buffer);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Return(BufferHelper buffer) => buffer.borrow = false;

        internal static BufferHelper Borrow()
        {
            var result = ThreadStaticInstance;
            if (result is null)
                ThreadStaticInstance = result = new BufferHelper(new byte[64 * 1024]);
            if (result.borrow)
                return GlobalSharedInstance;
            result.borrow = true;
            return result;
        }
    }
}
