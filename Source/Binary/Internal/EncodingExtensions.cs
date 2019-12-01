using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace Mikodev.Binary.Internal
{
    internal static class EncodingExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe string GetString(this Encoding encoding, ref byte bytes, int byteCount)
        {
            Debug.Assert(encoding != null);
            Debug.Assert(byteCount >= 0);
            if (byteCount == 0)
                return string.Empty;
            fixed (byte* srcptr = &bytes)
                return encoding.GetString(srcptr, byteCount);
        }
    }
}
