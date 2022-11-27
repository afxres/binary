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

internal delegate T NamedObjectDecodeDelegate<out T>(ReadOnlySpan<byte> span, ReadOnlySpan<long> data);

internal sealed class NamedObjectConverter<T> : Converter<T?>
{
    private readonly int memberCapacity;

    private readonly int memberRequired;

    private readonly ImmutableArray<bool> required;

    private readonly ImmutableArray<string> names;

    private readonly ByteViewDictionary<int> dictionary;

    private readonly EncodeDelegate<T> encode;

    private readonly NamedObjectDecodeDelegate<T> decode;

    public NamedObjectConverter(EncodeDelegate<T> encode, NamedObjectDecodeDelegate<T> decode, ImmutableArray<string> names, ImmutableArray<bool> required, ByteViewDictionary<int> dictionary)
    {
        Debug.Assert(dictionary is not null);
        Debug.Assert(names.Any());
        Debug.Assert(names.Length == required.Length);
        this.names = names;
        this.required = required;
        this.memberRequired = required.Count(x => x is true);
        this.memberCapacity = required.Length;
        this.dictionary = dictionary;
        this.encode = encode;
        this.decode = decode;
    }

    [DebuggerStepThrough, DoesNotReturn]
    private T ExceptKeyFound(int i) => throw new ArgumentException($"Named key '{this.names[i]}' already exists, type: {typeof(T)}");

    [DebuggerStepThrough, DoesNotReturn]
    private T ExceptNotFound(int i) => throw new ArgumentException($"Named key '{this.names[i]}' does not exist, type: {typeof(T)}");

    [DebuggerStepThrough, DoesNotReturn]
    private T ExceptNotFound(ReadOnlySpan<long> span)
    {
        var cursor = -1;
        var required = this.required;
        for (var i = 0; i < span.Length; i++)
        {
            if (span[i] is not 0 || required[i] is false)
                continue;
            cursor = i;
            break;
        }
        return ExceptNotFound(cursor);
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
        var required = this.required;
        var remain = this.memberRequired;
        var record = this.dictionary;
        var values = (stackalloc long[this.memberCapacity]);
        ref var source = ref MemoryMarshal.GetReference(span);

        var limits = span.Length;
        var offset = 0;
        var length = 0;
        while (limits - offset != length)
        {
            offset += length;
            length = NumberModule.DecodeEnsureBuffer(ref source, ref offset, limits);
            var cursor = record.GetValue(ref Unsafe.Add(ref source, offset), length);
            Debug.Assert(cursor is -1 || (uint)cursor < (uint)values.Length);
            offset += length;
            length = NumberModule.DecodeEnsureBuffer(ref source, ref offset, limits);
            if ((uint)cursor < (uint)values.Length)
            {
                ref var handle = ref values[cursor];
                if (handle is not 0)
                    return ExceptKeyFound(cursor);
                handle = NamedObjectTemplates.GetIndexData(offset, length);
                if (required[cursor] is false)
                    continue;
                remain--;
            }
        }

        Debug.Assert(remain >= 0);
        Debug.Assert(remain <= this.memberCapacity);
        if (remain is not 0)
            return ExceptNotFound(values);
        return decode.Invoke(span, values);
    }
}
