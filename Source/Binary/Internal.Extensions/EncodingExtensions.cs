using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Mikodev.Binary.Internal.Extensions
{
    internal static class EncodingExtensions
    {
        internal static unsafe int GetBytes(this Encoding encoding, ref byte bytes, int byteCount, ref char chars, int charCount)
        {
            Debug.Assert(encoding != null);
            Debug.Assert(byteCount > 0);
            Debug.Assert(charCount > 0);
            fixed (char* srcptr = &chars)
            fixed (byte* dstptr = &bytes)
                return encoding.GetBytes(srcptr, charCount, dstptr, byteCount);
        }

        internal static unsafe string GetString(this Encoding encoding, ref byte bytes, int byteCount)
        {
            Debug.Assert(encoding != null);
            Debug.Assert(byteCount > 0);
            fixed (byte* srcptr = &bytes)
                return encoding.GetString(srcptr, byteCount);
        }

        internal static string GetString(this Encoding encoding, in ReadOnlySpan<byte> span)
        {
            Debug.Assert(encoding != null);
            var byteCount = span.Length;
            if (byteCount == 0)
                return string.Empty;
            ref byte bytes = ref MemoryMarshal.GetReference(span);
            return GetString(encoding, ref bytes, byteCount);
        }

        internal static unsafe int GetByteCount(this Encoding encoding, ref char chars, int charCount)
        {
            Debug.Assert(encoding != null);
            Debug.Assert(charCount > 0);
            fixed (char* srcptr = &chars)
                return encoding.GetByteCount(srcptr, charCount);
        }
    }
}
