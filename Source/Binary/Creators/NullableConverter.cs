using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Mikodev.Binary.Creators
{
    internal sealed class NullableConverter<T> : Converter<T?> where T : struct
    {
        private const byte None = 0;

        private const byte Some = 1;

        private readonly Converter<T> converter;

        public NullableConverter(Converter<T> converter) => this.converter = converter;

        [DebuggerStepThrough]
        private T? ThrowInvalid(int tag) => throw new ArgumentException($"Invalid nullable tag: {tag}, type: {ItemType}");

        [DebuggerStepThrough, MethodImpl(MethodImplOptions.AggressiveInlining)]
        private T? ThrowInvalidOrNull(int tag) => tag == None ? null : ThrowInvalid(tag);

        public override void Encode(ref Allocator allocator, T? item)
        {
            var header = item == null ? None : Some;
            Allocator.Append(ref allocator, header);
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
            return ThrowInvalidOrNull(head);
        }

        public override void EncodeAuto(ref Allocator allocator, T? item)
        {
            var header = item == null ? None : Some;
            Allocator.Append(ref allocator, header);
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
            return ThrowInvalidOrNull(head);
        }
    }
}
