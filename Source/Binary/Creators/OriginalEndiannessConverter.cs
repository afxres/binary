using Mikodev.Binary.Internal;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Mikodev.Binary.Creators
{
    internal sealed class OriginalEndiannessConverter<T> : Converter<T> where T : unmanaged
    {
        public OriginalEndiannessConverter() : base(Unsafe.SizeOf<T>())
        {
            Debug.Assert(typeof(T) == typeof(Guid) || Unsafe.SizeOf<T>() == 1 || Unsafe.SizeOf<T>() == 2 || Unsafe.SizeOf<T>() == 4 || Unsafe.SizeOf<T>() == 8);
        }

        public override void Encode(ref Allocator allocator, T item)
        {
            Unsafe.WriteUnaligned(ref Allocator.Assign(ref allocator, Unsafe.SizeOf<T>()), item);
        }

        public override T Decode(in ReadOnlySpan<byte> span)
        {
            if (span.Length < Unsafe.SizeOf<T>())
                return ThrowHelper.ThrowNotEnoughBytes<T>();
            ref var source = ref MemoryMarshal.GetReference(span);
            return Unsafe.ReadUnaligned<T>(ref source);
        }

        public override void EncodeAuto(ref Allocator allocator, T item)
        {
            Unsafe.WriteUnaligned(ref Allocator.Assign(ref allocator, Unsafe.SizeOf<T>()), item);
        }

        public override T DecodeAuto(ref ReadOnlySpan<byte> span)
        {
            // take reference first
            ref var source = ref MemoryMarshal.GetReference(span);
            // check bounds via slice method
            span = span.Slice(Unsafe.SizeOf<T>());
            return Unsafe.ReadUnaligned<T>(ref source);
        }

        public override void EncodeWithLengthPrefix(ref Allocator allocator, T item)
        {
            ref var target = ref Allocator.Assign(ref allocator, Unsafe.SizeOf<T>() + sizeof(byte));
            target = (byte)Unsafe.SizeOf<T>();
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref target, sizeof(byte)), item);
        }

        public override T DecodeWithLengthPrefix(ref ReadOnlySpan<byte> span)
        {
            var data = PrimitiveHelper.DecodeBufferWithLengthPrefix(ref span);
            if (data.Length < Unsafe.SizeOf<T>())
                return ThrowHelper.ThrowNotEnoughBytes<T>();
            return Unsafe.ReadUnaligned<T>(ref MemoryMarshal.GetReference(data));
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
                return ThrowHelper.ThrowNotEnoughBytes<T>();
            return Unsafe.ReadUnaligned<T>(ref buffer[0]);
        }
    }
}
