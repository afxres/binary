using System;
using System.Diagnostics;

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
            this.decode = decode ?? ((ref ReadOnlySpan<byte> _) => ThrowHelper.ThrowNoSuitableConstructor<T>());
            this.decodeAuto = decodeAuto ?? ((ref ReadOnlySpan<byte> _) => ThrowHelper.ThrowNoSuitableConstructor<T>());
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
            Debug.Assert(this.decode is not null);
            var body = span;
            return this.decode.Invoke(ref body);
        }

        public override T DecodeAuto(ref ReadOnlySpan<byte> span)
        {
            return this.decodeAuto.Invoke(ref span);
        }
    }
}
