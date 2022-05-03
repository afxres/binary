namespace Mikodev.Binary.Internal.Sequence;

using System;

internal sealed partial class SpanLikeConverter<T, E> : Converter<T>
{
    private readonly int itemLength;

    private readonly SequenceAdapter<E> invoke;

    private readonly SequenceBuilder<T, E> create;

    public SpanLikeConverter(SequenceBuilder<T, E> create, Converter<E> converter)
    {
        this.invoke = SequenceAdapter.Create(converter);
        this.create = create;
        this.itemLength = converter.Length;
    }

    public override void Encode(ref Allocator allocator, T? item) => this.invoke.Encode(ref allocator, this.create.Handle(item));

    public override void EncodeAuto(ref Allocator allocator, T? item) => EncodeWithLengthPrefixInternal(ref allocator, item);

    public override void EncodeWithLengthPrefix(ref Allocator allocator, T? item) => EncodeWithLengthPrefixInternal(ref allocator, item);

    public override T Decode(in ReadOnlySpan<byte> span) => this.create.Invoke(span, this.invoke);

    public override T DecodeAuto(ref ReadOnlySpan<byte> span) => this.create.Invoke(Converter.DecodeWithLengthPrefix(ref span), this.invoke);

    public override T DecodeWithLengthPrefix(ref ReadOnlySpan<byte> span) => this.create.Invoke(Converter.DecodeWithLengthPrefix(ref span), this.invoke);
}
