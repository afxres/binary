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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Append(ref Allocator allocator, T item)
        {
            Unsafe.WriteUnaligned(ref Allocator.Assign(ref allocator, Unsafe.SizeOf<T>()), item);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static T Detach(ReadOnlySpan<byte> span)
        {
            if (span.Length < Unsafe.SizeOf<T>())
                return ThrowHelper.ThrowNotEnoughBytes<T>();
            return Unsafe.ReadUnaligned<T>(ref MemoryMarshal.GetReference(span));
        }

        public override void Encode(ref Allocator allocator, T item)
        {
            Append(ref allocator, item);
        }

        public override T Decode(in ReadOnlySpan<byte> span)
        {
            return Detach(span);
        }

        public override void EncodeAuto(ref Allocator allocator, T item)
        {
            Append(ref allocator, item);
        }

        public override T DecodeAuto(ref ReadOnlySpan<byte> span)
        {
            // check bounds via slice method
            ref var source = ref MemoryMarshal.GetReference(span);
            span = span.Slice(Unsafe.SizeOf<T>());
            return Unsafe.ReadUnaligned<T>(ref source);
        }

        public override void EncodeWithLengthPrefix(ref Allocator allocator, T item)
        {
            ref var target = ref Allocator.Assign(ref allocator, Unsafe.SizeOf<T>() + sizeof(byte));
            Unsafe.WriteUnaligned(ref target, (byte)Unsafe.SizeOf<T>());
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref target, sizeof(byte)), item);
        }

        public override T DecodeWithLengthPrefix(ref ReadOnlySpan<byte> span)
        {
            return Detach(PrimitiveHelper.DecodeBufferWithLengthPrefix(ref span));
        }

        public override byte[] Encode(T item)
        {
            var buffer = new byte[Unsafe.SizeOf<T>()];
            Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(new Span<byte>(buffer)), item);
            return buffer;
        }

        public override T Decode(byte[] buffer)
        {
            if (buffer is null || buffer.Length < Unsafe.SizeOf<T>())
                return ThrowHelper.ThrowNotEnoughBytes<T>();
            return Unsafe.ReadUnaligned<T>(ref MemoryMarshal.GetReference(new Span<byte>(buffer)));
        }
    }
}
