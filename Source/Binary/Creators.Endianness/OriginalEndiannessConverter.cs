using Mikodev.Binary.Internal;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace Mikodev.Binary.Creators.Endianness
{
    internal sealed class OriginalEndiannessConverter<T> : Converter<T> where T : unmanaged
    {
        public OriginalEndiannessConverter() : base(Memory.SizeOf<T>())
        {
            Debug.Assert(typeof(T) == typeof(Guid) || new[] { 1, 2, 4, 8 }.Contains(Memory.SizeOf<T>()));
        }

        public override void Encode(ref Allocator allocator, T item)
        {
            Memory.Set(ref allocator.AllocateReference(Memory.SizeOf<T>()), item);
        }

        public override T Decode(in ReadOnlySpan<byte> span)
        {
            if (span.Length < Memory.SizeOf<T>())
                ThrowHelper.ThrowNotEnoughBytes();
            ref var source = ref MemoryMarshal.GetReference(span);
            return Memory.Get<T>(ref source);
        }

        public override void EncodeAuto(ref Allocator allocator, T item)
        {
            Memory.Set(ref allocator.AllocateReference(Memory.SizeOf<T>()), item);
        }

        public override T DecodeAuto(ref ReadOnlySpan<byte> span)
        {
            // take reference first, then check bounds via slice method
            ref var location = ref MemoryMarshal.GetReference(span);
            span = span.Slice(Memory.SizeOf<T>());
            return Memory.Get<T>(ref location);
        }

        public override void EncodeWithLengthPrefix(ref Allocator allocator, T item)
        {
            ref var location = ref allocator.AllocateReference(Memory.SizeOf<T>() + 1);
            location = (byte)Memory.SizeOf<T>();
            Memory.Set(ref Memory.Add(ref location, 1), item);
        }

        public override T DecodeWithLengthPrefix(ref ReadOnlySpan<byte> span)
        {
            var spanLength = span.Length;
            if (spanLength == 0)
                goto fail;
            ref var location = ref MemoryMarshal.GetReference(span);
            var prefixLength = PrimitiveHelper.DecodeNumberLength(location);
            if (spanLength < prefixLength)
                goto fail;
            var length = PrimitiveHelper.DecodeNumber(ref location, prefixLength);
            if (length < Memory.SizeOf<T>())
                goto fail;
            // check bounds via slice method
            span = span.Slice(prefixLength + length);
            return Memory.Get<T>(ref Memory.Add(ref location, prefixLength));

        fail:
            ThrowHelper.ThrowNotEnoughBytes();
            throw null;
        }

        public override byte[] Encode(T item)
        {
            var buffer = new byte[Memory.SizeOf<T>()];
            Memory.Set(ref buffer[0], item);
            return buffer;
        }

        public override T Decode(byte[] buffer)
        {
            if (buffer == null || buffer.Length < Memory.SizeOf<T>())
                ThrowHelper.ThrowNotEnoughBytes();
            return Memory.Get<T>(ref buffer[0]);
        }
    }
}
