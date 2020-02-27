using System.Buffers;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Mikodev.Binary.Internal
{
    internal static class StringHelper
    {
        private static readonly int[] MaxByteCounts;

        private static readonly int[] MaxCharCounts;

        static StringHelper()
        {
            const int MaxByteCountLimits = 64;
            const int MaxCharCountLimits = 128;
            MaxByteCounts = Enumerable.Range(0, MaxByteCountLimits + 1).Select(Converter.Encoding.GetMaxByteCount).ToArray();
            MaxCharCounts = Enumerable.Range(0, MaxCharCountLimits + 1).Select(Converter.Encoding.GetMaxCharCount).ToArray();
            MaxByteCounts[0] = 0;
            MaxCharCounts[0] = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe int GetBytes(Encoding encoding, ref char source, int sourceLength, ref byte target, int targetLength)
        {
            Debug.Assert(encoding != null);
            Debug.Assert(sourceLength >= 0);
            Debug.Assert(targetLength >= 0);
            fixed (byte* dstptr = &target)
            fixed (char* srcptr = &source)
                return encoding.GetBytes(srcptr, sourceLength, dstptr, targetLength);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe int GetMaxByteCount(Encoding encoding, ref char source, int sourceLength)
        {
            Debug.Assert(encoding != null);
            Debug.Assert(sourceLength >= 0);
            if (sourceLength == 0)
                return 0;
            int[] counts;
            if (encoding != Converter.Encoding || (uint)sourceLength >= (uint)(counts = MaxByteCounts).Length)
                fixed (char* srcptr = &source)
                    return encoding.GetByteCount(srcptr, sourceLength);
            return counts[sourceLength];
        }

        internal static unsafe string GetString(Encoding encoding, ref byte source, int sourceLength)
        {
            Debug.Assert(encoding != null);
            Debug.Assert(sourceLength >= 0);
            if (sourceLength == 0)
                return string.Empty;
            int[] counts;
            if (encoding != Converter.Encoding || (uint)sourceLength >= (uint)(counts = MaxCharCounts).Length)
                fixed (byte* srcptr = &source)
                    return encoding.GetString(srcptr, sourceLength);
            var targetLimits = counts[sourceLength];
            Debug.Assert(targetLimits > 0);
            var dstptr = stackalloc char[targetLimits];
            int targetLength;
            fixed (byte* srcptr = &source)
                targetLength = encoding.GetChars(srcptr, sourceLength, dstptr, targetLimits);
            return new string(dstptr, 0, targetLength);
        }
    }
}
