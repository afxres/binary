namespace Mikodev.Binary.Features;

using Mikodev.Binary.Features.Adapters;
using Mikodev.Binary.Internal;
using Mikodev.Binary.Internal.Sequence;
using Mikodev.Binary.Internal.Sequence.Contexts;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

internal sealed class RawConverter<T, U> : Converter<T>, ISequenceAdapterCreator<T> where U : struct, IRawConverter<T>
{
    public RawConverter() : base(U.Length)
    {
        Debug.Assert(U.Length >= 1);
        Debug.Assert(U.Length <= 16);
        Debug.Assert(NumberModule.EncodeLength((uint)U.Length) is 1);
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

    public override byte[] Encode(T? item)
    {
        var result = new byte[U.Length];
        U.Encode(ref MemoryMarshal.GetArrayDataReference(result), item);
        return result;
    }

    public override void Encode(ref Allocator allocator, T? item)
    {
        U.Encode(ref Allocator.Assign(ref allocator, U.Length), item);
    }

    public override void EncodeAuto(ref Allocator allocator, T? item)
    {
        U.Encode(ref Allocator.Assign(ref allocator, U.Length), item);
    }

    public override void EncodeWithLengthPrefix(ref Allocator allocator, T? item)
    {
        var prefix = NumberModule.EncodeLength((uint)U.Length);
        ref var target = ref Allocator.Assign(ref allocator, U.Length + prefix);
        NumberModule.Encode(ref target, (uint)U.Length, prefix);
        U.Encode(ref Unsafe.Add(ref target, prefix), item);
    }

    SequenceAdapter<T> ISequenceAdapterCreator<T>.GetAdapter()
    {
        if (default(U) is ISequenceAdapterCreator<T> result)
            return result.GetAdapter();
        return new RawConverterSequenceAdapter<T, U>();
    }
}
