namespace Mikodev.Binary.Internal.Sequence;

using Mikodev.Binary.Internal.Metadata;
using System;

internal sealed partial class SequenceConverter<T>(AllocatorAction<T?> encode, DecodePassSpanDelegate<T>? decode) : Converter<T>
{
    private readonly AllocatorAction<T?> encode = encode;

    private readonly DecodePassSpanDelegate<T> decode = decode ?? (_ => ThrowHelper.ThrowNoSuitableConstructor<T>());

    public override void Encode(ref Allocator allocator, T? item) => this.encode.Invoke(ref allocator, item);

    public override void EncodeAuto(ref Allocator allocator, T? item) => EncodeWithLengthPrefixInternal(ref allocator, item);

    public override void EncodeWithLengthPrefix(ref Allocator allocator, T? item) => EncodeWithLengthPrefixInternal(ref allocator, item);

    public override T Decode(in ReadOnlySpan<byte> span) => this.decode.Invoke(span);

    public override T DecodeAuto(ref ReadOnlySpan<byte> span) => this.decode.Invoke(Converter.DecodeWithLengthPrefix(ref span));

    public override T DecodeWithLengthPrefix(ref ReadOnlySpan<byte> span) => this.decode.Invoke(Converter.DecodeWithLengthPrefix(ref span));
}
