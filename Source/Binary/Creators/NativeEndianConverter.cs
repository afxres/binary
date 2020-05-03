using Mikodev.Binary.Internal;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Mikodev.Binary.Creators
{
    internal sealed class NativeEndianConverter<T> : Converter<T> where T : unmanaged
    {
        public NativeEndianConverter() : base(Unsafe.SizeOf<T>())
        {
            Debug.Assert(typeof(T) == typeof(Guid) || Unsafe.SizeOf<T>() == 1 || Unsafe.SizeOf<T>() == 2 || Unsafe.SizeOf<T>() == 4 || Unsafe.SizeOf<T>() == 8);
        }

        public override void Encode(ref Allocator allocator, T item)
        {
            MemoryHelper.EncodeNativeEndian(ref allocator, item);
        }

        public override T Decode(in ReadOnlySpan<byte> span)
        {
            return MemoryHelper.DecodeNativeEndian<T>(span);
        }

        public override void EncodeAuto(ref Allocator allocator, T item)
        {
            MemoryHelper.EncodeNativeEndian(ref allocator, item);
        }

        public override T DecodeAuto(ref ReadOnlySpan<byte> span)
        {
            // check bounds via slice method
            ref var source = ref MemoryMarshal.GetReference(span);
            span = span.Slice(Unsafe.SizeOf<T>());
            return MemoryHelper.DecodeNativeEndian<T>(ref source);
        }

        public override void EncodeWithLengthPrefix(ref Allocator allocator, T item)
        {
            ref var target = ref Allocator.Assign(ref allocator, Unsafe.SizeOf<T>() + sizeof(byte));
            MemoryHelper.EncodeNumber(ref target, (uint)Unsafe.SizeOf<T>(), numberLength: 1);
            MemoryHelper.EncodeNativeEndian(ref Unsafe.Add(ref target, sizeof(byte)), item);
        }

        public override T DecodeWithLengthPrefix(ref ReadOnlySpan<byte> span)
        {
            return MemoryHelper.DecodeNativeEndian<T>(PrimitiveHelper.DecodeBufferWithLengthPrefix(ref span));
        }

        public override byte[] Encode(T item)
        {
            var buffer = new byte[Unsafe.SizeOf<T>()];
            MemoryHelper.EncodeNativeEndian(ref MemoryMarshal.GetReference(new Span<byte>(buffer)), item);
            return buffer;
        }

        public override T Decode(byte[] buffer)
        {
            if (buffer is null || buffer.Length < Unsafe.SizeOf<T>())
                return ThrowHelper.ThrowNotEnoughBytes<T>();
            return MemoryHelper.DecodeNativeEndian<T>(ref MemoryMarshal.GetReference(new Span<byte>(buffer)));
        }
    }
}
