using Mikodev.Binary.Internal;
using Mikodev.Binary.Internal.Delegates;
using System;

namespace Mikodev.Binary.Converters.Runtime
{
    internal sealed class TupleObjectConverter<T> : Converter<T>
    {
        private readonly EncodeWith<T> toBytes;

        private readonly DecodeWith<T> toValue;

        private readonly EncodeWith<T> toBytesWith;

        private readonly DecodeWith<T> toValueWith;

        public TupleObjectConverter(EncodeWith<T> toBytes, DecodeWith<T> toValue, EncodeWith<T> toBytesWith, DecodeWith<T> toValueWith, int length) : base(length)
        {
            this.toBytes = toBytes;
            this.toValue = toValue;
            this.toBytesWith = toBytesWith;
            this.toValueWith = toValueWith;
        }

        public override void Encode(ref Allocator allocator, T item)
        {
            if (item == null)
                ThrowHelper.ThrowTupleNull(ItemType);
            toBytes.Invoke(ref allocator, item);
        }

        public override T Decode(in ReadOnlySpan<byte> span)
        {
            if (toValue == null)
                return ThrowHelper.ThrowNoSuitableConstructor<T>();
            var temp = span;
            return toValue.Invoke(ref temp);
        }

        public override void EncodeAuto(ref Allocator allocator, T item)
        {
            if (item == null)
                ThrowHelper.ThrowTupleNull(ItemType);
            toBytesWith.Invoke(ref allocator, item);
        }

        public override T DecodeAuto(ref ReadOnlySpan<byte> span)
        {
            if (toValueWith == null)
                return ThrowHelper.ThrowNoSuitableConstructor<T>();
            return toValueWith.Invoke(ref span);
        }
    }
}
