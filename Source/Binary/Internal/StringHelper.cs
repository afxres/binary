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

        internal static unsafe string GetString(Encoding encoding, byte* source, int length)
        {
            Debug.Assert(encoding != null);
            Debug.Assert(length >= 0);
            if (length == 0)
                return string.Empty;
            int[] counts;
            if (!ReferenceEquals(encoding, Converter.Encoding) || (uint)length >= (uint)(counts = maxCharCounts).Length)
                return encoding.GetString(source, length);
            var dstmax = counts[length];
            Debug.Assert(dstmax > 0);
            var dstptr = stackalloc char[dstmax];
            int dstlen;
            dstlen = encoding.GetChars(source, length, dstptr, dstmax);
            return new string(dstptr, 0, dstlen);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe int GetMaxByteCount(Encoding encoding, char* source, int length)
        {
            Debug.Assert(encoding != null);
            Debug.Assert(length >= 0);
            if (length == 0)
                return 0;
            int[] counts;
            if (!ReferenceEquals(encoding, Converter.Encoding) || (uint)length >= (uint)(counts = maxByteCounts).Length)
                return encoding.GetByteCount(source, length);
            return counts[length];
        }
    }
}
