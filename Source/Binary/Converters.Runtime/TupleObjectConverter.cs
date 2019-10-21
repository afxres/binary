using Mikodev.Binary.Internal;
using Mikodev.Binary.Internal.Delegates;
using System;

namespace Mikodev.Binary.Converters.Runtime
{
    internal sealed class TupleObjectConverter<T> : Converter<T>
    {
        private readonly EncodeWith<T> encode;

        private readonly DecodeWith<T> decode;

        private readonly EncodeWith<T> encodeWith;

        private readonly DecodeWith<T> decodeWith;

        public TupleObjectConverter(EncodeWith<T> encode, DecodeWith<T> decode, EncodeWith<T> encodeWith, DecodeWith<T> decodeWith, int length) : base(length)
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
