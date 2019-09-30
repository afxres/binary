using Mikodev.Binary.Internal;
using Mikodev.Binary.Internal.Delegates;
using System;

namespace Mikodev.Binary.Converters.Runtime
{
    internal sealed class TupleObjectConverter<T> : Converter<T>
    {
        private readonly ToBytesWith<T> toBytes;

        private readonly ToValueWith<T> toValue;

        private readonly ToBytesWith<T> toBytesWith;

        private readonly ToValueWith<T> toValueWith;

        public TupleObjectConverter(ToBytesWith<T> toBytes, ToValueWith<T> toValue, ToBytesWith<T> toBytesWith, ToValueWith<T> toValueWith, int length) : base(length)
        {
            this.toBytes = toBytes;
            this.toValue = toValue;
            this.toBytesWith = toBytesWith;
            this.toValueWith = toValueWith;
        }

        public override void ToBytes(ref Allocator allocator, T item)
        {
            if (item == null)
                ThrowHelper.ThrowTupleNull(ItemType);
            toBytes.Invoke(ref allocator, item);
        }

        public override T ToValue(in ReadOnlySpan<byte> span)
        {
            var temp = span;
            return toValue.Invoke(ref temp);
        }

        public override void ToBytesWithMark(ref Allocator allocator, T item)
        {
            if (item == null)
                ThrowHelper.ThrowTupleNull(ItemType);
            toBytesWith.Invoke(ref allocator, item);
        }

        public override T ToValueWithMark(ref ReadOnlySpan<byte> span)
        {
            return toValueWith.Invoke(ref span);
        }
    }
}
