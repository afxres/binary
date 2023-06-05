namespace Mikodev.Binary.Components;

using Mikodev.Binary.Internal;
using System;

public abstract class CollectionConverter<T> : Converter<T>
{
    protected CollectionConverter() { }

    public abstract override void Encode(ref Allocator allocator, T? item);

    public override T Decode(in ReadOnlySpan<byte> span) => ThrowHelper.ThrowNoSuitableConstructor<T>();
}
