using Mikodev.Binary.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Mikodev.Binary.Creators.Endianness
{
    internal sealed class OriginalEndiannessConverter<T> : Converter<T> where T : unmanaged
    {
        public OriginalEndiannessConverter() : base(Memory.SizeOf<T>())
        {
            Debug.Assert(typeof(T) == typeof(Guid) || new List<int> { 1, 2, 4, 8 }.Contains(Memory.SizeOf<T>()));
        }

        public override void ToBytes(ref Allocator allocator, T item)
        {
            Memory.Set(ref allocator.AllocateReference(Memory.SizeOf<T>()), item);
        }

        public override T ToValue(in ReadOnlySpan<byte> span)
        {
            if (span.Length < Memory.SizeOf<T>())
                return ThrowHelper.ThrowNotEnoughBytes<T>();
            ref var source = ref MemoryMarshal.GetReference(span);
            return Memory.Get<T>(ref source);
        }

        public override void ToBytesWithMark(ref Allocator allocator, T item)
        {
            Memory.Set(ref allocator.AllocateReference(Memory.SizeOf<T>()), item);
        }

        public override T ToValueWithMark(ref ReadOnlySpan<byte> span)
        {
            // take reference first, then check bounds via slice method
            ref var source = ref MemoryMarshal.GetReference(span);
            span = span.Slice(Memory.SizeOf<T>());
            return Memory.Get<T>(ref source);
        }

        public override void ToBytesWithLengthPrefix(ref Allocator allocator, T item)
        {
            ref var location = ref allocator.AllocateReference(Memory.SizeOf<T>() + 1);
            location = (byte)Memory.SizeOf<T>();
            Memory.Set(ref Memory.Add(ref location, 1), item);
        }

        public override T ToValueWithLengthPrefix(ref ReadOnlySpan<byte> span)
        {
            var spanLength = span.Length;
            if (spanLength == 0)
                goto fail;
            ref var location = ref MemoryMarshal.GetReference(span);
            var prefixLength = PrimitiveHelper.DecodePrefixLength(location);
            if (spanLength < prefixLength)
                goto fail;
            var length = PrimitiveHelper.DecodeLengthPrefix(ref location, prefixLength);
            if (length < Memory.SizeOf<T>())
                return ThrowHelper.ThrowNotEnoughBytes<T>();
            if ((uint)spanLength < (uint)(prefixLength + length))
                goto fail;
            var result = Memory.Get<T>(ref Memory.Add(ref location, prefixLength));
            span = span.Slice(prefixLength + length);
            return result;

        fail:
            return ThrowHelper.ThrowLengthPrefixInvalidBytes<T>();
        }
    }
}
