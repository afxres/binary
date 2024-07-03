namespace Mikodev.Binary.Internal.Sequence;

using Mikodev.Binary.Internal.Metadata;
using System;

internal sealed class SequenceConverter<T>(AllocatorAction<T?> encode, DecodePassSpanDelegate<T>? decode) : Converter<T>
{
    private readonly AllocatorAction<T?> encode = encode;

    private readonly DecodePassSpanDelegate<T>? decode = decode;

    public override void Encode(ref Allocator allocator, T? item)
    {
        this.encode.Invoke(ref allocator, item);
    }

    public override T Decode(in ReadOnlySpan<byte> span)
    {
        var decode = this.decode;
        if (decode is null)
            ThrowHelper.ThrowNoSuitableConstructor<T>();
        return decode.Invoke(span);
    }
}
