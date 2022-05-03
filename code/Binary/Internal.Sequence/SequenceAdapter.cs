namespace Mikodev.Binary.Internal.Sequence;

using System;

internal abstract class SequenceAdapter<T>
{
    public abstract void Encode(ref Allocator allocator, ReadOnlySpan<T> item);

    public abstract MemoryBuffer<T> Decode(ReadOnlySpan<byte> span);
}
