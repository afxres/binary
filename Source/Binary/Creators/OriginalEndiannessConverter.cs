using Mikodev.Binary.Internal;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Mikodev.Binary.Creators
{
    internal sealed class OriginalEndiannessConverter<T> : Converter<T> where T : unmanaged
    {
        public OriginalEndiannessConverter() : base(Unsafe.SizeOf<T>())
        {
            Debug.Assert(typeof(T) == typeof(Guid) || new[] { 1, 2, 4, 8 }.Contains(Unsafe.SizeOf<T>()));
        }

        public override void Encode(ref Allocator allocator, T item)
        {
            Unsafe.WriteUnaligned(ref Allocator.Allocate(ref allocator, Unsafe.SizeOf<T>()), item);
        }

        public override T Decode(in ReadOnlySpan<byte> span)
        {
            if (span.Length < Unsafe.SizeOf<T>())
                ThrowHelper.ThrowNotEnoughBytes();
            ref var source = ref MemoryMarshal.GetReference(span);
            return Unsafe.ReadUnaligned<T>(ref source);
        }

        public override void EncodeAuto(ref Allocator allocator, T item)
        {
            Unsafe.WriteUnaligned(ref Allocator.Allocate(ref allocator, Unsafe.SizeOf<T>()), item);
        }

        public override T DecodeAuto(ref ReadOnlySpan<byte> span)
        {
            // take reference first, then check bounds via slice method
            ref var location = ref MemoryMarshal.GetReference(span);
            span = span.Slice(Unsafe.SizeOf<T>());
            return Unsafe.ReadUnaligned<T>(ref location);
        }

        public override void EncodeWithLengthPrefix(ref Allocator allocator, T item)
        {
            ref var location = ref Allocator.Allocate(ref allocator, Unsafe.SizeOf<T>() + 1);
            location = (byte)Unsafe.SizeOf<T>();
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref location, 1), item);
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
            if (length < Unsafe.SizeOf<T>())
                goto fail;
            // check bounds via slice method
            span = span.Slice(prefixLength + length);
            return Unsafe.ReadUnaligned<T>(ref Unsafe.Add(ref location, prefixLength));

        fail:
            return ThrowHelper.ThrowNotEnoughBytes<T>();
        }

        public override byte[] Encode(T item)
        {
            var buffer = new byte[Unsafe.SizeOf<T>()];
            Unsafe.WriteUnaligned(ref buffer[0], item);
            return buffer;
        }

        public override T Decode(byte[] buffer)
        {
            if (buffer == null || buffer.Length < Unsafe.SizeOf<T>())
                ThrowHelper.ThrowNotEnoughBytes();
            return Unsafe.ReadUnaligned<T>(ref buffer[0]);
        }
    }
}
