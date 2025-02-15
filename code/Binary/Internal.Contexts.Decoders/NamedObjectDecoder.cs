namespace Mikodev.Binary.Internal.Contexts.Decoders;

using Mikodev.Binary.External;
using Mikodev.Binary.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

internal sealed class NamedObjectDecoder
{
    private readonly Type type;

    private readonly ByteViewList view;

    private readonly bool[] optional;

    private readonly string[] names;

    public int Length => this.optional.Length;

    public NamedObjectDecoder(Converter<string> converter, IEnumerable<string> names, IEnumerable<bool> optional, Type type)
    {
        ArgumentNullException.ThrowIfNull(converter);
        ArgumentNullException.ThrowIfNull(names);
        ArgumentNullException.ThrowIfNull(optional);
        var head = names.ToArray();
        var tail = optional.ToArray();
        if (head.Length is 0 || tail.Length is 0)
            throw new ArgumentException($"Sequence contains no element.");
        if (head.Length != tail.Length)
            throw new ArgumentException($"Sequence lengths not match.");
        this.type = type;
        this.names = head;
        var view = BinaryObject.Create([.. head.Select(x => new ReadOnlyMemory<byte>(converter.Encode(x)))], out var error);
        Debug.Assert(view is null || error is -1);
        if (view is null)
            ExceptKeyFound(error);
        this.view = view;
        this.optional = tail;
    }

    [DebuggerStepThrough, DoesNotReturn]
    private void ExceptKeyFound(int cursor)
    {
        throw new ArgumentException($"Named key '{this.names[cursor]}' already exists, type: {this.type}");
    }

    [DebuggerStepThrough, DoesNotReturn]
    private void ExceptNotFound(int cursor)
    {
        throw new ArgumentException($"Named key '{this.names[cursor]}' does not exist, type: {this.type}");
    }

    public void Invoke(ReadOnlySpan<byte> span, Span<long> slices)
    {
        var optional = this.optional;
        var record = this.view;
        ref var source = ref MemoryMarshal.GetReference(span);

        var limits = span.Length;
        var offset = 0;
        var length = 0;
        while (limits - offset != length)
        {
            offset += length;
            length = NumberModule.DecodeEnsureBuffer(ref source, ref offset, limits);
            var cursor = record.Invoke(ref Unsafe.Add(ref source, offset), length);
            Debug.Assert(cursor is -1 || (uint)cursor < (uint)slices.Length);
            offset += length;
            length = NumberModule.DecodeEnsureBuffer(ref source, ref offset, limits);
            if ((uint)cursor >= (uint)slices.Length)
                continue;
            ref var handle = ref slices[cursor];
            if (handle is not 0)
                ExceptKeyFound(cursor);
            handle = (long)((ulong)(uint)offset << 32 | (uint)length);
        }

        for (var i = 0; i < slices.Length; i++)
        {
            if (slices[i] is not 0 || optional[i])
                continue;
            ExceptNotFound(i);
        }
    }
}
