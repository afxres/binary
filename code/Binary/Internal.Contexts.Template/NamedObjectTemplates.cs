using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Mikodev.Binary.Internal.Contexts.Template
{
    internal static class NamedObjectTemplates
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static long GetIndexData(int offset, int length)
        {
            return (long)(((ulong)(uint)offset << 32) | (uint)length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static ReadOnlySpan<byte> GetIndexSpan(ReadOnlySpan<byte> span, ReadOnlySpan<long> data, int index)
        {
            Debug.Assert(span.Length is not 0);
            Debug.Assert(data.Length is not 0);
            var item = data[index];
            var body = span.Slice((int)(item >> 32), (int)item);
            return body;
        }
    }
}
