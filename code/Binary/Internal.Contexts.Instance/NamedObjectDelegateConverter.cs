﻿namespace Mikodev.Binary.Internal.Contexts.Instance;

using Mikodev.Binary.Components;
using System.Collections.Generic;

internal delegate T NamedObjectDecodeDelegate<out T>(scoped NamedObjectParameter parameter);

internal sealed class NamedObjectDelegateConverter<T>(Converter<string> converter, IEnumerable<string> names, IEnumerable<bool> optional, AllocatorAction<T> encode, NamedObjectDecodeDelegate<T>? decode) : NamedObjectConverter<T>(converter, names, optional)
{
    private readonly AllocatorAction<T> encode = encode;

    private readonly NamedObjectDecodeDelegate<T> decode = decode ?? (_ => ThrowHelper.ThrowNoSuitableConstructor<T>());

    public override T Decode(NamedObjectParameter parameter)
    {
        return this.decode.Invoke(parameter);
    }

    public override void Encode(ref Allocator allocator, T? item)
    {
        if (item is null)
            return;
        this.encode.Invoke(ref allocator, item);
    }
}
