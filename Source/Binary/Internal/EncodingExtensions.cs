using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Mikodev.Binary.Internal
{
    internal static class EncodingExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe int GetBytes(this Encoding encoding, ref byte bytes, int byteCount, ref char chars, int charCount)
        {
            Debug.Assert(encoding != null);
            Debug.Assert(byteCount > 0);
            Debug.Assert(charCount > 0);
            fixed (char* srcptr = &chars)
            fixed (byte* dstptr = &bytes)
                return encoding.GetBytes(srcptr, charCount, dstptr, byteCount);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe string GetString(this Encoding encoding, ref byte bytes, int byteCount)
        {
            Debug.Assert(encoding != null);
            Debug.Assert(byteCount > 0);
            fixed (byte* srcptr = &bytes)
                return encoding.GetString(srcptr, byteCount);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string GetString(this Encoding encoding, ReadOnlySpan<byte> span)
        {
            Debug.Assert(encoding != null);
            var byteCount = span.Length;
            if (byteCount == 0)
                return string.Empty;
            ref var bytes = ref MemoryMarshal.GetReference(span);
            return GetString(encoding, ref bytes, byteCount);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe int GetByteCount(this Encoding encoding, ref char chars, int charCount)
        {
            Debug.Assert(encoding != null);
            Debug.Assert(charCount > 0);
            fixed (char* srcptr = &chars)
                return encoding.GetByteCount(srcptr, charCount);
        }
    }
}
