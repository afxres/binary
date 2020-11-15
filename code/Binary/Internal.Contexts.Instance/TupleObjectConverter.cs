using System;

namespace Mikodev.Binary.Internal.Contexts.Instance
{
    internal delegate void TupleObjectEncoder<in T>(ref Allocator allocator, T item);

    internal delegate T TupleObjectDecoder<out T>(ref ReadOnlySpan<byte> span);

    internal sealed class TupleObjectConverter<T> : Converter<T>
    {
        private readonly TupleObjectEncoder<T> encode;

        private readonly TupleObjectEncoder<T> encodeAuto;

        private readonly TupleObjectDecoder<T> decode;

        private readonly TupleObjectDecoder<T> decodeAuto;

        public TupleObjectConverter(TupleObjectEncoder<T> encode, TupleObjectEncoder<T> encodeAuto, TupleObjectDecoder<T> decode, TupleObjectDecoder<T> decodeAuto, int length) : base(length)
        {
            this.encode = encode;
            this.encodeAuto = encodeAuto;
            this.decode = decode;
            this.decodeAuto = decodeAuto;
        }

        public override void Encode(ref Allocator allocator, T item)
        {
            if (item is null)
                ThrowHelper.ThrowTupleNull<T>();
            this.encode.Invoke(ref allocator, item);
        }

        public override void EncodeAuto(ref Allocator allocator, T item)
        {
            if (item is null)
                ThrowHelper.ThrowTupleNull<T>();
            this.encodeAuto.Invoke(ref allocator, item);
        }

        public override T Decode(in ReadOnlySpan<byte> span)
        {
            var decode = this.decode;
            if (decode is null)
                return ThrowHelper.ThrowNoSuitableConstructor<T>();
            var body = span;
            return decode.Invoke(ref body);
        }

        public override T DecodeAuto(ref ReadOnlySpan<byte> span)
        {
            var decodeAuto = this.decodeAuto;
            if (decodeAuto is null)
                return ThrowHelper.ThrowNoSuitableConstructor<T>();
            return decodeAuto.Invoke(ref span);
        }
    }
}
