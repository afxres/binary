using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Mikodev.Binary.Internal
{
    internal readonly ref struct MemorySlices
    {
        private readonly ReadOnlySpan<byte> span;

        private readonly Span<long> data;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MemorySlices(ReadOnlySpan<byte> span, Span<long> data)
        {
            Debug.Assert(span.Length > 0);
            Debug.Assert(data.Length > 0);
            this.span = span;
            this.data = data;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<byte> Invoke(int index)
        {
            var item = this.data[index];
            var body = this.span.Slice((int)(item >> 32), (int)item);
            return body;
        }
    }
}
