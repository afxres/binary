using Mikodev.Binary.Internal;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace Mikodev.Binary.Creators
{
    internal sealed class OriginalEndiannessConverter<T> : Converter<T> where T : unmanaged
    {
        public OriginalEndiannessConverter() : base(MemoryHelper.SizeOf<T>())
        {
            Debug.Assert(typeof(T) == typeof(Guid) || new[] { 1, 2, 4, 8 }.Contains(MemoryHelper.SizeOf<T>()));
        }

        public override void Encode(ref Allocator allocator, T item)
        {
            MemoryHelper.Set(ref Allocator.AllocateReference(ref allocator, MemoryHelper.SizeOf<T>()), item);
        }

        public override T Decode(in ReadOnlySpan<byte> span)
        {
            if (span.Length < MemoryHelper.SizeOf<T>())
                ThrowHelper.ThrowNotEnoughBytes();
            ref var source = ref MemoryMarshal.GetReference(span);
            return MemoryHelper.Get<T>(ref source);
        }

        public override void EncodeAuto(ref Allocator allocator, T item)
        {
            MemoryHelper.Set(ref Allocator.AllocateReference(ref allocator, MemoryHelper.SizeOf<T>()), item);
        }

        public override T DecodeAuto(ref ReadOnlySpan<byte> span)
        {
            // take reference first, then check bounds via slice method
            ref var location = ref MemoryMarshal.GetReference(span);
            span = span.Slice(MemoryHelper.SizeOf<T>());
            return MemoryHelper.Get<T>(ref location);
        }

        public override void EncodeWithLengthPrefix(ref Allocator allocator, T item)
        {
            ref var location = ref Allocator.AllocateReference(ref allocator, MemoryHelper.SizeOf<T>() + 1);
            location = (byte)MemoryHelper.SizeOf<T>();
            MemoryHelper.Set(ref MemoryHelper.Add(ref location, 1), item);
        }

        public override T DecodeWithLengthPrefix(ref ReadOnlySpan<byte> span)
        {
            var limits = span.Length;
            if (limits == 0)
                goto fail;
            ref var location = ref MemoryMarshal.GetReference(span);
            var prefixLength = PrimitiveHelper.DecodeNumberLength(location);
            if (limits < prefixLength)
                goto fail;
            var length = PrimitiveHelper.DecodeNumber(ref location, prefixLength);
            if (length < MemoryHelper.SizeOf<T>())
                goto fail;
            // check bounds via slice method
            span = span.Slice(prefixLength + length);
            return MemoryHelper.Get<T>(ref MemoryHelper.Add(ref location, prefixLength));

        fail:
            ThrowHelper.ThrowNotEnoughBytes();
            throw null;
        }

        public override byte[] Encode(T item)
        {
            var buffer = new byte[MemoryHelper.SizeOf<T>()];
            MemoryHelper.Set(ref buffer[0], item);
            return buffer;
        }

        public override T Decode(byte[] buffer)
        {
            if (buffer == null || buffer.Length < MemoryHelper.SizeOf<T>())
                ThrowHelper.ThrowNotEnoughBytes();
            return MemoryHelper.Get<T>(ref buffer[0]);
        }
    }
}
