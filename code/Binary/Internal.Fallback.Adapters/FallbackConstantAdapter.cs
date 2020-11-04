using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Mikodev.Binary.Internal.Fallback.Adapters
{
    internal sealed class FallbackConstantAdapter<T, U> : FallbackAdapter<T> where U : unmanaged
    {
        private readonly Converter<T> converter;

        public FallbackConstantAdapter(Converter<T> converter) => this.converter = converter;

        public override void EncodeAuto(ref Allocator allocator, T item)
        {
            this.converter.Encode(ref allocator, item);
        }

        public override void EncodeWithLengthPrefix(ref Allocator allocator, T item)
        {
            var converter = this.converter;
            Debug.Assert(MemoryHelper.EncodeNumberLength((uint)converter.Length) == Unsafe.SizeOf<U>());
            MemoryHelper.EncodeNumber(ref Allocator.Assign(ref allocator, Unsafe.SizeOf<U>()), (uint)converter.Length, numberLength: Unsafe.SizeOf<U>());
            converter.Encode(ref allocator, item);
        }

        public override byte[] Encode(T item)
        {
            var converter = this.converter;
            var length = converter.Length;
            var buffer = new byte[length];
            var allocator = new Allocator(new Span<byte>(buffer), maxCapacity: length);
            converter.Encode(ref allocator, item);
            return buffer;
        }

        public override T DecodeAuto(ref ReadOnlySpan<byte> span)
        {
            var converter = this.converter;
            var length = converter.Length;
            var buffer = MemoryHelper.EnsureLengthReturnBuffer(ref span, length);
            var result = converter.Decode(in buffer);
            return result;
        }
    }
}
