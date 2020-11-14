﻿using System;

namespace Mikodev.Binary.Internal.SpanLike.Adapters
{
    internal sealed class VariableAdapter<T> : SpanLikeAdapter<T>
    {
        private readonly Converter<T> converter;

        public VariableAdapter(Converter<T> converter) => this.converter = converter;

        public override void Encode(ref Allocator allocator, ReadOnlySpan<T> item)
        {
            var converter = this.converter;
            foreach (var i in item)
                converter.EncodeAuto(ref allocator, i);
        }

        public override MemoryResult<T> Decode(ReadOnlySpan<byte> span)
        {
            static void Expand(ref T[] buffer, T item)
            {
                var source = buffer;
                var cursor = source.Length;
                buffer = new T[checked(cursor * 2)];
                Array.Copy(source, 0, buffer, 0, cursor);
                buffer[cursor] = item;
            }

            if (span.Length is 0)
                return new MemoryResult<T>(Array.Empty<T>(), 0);
            const int Initial = 8;
            var buffer = new T[Initial];
            var cursor = 0;
            var body = span;
            var converter = this.converter;
            while (body.Length is not 0)
            {
                var item = converter.DecodeAuto(ref body);
                if ((uint)cursor < (uint)buffer.Length)
                    buffer[cursor] = item;
                else
                    Expand(ref buffer, item);
                cursor++;
            }
            return new MemoryResult<T>(buffer, cursor);
        }
    }
}