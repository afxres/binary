namespace Mikodev.Binary.Features;

using Mikodev.Binary.Converters.Endianness.Adapters;
using Mikodev.Binary.Features.Adapters;
using Mikodev.Binary.Features.Contexts;
using Mikodev.Binary.Features.Instance;
using Mikodev.Binary.Internal;
using Mikodev.Binary.Internal.SpanLike;
using Mikodev.Binary.Internal.SpanLike.Contexts;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

#if NET6_0_OR_GREATER
[RequiresPreviewFeatures]
internal sealed class RawConverter<T, U> : Converter<T>, ISpanLikeAdapterCreator<T> where T : unmanaged where U : struct, IRawConverter<T>
{
    public RawConverter() : base(U.Length)
    {
        if (U.Length > 0)
            return;
        ThrowHelper.ThrowLengthNegative();
    }

    public override T Decode(byte[]? buffer)
    {
        return U.Decode(ref MemoryModule.EnsureLength(new ReadOnlySpan<byte>(buffer), U.Length));
    }

    public override T Decode(in ReadOnlySpan<byte> span)
    {
        return U.Decode(ref MemoryModule.EnsureLength(span, U.Length));
    }

    public override T DecodeAuto(ref ReadOnlySpan<byte> span)
    {
        return U.Decode(ref MemoryModule.EnsureLength(ref span, U.Length));
    }

    public override T DecodeWithLengthPrefix(ref ReadOnlySpan<byte> span)
    {
        return U.Decode(ref MemoryModule.EnsureLength(Converter.DecodeWithLengthPrefix(ref span), U.Length));
    }

    public override byte[] Encode(T item)
    {
        var result = new byte[U.Length];
        U.Encode(ref MemoryMarshal.GetReference(new Span<byte>(result)), item);
        return result;
    }

    public override void Encode(ref Allocator allocator, T item)
    {
        U.Encode(ref Allocator.Assign(ref allocator, U.Length), item);
    }

    public override void EncodeAuto(ref Allocator allocator, T item)
    {
        U.Encode(ref Allocator.Assign(ref allocator, U.Length), item);
    }

    public override void EncodeWithLengthPrefix(ref Allocator allocator, T item)
    {
        var prefix = NumberModule.EncodeLength((uint)U.Length);
        ref var target = ref Allocator.Assign(ref allocator, U.Length + prefix);
        NumberModule.Encode(ref target, (uint)U.Length, prefix);
        U.Encode(ref Unsafe.Add(ref target, prefix), item);
    }

    public SpanLikeAdapter<T> GetAdapter()
    {
        if (typeof(U) == typeof(NativeEndianRawConverter<T>))
            return new NativeEndianSpanLikeAdapter<T>();
        return new RawConverterSpanLikeAdapter<T, U>();
    }
}
#endif
