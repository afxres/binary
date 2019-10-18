using Mikodev.Binary.Internal;
using System;

namespace Mikodev.Binary.Abstractions
{
    /// <summary>
    /// Fixed length type converter
    /// </summary>
    [Obsolete]
    public abstract class ConstantConverter<T> : Converter<T>
    {
        protected ConstantConverter(int length) : base(length)
        {
            if (length > 0)
                return;
            ThrowHelper.ThrowArgumentOutOfRange(nameof(length));
        }

        public override void EncodeAuto(ref Allocator allocator, T item) => Encode(ref allocator, item);

        public override T DecodeAuto(ref ReadOnlySpan<byte> span)
        {
            var item = Decode(in span);
            span = span.Slice(Length);
            return item;
        }
    }
}
