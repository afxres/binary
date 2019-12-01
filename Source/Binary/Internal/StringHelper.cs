using System.Buffers;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Mikodev.Binary.Internal
{
    internal static class StringHelper
    {
        private static readonly int[] maxByteCounts;

        private static readonly int[] maxCharCounts;

        static StringHelper()
        {
            const int MaxByteCountLimits = 64;
            const int MaxCharCountLimits = 128;
            maxByteCounts = Enumerable.Range(0, MaxByteCountLimits + 1).Select(Converter.Encoding.GetMaxByteCount).ToArray();
            maxCharCounts = Enumerable.Range(0, MaxCharCountLimits + 1).Select(Converter.Encoding.GetMaxCharCount).ToArray();
            maxByteCounts[0] = 0;
            maxCharCounts[0] = 0;
        }

        internal static unsafe string GetString(Encoding encoding, ref byte bytes, int byteCount)
        {
            Debug.Assert(encoding == Converter.Encoding);
            var counts = maxCharCounts;
            if (byteCount == 0 || (uint)byteCount >= (uint)counts.Length)
                return encoding.GetString(ref bytes, byteCount);
            var maxCharCount = counts[byteCount];
            Debug.Assert(maxCharCount > 0);
            var chars = stackalloc char[maxCharCount];
            int charCount;
            fixed (byte* srcptr = &bytes)
                charCount = encoding.GetChars(srcptr, byteCount, chars, maxCharCount);
            return new string(chars, 0, charCount);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe int GetMaxByteCountOrByteCount(Encoding encoding, char* chars, int charCount)
        {
            Debug.Assert(encoding == Converter.Encoding);
            Debug.Assert(charCount >= 0);
            var counts = maxByteCounts;
            if ((uint)charCount < (uint)counts.Length)
                return counts[charCount];
            return encoding.GetByteCount(chars, charCount);
        }
    }
}
