using System;

namespace Mikodev.Binary.Internal
{
    internal static class BufferHelper
    {
#if DEBUG

        internal static byte[] GetBuffer() => Array.Empty<byte>();

#else
        [ThreadStatic]
        private static byte[] buffer;

        internal static byte[] GetBuffer()
        {
            const int Length = 1 << 16;
            var result = buffer;
            if (result == null)
                buffer = result = new byte[Length];
            return result;
        }
#endif
    }
}
