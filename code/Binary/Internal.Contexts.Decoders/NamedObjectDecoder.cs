namespace Mikodev.Binary.Internal.Contexts.Decoders;

using Mikodev.Binary.External;
using Mikodev.Binary.Internal;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

internal sealed class NamedObjectDecoder
{
    private readonly int required;

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
        var data = head.Select(x => new ReadOnlyMemory<byte>(converter.Encode(x))).ToImmutableArray();
        var view = BinaryObject.Create(data);
        if (view is null)
            throw new ArgumentException($"Named object error, duplicate binary string keys detected, type: {type}, string converter type: {converter.GetType()}");
        this.type = type;
        this.view = view;
        this.names = head;
        this.optional = tail;
        this.required = tail.Count(x => x is false);
    }

    [DebuggerStepThrough, DoesNotReturn]
    private void ExceptKeyFound(int cursor)
    {
        throw new ArgumentException($"Named key '{this.names[cursor]}' already exists, type: {this.type}");
    }

    [DebuggerStepThrough, DoesNotReturn]
    private void ExceptNotFound(ReadOnlySpan<long> span)
    {
        var cursor = -1;
        var optional = this.optional;
        for (var i = 0; i < span.Length; i++)
        {
            if (span[i] is not 0 || optional[i])
                continue;
            cursor = i;
            break;
        }
        throw new ArgumentException($"Named key '{this.names[cursor]}' does not exist, type: {this.type}");
    }

    public void Invoke(ReadOnlySpan<byte> span, Span<long> slices)
    {
        // maybe 'StackOverflowException', just let it crash
        var optional = this.optional;
        var remain = this.required;
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
