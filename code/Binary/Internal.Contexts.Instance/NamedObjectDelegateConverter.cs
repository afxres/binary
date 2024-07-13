namespace Mikodev.Binary.Internal.Contexts.Instance;

using Mikodev.Binary.Components;
using Mikodev.Binary.Internal.Contexts.Decoders;
using System;
using System.Collections.Generic;

internal delegate T NamedObjectDecodeDelegate<out T>(scoped NamedObjectParameter parameter);

internal sealed class NamedObjectDelegateConverter<T>(Converter<string> converter, IEnumerable<string> names, IEnumerable<bool> optional, AllocatorAction<T> encode, NamedObjectDecodeDelegate<T>? decode) : Converter<T?>()
{
    private readonly AllocatorAction<T> encode = encode;

    private readonly NamedObjectDecodeDelegate<T>? decode = decode;

    private readonly NamedObjectDecoder invoke = new NamedObjectDecoder(converter, names, optional, typeof(T));

    public override void Encode(ref Allocator allocator, T? item)
    {
        if (item is null)
            return;
        this.encode.Invoke(ref allocator, item);
    }

    public override T? Decode(in ReadOnlySpan<byte> span)
    {
        if (span.Length is 0)
            return ObjectModule.GetNullValueOrNotEnoughBytes<T>();

        var decode = this.decode;
        if (decode is null)
            ThrowHelper.ThrowNoSuitableConstructor<T>();

        var invoke = this.invoke;
        var slices = (stackalloc long[invoke.Length]);
        invoke.Invoke(span, slices);
        return decode.Invoke(new NamedObjectParameter(span, slices));
    }
}
