namespace Mikodev.Binary.Components;

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

public abstract class NamedObjectConverter<T> : Converter<T?>
{
    private readonly int required;

    private readonly ImmutableArray<bool> optional;

    private readonly ImmutableArray<string> names;

    private readonly ByteViewDictionary<int> dictionary;

    protected NamedObjectConverter(Converter<string> converter, IEnumerable<string> names, IEnumerable<bool> optional)
    {
        ArgumentNullException.ThrowIfNull(converter);
        ArgumentNullException.ThrowIfNull(names);
        ArgumentNullException.ThrowIfNull(optional);
        var alpha = names.ToImmutableArray();
        var bravo = optional.ToImmutableArray();
        if (alpha.Length is 0 || bravo.Length is 0)
            throw new ArgumentException($"Sequence contains no element.");
        if (alpha.Length != bravo.Length)
            throw new ArgumentException($"Sequence lengths not match.");
        var selector = new Func<string, ReadOnlyMemory<byte>>(x => converter.Encode(x));
        var dictionary = BinaryObject.Create(alpha.Select(selector).ToImmutableArray());
        if (dictionary is null)
            throw new ArgumentException($"Named object error, duplicate binary string keys detected, type: {typeof(T)}, string converter type: {converter.GetType()}");
        this.names = alpha;
        this.optional = bravo;
        this.required = bravo.Count(x => x is false);
        this.dictionary = dictionary;
    }

    [DebuggerStepThrough, DoesNotReturn]
    private void ExceptKeyFound(int cursor)
    {
        throw new ArgumentException($"Named key '{this.names[cursor]}' already exists, type: {typeof(T)}");
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
        throw new ArgumentException($"Named key '{this.names[cursor]}' does not exist, type: {typeof(T)}");
    }

    private static T? Decode()
    {
        if (default(T) is not null)
            ThrowHelper.ThrowNotEnoughBytes();
        return default;
    }

    public sealed override T? Decode(in ReadOnlySpan<byte> span)
    {
        if (span.Length is 0)
            return Decode();

        // maybe 'StackOverflowException', just let it crash
        var optional = this.optional;
        var remain = this.required;
        var record = this.dictionary;
        var slices = (stackalloc long[optional.Length]);
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
        if (remain is not 0)
            ExceptNotFound(slices);
        return Decode(new NamedObjectParameter(span, slices));
    }

    public abstract T Decode(scoped NamedObjectParameter parameter);
}
