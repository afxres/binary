using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Mikodev.Binary.Creators
{
    internal sealed class NullableConverter<T> : Converter<T?> where T : struct
    {
        private const int None = 0;

        private const int Some = 1;

        private readonly Converter<T> converter;

        public NullableConverter(Converter<T> converter) => this.converter = converter;

        [DebuggerStepThrough, DoesNotReturn]
        private static T? ThrowInvalid(int tag) => throw new ArgumentException($"Invalid nullable tag: {tag}, type: {typeof(T?)}");

        [DebuggerStepThrough]
        private static T? ThrowInvalidOrNull(int tag) => tag == None ? null : ThrowInvalid(tag);

        public override void Encode(ref Allocator allocator, T? item)
        {
            var head = item.HasValue ? Some : None;
            PrimitiveHelper.EncodeNumber(ref allocator, head);
            if (head == None)
                return;
            converter.Encode(ref allocator, item.GetValueOrDefault());
        }

        public override T? Decode(in ReadOnlySpan<byte> span)
        {
            var body = span;
            var head = PrimitiveHelper.DecodeNumber(ref body);
            if (head == Some)
                return converter.Decode(in body);
            return ThrowInvalidOrNull(head);
        }

        public override void EncodeAuto(ref Allocator allocator, T? item)
        {
            var head = item.HasValue ? Some : None;
            PrimitiveHelper.EncodeNumber(ref allocator, head);
            if (head == None)
                return;
            converter.EncodeAuto(ref allocator, item.GetValueOrDefault());
        }

        public override T? DecodeAuto(ref ReadOnlySpan<byte> span)
        {
            var head = PrimitiveHelper.DecodeNumber(ref span);
            if (head == Some)
                return converter.DecodeAuto(ref span);
            return ThrowInvalidOrNull(head);
        }
    }
}
