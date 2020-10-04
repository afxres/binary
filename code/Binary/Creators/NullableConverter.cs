using Mikodev.Binary.Internal;
using System;

namespace Mikodev.Binary.Creators
{
    internal sealed class NullableConverter<T> : Converter<T?> where T : struct
    {
        private const int None = 0;

        private const int Some = 1;

        private readonly Converter<T> converter;

        public NullableConverter(Converter<T> converter) => this.converter = converter;

        public override void Encode(ref Allocator allocator, T? item)
        {
            var head = item.HasValue ? Some : None;
            PrimitiveHelper.EncodeNumber(ref allocator, head);
            if (head is None)
                return;
            converter.Encode(ref allocator, item.GetValueOrDefault());
        }

        public override void EncodeAuto(ref Allocator allocator, T? item)
        {
            var head = item.HasValue ? Some : None;
            PrimitiveHelper.EncodeNumber(ref allocator, head);
            if (head is None)
                return;
            converter.EncodeAuto(ref allocator, item.GetValueOrDefault());
        }

        public override T? Decode(in ReadOnlySpan<byte> span)
        {
            var body = span;
            var head = PrimitiveHelper.DecodeNumber(ref body);
            if (head is Some)
                return converter.Decode(in body);
            return head is None ? null : ThrowHelper.ThrowNullableTagInvalid<T>(head);
        }

        public override T? DecodeAuto(ref ReadOnlySpan<byte> span)
        {
            var head = PrimitiveHelper.DecodeNumber(ref span);
            if (head is Some)
                return converter.DecodeAuto(ref span);
            return head is None ? null : ThrowHelper.ThrowNullableTagInvalid<T>(head);
        }
    }
}
