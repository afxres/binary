using Mikodev.Binary.Internal;
using System;

namespace Mikodev.Binary.Abstractions
{
    /// <summary>
    /// Fixed length type converter
    /// </summary>
    public abstract class ConstantConverter<T> : Converter<T>
    {
        protected ConstantConverter(int length) : base(length)
        {
            if (length > 0)
                return;
            ThrowHelper.ThrowArgumentOutOfRange(nameof(length));
        }

        public override void ToBytesWithMark(ref Allocator allocator, T item) => ToBytes(ref allocator, item);

        public override T ToValueWithMark(ref ReadOnlySpan<byte> span)
        {
            var item = ToValue(in span);
            span = span.Slice(Length);
            return item;
        }
    }
}
