using Mikodev.Binary.Internal.Delegates;
using System;

namespace Mikodev.Binary.Internal.Contexts.Implementations
{
    internal sealed class TupleObjectConverter<T> : Converter<T>
    {
        private readonly OfTupleObject<T> encode;

        private readonly ToTupleObject<T> decode;

        private readonly OfTupleObject<T> encodeWith;

        private readonly ToTupleObject<T> decodeWith;

        public TupleObjectConverter(OfTupleObject<T> encode, ToTupleObject<T> decode, OfTupleObject<T> encodeWith, ToTupleObject<T> decodeWith, int length) : base(length)
        {
            this.encode = encode;
            this.decode = decode;
            this.encodeWith = encodeWith;
            this.decodeWith = decodeWith;
        }

        public override void Encode(ref Allocator allocator, T item)
        {
            if (item == null)
                ThrowHelper.ThrowTupleNull(ItemType);
            encode.Invoke(ref allocator, item);
        }

        public override T Decode(in ReadOnlySpan<byte> span)
        {
            if (decode == null)
                return ThrowHelper.ThrowNoSuitableConstructor<T>();
            var temp = span;
            return decode.Invoke(ref temp);
        }

        public override void EncodeAuto(ref Allocator allocator, T item)
        {
            if (item == null)
                ThrowHelper.ThrowTupleNull(ItemType);
            encodeWith.Invoke(ref allocator, item);
        }

        public override T DecodeAuto(ref ReadOnlySpan<byte> span)
        {
            if (decodeWith == null)
                return ThrowHelper.ThrowNoSuitableConstructor<T>();
            return decodeWith.Invoke(ref span);
        }
    }
}
