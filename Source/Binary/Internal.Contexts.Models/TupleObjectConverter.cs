using System;

namespace Mikodev.Binary.Internal.Contexts.Models
{
    internal delegate void OfTupleObject<in T>(ref Allocator allocator, T item);

    internal delegate T ToTupleObject<out T>(ref ReadOnlySpan<byte> span);

    internal sealed class TupleObjectConverter<T> : Converter<T>
    {
        private readonly OfTupleObject<T> encode;

        private readonly ToTupleObject<T> decode;

        private readonly OfTupleObject<T> encodeAuto;

        private readonly ToTupleObject<T> decodeAuto;

        public TupleObjectConverter(OfTupleObject<T> encode, ToTupleObject<T> decode, OfTupleObject<T> encodeAuto, ToTupleObject<T> decodeAuto, int length) : base(length)
        {
            this.encode = encode;
            this.decode = decode;
            this.encodeAuto = encodeAuto;
            this.decodeAuto = decodeAuto;
        }

        public override void Encode(ref Allocator allocator, T item)
        {
            if (item is null)
                ThrowHelper.ThrowTupleNull(typeof(T));
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
                ThrowHelper.ThrowTupleNull(typeof(T));
            encodeAuto.Invoke(ref allocator, item);
        }

        public override T DecodeAuto(ref ReadOnlySpan<byte> span)
        {
            if (decodeAuto is null)
                return ThrowHelper.ThrowNoSuitableConstructor<T>();
            return decodeAuto.Invoke(ref span);
        }
    }
}
