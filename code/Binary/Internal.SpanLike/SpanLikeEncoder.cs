namespace Mikodev.Binary.Internal.SpanLike;

internal abstract class SpanLikeEncoder<T>
{
    public abstract void Encode(ref Allocator allocator, T? item);

    public abstract void EncodeWithLengthPrefix(ref Allocator allocator, T? item);
}
