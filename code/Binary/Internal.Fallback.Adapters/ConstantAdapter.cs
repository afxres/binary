using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Mikodev.Binary.Internal.Fallback.Adapters
{
    internal sealed class ConstantAdapter<T, U> : FallbackAdapter<T> where U : unmanaged
    {
        private readonly Converter<T> converter;

        public ConstantAdapter(Converter<T> converter) => this.converter = converter;

        public override byte[] Encode(T item)
        {
            var converter = this.converter;
            var length = converter.Length;
            var buffer = new byte[length];
            var allocator = new Allocator(new Span<byte>(buffer), maxCapacity: length);
            converter.Encode(ref allocator, item);
            return buffer;
        }

        public override void EncodeWithLengthPrefix(ref Allocator allocator, T item)
        {
            var converter = this.converter;
            Debug.Assert(MemoryHelper.EncodeNumberLength((uint)converter.Length) == Unsafe.SizeOf<U>());
            MemoryHelper.EncodeNumber(ref Allocator.Assign(ref allocator, Unsafe.SizeOf<U>()), (uint)converter.Length, numberLength: Unsafe.SizeOf<U>());
            converter.Encode(ref allocator, item);
        }
    }
}
