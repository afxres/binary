using Mikodev.Binary.Internal;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Mikodev.Binary.Converters.Endianness
{
    internal sealed class NativeEndianConverter<T> : Converter<T> where T : unmanaged
    {
        public NativeEndianConverter() : base(Unsafe.SizeOf<T>())
        {
            Debug.Assert(BitConverter.IsLittleEndian);
            Debug.Assert(Unsafe.SizeOf<T>() is 1 or 2 or 4 or 8 or 16);
            Debug.Assert(MemoryHelper.EncodeNumberLength((uint)Unsafe.SizeOf<T>()) is 1);
        }

        public override void Encode(ref Allocator allocator, T item)
        {
            MemoryHelper.EncodeNativeEndian(ref allocator, item);
        }

        public override void EncodeAuto(ref Allocator allocator, T item)
        {
            MemoryHelper.EncodeNativeEndian(ref allocator, item);
        }

        public override void EncodeWithLengthPrefix(ref Allocator allocator, T item)
        {
            ref var target = ref Allocator.Assign(ref allocator, Unsafe.SizeOf<T>() + 1);
            MemoryHelper.EncodeNumber(ref target, (uint)Unsafe.SizeOf<T>(), numberLength: 1);
            MemoryHelper.EncodeNativeEndian(ref Unsafe.Add(ref target, 1), item);
        }

        public override byte[] Encode(T item)
        {
            var buffer = new byte[Unsafe.SizeOf<T>()];
            MemoryHelper.EncodeNativeEndian(ref MemoryMarshal.GetReference(new Span<byte>(buffer)), item);
            return buffer;
        }

        public override T Decode(in ReadOnlySpan<byte> span)
        {
            return MemoryHelper.DecodeNativeEndian<T>(span);
        }

        public override T DecodeAuto(ref ReadOnlySpan<byte> span)
        {
            return MemoryHelper.DecodeNativeEndian<T>(ref MemoryHelper.EnsureLength(ref span, Unsafe.SizeOf<T>()));
        }

        public override T DecodeWithLengthPrefix(ref ReadOnlySpan<byte> span)
        {
            return MemoryHelper.DecodeNativeEndian<T>(Converter.DecodeWithLengthPrefix(ref span));
        }

        public override T Decode(byte[] buffer)
        {
            return MemoryHelper.DecodeNativeEndian<T>(new ReadOnlySpan<byte>(buffer));
        }
    }
}
