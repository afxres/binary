namespace Mikodev.Binary.Features.Adapters;

using Mikodev.Binary.Features;
using Mikodev.Binary.Internal;
using Mikodev.Binary.Internal.Sequence;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

internal sealed class RawConverterSequenceAdapter<T, U> : SequenceAdapter<T> where U : struct, IRawConverter<T>
{
    public override void Encode(ref Allocator allocator, ReadOnlySpan<T> item)
    {
        Debug.Assert(U.Length >= 1);
        if (item.Length is 0)
            return;
        var length = U.Length;
        ref var target = ref Allocator.Assign(ref allocator, checked(length * item.Length));
        for (var i = 0; i < item.Length; i++)
            U.Encode(ref Unsafe.Add(ref target, length * i), item[i]);
        return;
    }

    public override MemoryBuffer<T> Decode(ReadOnlySpan<byte> span)
    {
        Debug.Assert(U.Length >= 1);
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
