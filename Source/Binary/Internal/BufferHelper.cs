using System;

namespace Mikodev.Binary.Internal
{
    internal static class BufferHelper
    {
        [ThreadStatic]
        private static byte[] buffer;

        internal static byte[] GetBuffer()
        {
            const int Length = 1 << 16;
            var result = buffer;
            if (result is null)
                buffer = result = new byte[Length];
            return result;
        }
    }
}
