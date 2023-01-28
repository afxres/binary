namespace Mikodev.Binary.Internal.SpanLike.Encoders;

using Mikodev.Binary.Internal.SpanLike.Contexts;

internal sealed class VariableEncoder<T, E, A> : SpanLikeEncoder<T> where A : struct, ISpanLikeAdapter<T, E>
{
    private readonly Converter<E> converter;

    public VariableEncoder(Converter<E> converter)
    {
        this.converter = converter;
    }

    public override void Encode(ref Allocator allocator, T? item)
    {
        A.Encode(ref allocator, item, this.converter);
    }

    public override void EncodeWithLengthPrefix(ref Allocator allocator, T? item)
    {
        var anchor = Allocator.Anchor(ref allocator, sizeof(int));
        A.Encode(ref allocator, item, this.converter);
        Allocator.FinishAnchor(ref allocator, anchor);
    }
}
