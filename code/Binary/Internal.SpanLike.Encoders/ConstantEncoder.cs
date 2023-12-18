namespace Mikodev.Binary.Internal.SpanLike.Encoders;

using Mikodev.Binary.Internal.SpanLike.Contexts;
using System.Diagnostics;

internal sealed class ConstantEncoder<T, E, A>(Converter<E> converter) : SpanLikeEncoder<T> where A : struct, ISpanLikeAdapter<T, E>
{
    private readonly Converter<E> converter = converter;

    public override void Encode(ref Allocator allocator, T? item)
    {
        A.Encode(ref allocator, item, this.converter);
    }

    public override void EncodeWithLengthPrefix(ref Allocator allocator, T? item)
    {
        var length = A.Length(item);
        var converter = this.converter;
        Debug.Assert(converter.Length >= 1);
        var number = checked(converter.Length * length);
        var numberLength = NumberModule.EncodeLength((uint)number);
        NumberModule.Encode(ref Allocator.Assign(ref allocator, numberLength), (uint)number, numberLength);
        A.Encode(ref allocator, item, converter);
    }
}
