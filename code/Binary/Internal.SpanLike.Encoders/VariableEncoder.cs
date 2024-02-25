namespace Mikodev.Binary.Internal.SpanLike.Encoders;

using Mikodev.Binary.Internal.SpanLike.Contexts;

internal sealed class VariableEncoder<T, E, A>(Converter<E> converter) : SpanLikeEncoder<T> where A : struct, ISpanLikeAdapter<T, E>
{
    private readonly Converter<E> converter = converter;

    public override void Encode(ref Allocator allocator, T? item)
    {
        A.Encode(ref allocator, item, this.converter);
    }

    public override void EncodeWithLengthPrefix(ref Allocator allocator, T? item)
    {
        var anchor = Allocator.Anchor(ref allocator);
        A.Encode(ref allocator, item, this.converter);
        Allocator.FinishAnchor(ref allocator, anchor);
    }
}
