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
            // take reference first
            ref var source = ref MemoryMarshal.GetReference(span);
            // check bounds via slice method
            span = span.Slice(Unsafe.SizeOf<T>());
            return Unsafe.ReadUnaligned<T>(ref source);
        }

        public override void EncodeWithLengthPrefix(ref Allocator allocator, T item)
        {
            ref var target = ref Allocator.Allocate(ref allocator, Unsafe.SizeOf<T>() + sizeof(byte));
            target = (byte)Unsafe.SizeOf<T>();
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref target, sizeof(byte)), item);
        }

        public override T DecodeWithLengthPrefix(ref ReadOnlySpan<byte> span)
        {
            int length;
            int numberLength;
            var limits = span.Length;
            ref var source = ref MemoryMarshal.GetReference(span);
            if (limits == 0 || limits < (numberLength = PrimitiveHelper.DecodeNumberLength(source)) || (length = PrimitiveHelper.DecodeNumber(ref source, numberLength)) < Unsafe.SizeOf<T>())
                return ThrowHelper.ThrowNotEnoughBytes<T>();
            // check bounds via slice method
            span = span.Slice(numberLength).Slice(length);
            return Unsafe.ReadUnaligned<T>(ref Unsafe.Add(ref source, numberLength));
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
