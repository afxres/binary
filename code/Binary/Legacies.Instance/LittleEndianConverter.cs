namespace Mikodev.Binary.Legacies.Instance;

using Mikodev.Binary.Features.Fallback;
using Mikodev.Binary.Internal;
using System;
using System.Runtime.CompilerServices;

internal sealed class LittleEndianConverter<T> : Converter<T> where T : unmanaged
{
    public LittleEndianConverter() : base(Unsafe.SizeOf<T>()) { }

    public override void Encode(ref Allocator allocator, T item) => LittleEndianFallback.Encode(ref Allocator.Assign(ref allocator, Unsafe.SizeOf<T>()), item);

    public override T Decode(in ReadOnlySpan<byte> span) => LittleEndianFallback.Decode<T>(ref MemoryModule.EnsureLength(span, Unsafe.SizeOf<T>()));
}
