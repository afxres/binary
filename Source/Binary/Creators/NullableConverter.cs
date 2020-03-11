using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Mikodev.Binary.Creators
{
    internal sealed class NullableConverter<T> : Converter<T?> where T : struct
    {
        private const int None = 0;

        private const int Some = 1;

        private readonly Converter<T> converter;

        public NullableConverter(Converter<T> converter) => this.converter = converter;

        [DebuggerStepThrough]
        private T? ThrowInvalid(int tag) => throw new ArgumentException($"Invalid nullable tag: {tag}, type: {ItemType}");

        [DebuggerStepThrough, MethodImpl(MethodImplOptions.AggressiveInlining)]
        private T? ThrowInvalidOrNull(int tag) => tag == None ? null : ThrowInvalid(tag);

        public override void Encode(ref Allocator allocator, T? item)
        {
            var head = item.HasValue ? Some : None;
            Allocator.Append(ref allocator, (byte)head);
            if (head == None)
                return;
            converter.Encode(ref allocator, item.GetValueOrDefault());
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
            var head = item.HasValue ? Some : None;
            Allocator.Append(ref allocator, (byte)head);
            if (head == None)
                return;
            converter.EncodeAuto(ref allocator, item.GetValueOrDefault());
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
