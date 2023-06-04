namespace Mikodev.Binary.Internal.Contexts.Instance;

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

internal sealed partial class NamedObjectDecoder
{
    public int MemberLength => this.capacity;

    public void Invoke(ReadOnlySpan<byte> span, Span<long> slices)
    {
        var optional = this.optional;
        var remain = this.required;
        var record = this.dictionary;
        ref var source = ref MemoryMarshal.GetReference(span);

        var limits = span.Length;
        var offset = 0;
        var length = 0;
        while (limits - offset != length)
        {
            offset += length;
            length = NumberModule.DecodeEnsureBuffer(ref source, ref offset, limits);
            var cursor = record.GetValue(ref Unsafe.Add(ref source, offset), length);
            Debug.Assert(cursor is -1 || (uint)cursor < (uint)slices.Length);
            offset += length;
            length = NumberModule.DecodeEnsureBuffer(ref source, ref offset, limits);
            if ((uint)cursor >= (uint)slices.Length)
                continue;
            ref var handle = ref slices[cursor];
            if (handle is not 0)
                ExceptKeyFound(cursor);
            handle = (long)(((ulong)(uint)offset << 32) | (uint)length);
            if (optional[cursor])
                continue;
            remain--;
        }

        Debug.Assert(remain >= 0);
        Debug.Assert(remain <= optional.Length);
        if (remain is 0)
            return;
        ExceptNotFound(slices);
    }
}
