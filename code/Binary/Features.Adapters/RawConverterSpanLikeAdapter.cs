namespace Mikodev.Binary.Features.Adapters;

using Mikodev.Binary.Features.Contexts;
using Mikodev.Binary.Internal;
using Mikodev.Binary.Internal.Sequence;
using Mikodev.Binary.Internal.SpanLike;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

#if NET6_0_OR_GREATER
[RequiresPreviewFeatures]
internal sealed class RawConverterSpanLikeAdapter<T, U> : SpanLikeAdapter<T> where U : struct, IRawConverter<T>
{
    public override void Encode(ref Allocator allocator, ReadOnlySpan<T> item)
    {
        var length = U.Length;
        ref var target = ref Allocator.Assign(ref allocator, checked(length * item.Length));
        for (var i = 0; i < item.Length; i++)
            U.Encode(ref Unsafe.Add(ref target, length * i), item[i]);
        return;
    }

    public override MemoryBuffer<T> Decode(ReadOnlySpan<byte> span)
    {
        var limits = span.Length;
        if (limits is 0)
            return new MemoryBuffer<T>(Array.Empty<T>(), 0);
        var length = U.Length;
        var capacity = SequenceMethods.GetCapacity<T>(limits, length);
        var result = new T[capacity];
        ref var source = ref MemoryMarshal.GetReference(span);
        for (var i = 0; i < capacity; i++)
            result[i] = U.Decode(ref Unsafe.Add(ref source, length * i));
        return new MemoryBuffer<T>(result, capacity);
    }
}
#endif
