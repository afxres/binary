using Mikodev.Binary.Internal;
using System;

namespace Mikodev.Binary.Creators
{
    internal sealed class NullableConverter<T> : Converter<T?> where T : struct
    {
        private const byte None = 0;

        private const byte Some = 1;

        private readonly Converter<T> converter;

        public NullableConverter(Converter<T> converter) : base(0)
        {
            this.converter = converter;
        }

        public override void Encode(ref Allocator allocator, T? item)
        {
            var header = item == null ? None : Some;
            allocator.AllocateReference(sizeof(byte)) = header;
            if (!(item is T result))
                return;
            converter.Encode(ref allocator, result);
        }

        public override T? Decode(in ReadOnlySpan<byte> span)
        {
            var head = span[0];
            var body = span.Slice(sizeof(byte));
            if (head == Some)
                return converter.Decode(in body);
            if (head != None)
                ThrowHelper.ThrowInvalidNullableTag(head, ItemType);
            return null;
        }

        public override void EncodeAuto(ref Allocator allocator, T? item)
        {
            var header = item == null ? None : Some;
            allocator.AllocateReference(sizeof(byte)) = header;
            if (!(item is T result))
                return;
            converter.EncodeAuto(ref allocator, result);
        }

        public override T? DecodeAuto(ref ReadOnlySpan<byte> span)
        {
            var head = span[0];
            span = span.Slice(sizeof(byte));
            if (head == Some)
                return converter.DecodeAuto(ref span);
            if (head != None)
                ThrowHelper.ThrowInvalidNullableTag(head, ItemType);
            return null;
        }
    }
}
