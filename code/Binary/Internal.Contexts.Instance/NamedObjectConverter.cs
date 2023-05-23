namespace Mikodev.Binary.Internal.Contexts.Instance;

using Mikodev.Binary.Components;
using Mikodev.Binary.External;
using Mikodev.Binary.Internal.Contexts.Template;
using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

internal sealed class NamedObjectConverter<T> : Converter<T?>
{
    private readonly int required;

    private readonly ImmutableArray<bool> optional;

    private readonly ImmutableArray<string> names;

    private readonly AllocatorAction<T> encode;

    private readonly ByteViewDictionary<int> dictionary;

    private readonly NamedObjectConstructor<T>? decode;

    public NamedObjectConverter(AllocatorAction<T> encode, NamedObjectConstructor<T>? decode, ByteViewDictionary<int> dictionary, ImmutableArray<string> names, ImmutableArray<bool> optional)
    {
        Debug.Assert(encode is not null);
        Debug.Assert(dictionary is not null);
        Debug.Assert(optional.Any());
        Debug.Assert(optional.Any(x => x is false));
        Debug.Assert(optional.Length == names.Length);
        this.names = names;
        this.encode = encode;
        this.decode = decode;
        this.optional = optional;
        this.required = optional.Count(x => x is false);
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

    public override void Encode(ref Allocator allocator, T? item)
    {
        if (item is null)
            return;
        this.encode.Invoke(ref allocator, item);
    }

    public override T? Decode(in ReadOnlySpan<byte> span)
    {
        var decode = this.decode;
        if (decode is null)
            return ThrowHelper.ThrowNoSuitableConstructor<T>();
        if (span.Length is 0 && default(T) is null)
            return default;
        if (span.Length is 0 && default(T) is not null)
            ThrowHelper.ThrowNotEnoughBytes();

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
            handle = NamedObjectTemplates.GetIndexData(offset, length);
            if (optional[cursor])
                continue;
            remain--;
        }

        Debug.Assert(remain >= 0);
        Debug.Assert(remain <= optional.Length);
        if (remain is not 0)
            ExceptNotFound(slices);
        return decode.Invoke(new NamedObjectConstructorParameter(span, slices));
    }
}
