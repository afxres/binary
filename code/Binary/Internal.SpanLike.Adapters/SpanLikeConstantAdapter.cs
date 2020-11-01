﻿using Mikodev.Binary.Internal.Sequence;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Mikodev.Binary.Internal.SpanLike.Adapters
{
    internal sealed class SpanLikeConstantAdapter<T> : SpanLikeAdapter<T>
    {
        private readonly Converter<T> converter;

        public SpanLikeConstantAdapter(Converter<T> converter) => this.converter = converter;

        public override void Encode(ref Allocator allocator, ReadOnlySpan<T> item)
        {
            var converter = this.converter;
            foreach (var i in item)
                converter.Encode(ref allocator, i);
        }

        public override MemoryResult<T> Decode(ReadOnlySpan<byte> span)
        {
            var limits = span.Length;
            if (limits is 0)
                return new MemoryResult<T>(Array.Empty<T>(), 0);
            var converter = this.converter;
            var length = converter.Length;
            var capacity = SequenceMethods.GetCapacity<T>(limits, length);
            var result = new T[capacity];
            ref var source = ref MemoryMarshal.GetReference(span);
            for (var i = 0; i < capacity; i++)
                result[i] = converter.Decode(MemoryMarshal.CreateReadOnlySpan(ref Unsafe.Add(ref source, length * i), length));
            return new MemoryResult<T>(result, capacity);
        }
    }
}
