using Mikodev.Binary.Internal.Extensions;
using System.Buffers;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Mikodev.Binary.Internal
{
    internal static class StringHelper
    {
        private const int MaxByteCountThreshold = 64;

        private const int MaxCharCountThreshold = 128;

        private static readonly int[] maxByteCounts = Enumerable.Range(0, MaxByteCountThreshold + 1).Select(Converter.Encoding.GetMaxByteCount).ToArray();

        private static readonly int[] maxCharCounts = Enumerable.Range(0, MaxCharCountThreshold + 1).Select(Converter.Encoding.GetMaxCharCount).ToArray();

        internal static unsafe string Decode(Encoding encoding, ref byte bytes, int byteCount)
        {
            Debug.Assert(encoding == Converter.Encoding);
            if (byteCount == 0)
                return string.Empty;
            if (byteCount > MaxCharCountThreshold)
                return encoding.GetString(ref bytes, byteCount);
            var maxCharCount = maxCharCounts[byteCount];
            var chars = stackalloc char[maxCharCount];
            int charCount;
            fixed (byte* srcptr = &bytes)
                charCount = encoding.GetChars(srcptr, byteCount, chars, maxCharCount);
            return new string(chars, 0, charCount);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int GetMaxByteCountByteCount(Encoding encoding, ref char chars, int charCount)
        {
            Debug.Assert(encoding == Converter.Encoding);
            Debug.Assert(charCount >= 0);
            return charCount == 0 ? 0 : charCount > MaxByteCountThreshold
                ? encoding.GetByteCount(ref chars, charCount)
                : maxByteCounts[charCount];
        }
    }
}
