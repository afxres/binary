namespace Mikodev.Binary.Internal.Contexts.Instance;

using Mikodev.Binary.Components;
using Mikodev.Binary.Internal.Contexts.Decoders;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

internal delegate T NamedObjectDecodeDelegate<out T>(scoped NamedObjectParameter parameter);

internal sealed class NamedObjectDelegateConverter<T> : Converter<T?>
{
    [AllowNull] private AllocatorAction<T> encode;

    private NamedObjectDecodeDelegate<T>? decode;

    [AllowNull] private NamedObjectDecoder invoke;

    public void Initialize(AllocatorAction<T> encode, NamedObjectDecodeDelegate<T>? decode, NamedObjectDecoder invoke)
    {
        Debug.Assert(this.encode is null);
        Debug.Assert(this.invoke is null);
        this.encode = encode;
        this.decode = decode;
        this.invoke = invoke;
    }

    public override void Encode(ref Allocator allocator, T? item)
    {
        if (item is null)
            return;
        RuntimeHelpers.EnsureSufficientExecutionStack();
        Debug.Assert(this.encode is not null);
        this.encode.Invoke(ref allocator, item);
    }

    public override T? Decode(in ReadOnlySpan<byte> span)
    {
        if (span.Length is 0)
            return ObjectModule.GetNullValueOrNotEnoughBytes<T>();

        var decode = this.decode;
        if (decode is null)
            ThrowHelper.ThrowNoSuitableConstructor<T>();

        Debug.Assert(this.invoke is not null);
        var invoke = this.invoke;
        var slices = (stackalloc long[invoke.Length]);
        invoke.Invoke(span, slices);
        return decode.Invoke(new NamedObjectParameter(span, slices));
    }
}
