using System;

namespace Mikodev.Binary.Internal.Contexts.Models
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
            if (item is null)
                ThrowHelper.ThrowTupleNull(ItemType);
            encode.Invoke(ref allocator, item);
        }

        public override T Decode(in ReadOnlySpan<byte> span)
        {
            if (decode is null)
                return ThrowHelper.ThrowNoSuitableConstructor<T>();
            var body = span;
            return decode.Invoke(ref body);
        }

        public override void EncodeAuto(ref Allocator allocator, T item)
        {
            if (item is null)
                ThrowHelper.ThrowTupleNull(ItemType);
            encodeWith.Invoke(ref allocator, item);
        }

        public override T DecodeAuto(ref ReadOnlySpan<byte> span)
        {
            if (decodeWith is null)
                return ThrowHelper.ThrowNoSuitableConstructor<T>();
            return decodeWith.Invoke(ref span);
        }
    }
}
