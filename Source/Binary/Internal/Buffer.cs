using System;

namespace Mikodev.Binary.Internal
{
    internal static class Buffer
    {
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
    }
}
