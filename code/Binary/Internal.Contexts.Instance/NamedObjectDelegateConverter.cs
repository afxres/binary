namespace Mikodev.Binary.Internal.Contexts.Instance;

using Mikodev.Binary.Components;
using System;

internal delegate T NamedObjectDecodeDelegate<T>(scoped NamedObjectParameter parameter);

internal sealed class NamedObjectDelegateConverter<T> : Converter<T?>
{
    private readonly AllocatorAction<T> encode;

    private readonly NamedObjectDecoder invoke;

    private readonly NamedObjectDecodeDelegate<T>? decode;

    public NamedObjectDelegateConverter(NamedObjectDecoder invoke, AllocatorAction<T> encode, NamedObjectDecodeDelegate<T>? decode)
    {
        this.encode = encode;
        this.invoke = invoke;
        this.decode = decode;
    }

    public override void Encode(ref Allocator allocator, T? item)
    {
        if (item is null)
            return;
        this.encode.Invoke(ref allocator, item);
    }

    public override T? Decode(in ReadOnlySpan<byte> span)
    {
        if (span.Length is 0)
            return ObjectModule.GetNullValueOrThrow<T>();
        var decode = this.decode;
        if (decode is null)
            return ThrowHelper.ThrowNoSuitableConstructor<T>();

        // maybe 'StackOverflowException', just let it crash
        var invoke = this.invoke;
        var slices = (stackalloc long[invoke.MemberLength]);
        invoke.Invoke(span, slices);
        return decode.Invoke(new NamedObjectParameter(span, slices));
    }
}
