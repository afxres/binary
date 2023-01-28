namespace Mikodev.Binary.Internal.SpanLike.Encoders;

using Mikodev.Binary.Internal.SpanLike.Contexts;
using System.Diagnostics;

internal sealed class ConstantForwardEncoder<T, E, A> : SpanLikeEncoder<T> where A : struct, ISpanLikeAdapter<T, E>
{
    private readonly int itemLength;

    private readonly SpanLikeForwardEncoder<E> encoder;

    public ConstantForwardEncoder(SpanLikeForwardEncoder<E> encoder, int itemLength)
    {
        Debug.Assert(itemLength > 0);
        this.encoder = encoder;
        this.itemLength = itemLength;
    }

    public override void Encode(ref Allocator allocator, T? item)
    {
        this.encoder.Encode(ref allocator, A.AsSpan(item));
    }

    public override void EncodeWithLengthPrefix(ref Allocator allocator, T? item)
    {
        var length = A.Length(item);
        var itemLength = this.itemLength;
        Debug.Assert(itemLength >= 1);
        var number = checked(itemLength * length);
        var numberLength = NumberModule.EncodeLength((uint)number);
        NumberModule.Encode(ref Allocator.Assign(ref allocator, numberLength), (uint)number, numberLength);
        this.encoder.Encode(ref allocator, A.AsSpan(item));
    }
}
