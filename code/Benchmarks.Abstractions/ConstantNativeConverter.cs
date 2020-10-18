using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Mikodev.Binary.Benchmarks.Abstractions
{
    public class ConstantNativeConverter<T> : Converter<T> where T : unmanaged
    {
        public ConstantNativeConverter() : base(Unsafe.SizeOf<T>()) { }

        public override void Encode(ref Allocator allocator, T item)
        {
            ref var source = ref Unsafe.As<T, byte>(ref item);
            var span = MemoryMarshal.CreateReadOnlySpan(ref source, Unsafe.SizeOf<T>());
            AllocatorHelper.Append(ref allocator, span);
        }

        public override T Decode(in ReadOnlySpan<byte> span)
        {
            if (span.Length < Unsafe.SizeOf<T>())
                throw new ArgumentException("Not enough bytes.");
            return Unsafe.ReadUnaligned<T>(ref MemoryMarshal.GetReference(span));
        }
    }
}
