namespace Mikodev.Binary.Converters.Endianness;

using Mikodev.Binary.Converters.Endianness.Adapters;
using Mikodev.Binary.Internal;
using Mikodev.Binary.Internal.Sequence;
using Mikodev.Binary.Internal.Sequence.Contexts;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

internal sealed class NativeEndianConverter<T> : Converter<T>, ISequenceAdapterCreator<T> where T : unmanaged
{
    public NativeEndianConverter() : base(Unsafe.SizeOf<T>())
    {
        Debug.Assert(BitConverter.IsLittleEndian);
        Debug.Assert(Unsafe.SizeOf<T>() is 1 or 2 or 4 or 8);
        Debug.Assert(NumberModule.EncodeLength((uint)Unsafe.SizeOf<T>()) is 1);
    }

    public override void Encode(ref Allocator allocator, T item)
    {
        Unsafe.WriteUnaligned(ref Allocator.Assign(ref allocator, Unsafe.SizeOf<T>()), item);
    }

    public override void EncodeAuto(ref Allocator allocator, T item)
    {
        Unsafe.WriteUnaligned(ref Allocator.Assign(ref allocator, Unsafe.SizeOf<T>()), item);
    }

    public override void EncodeWithLengthPrefix(ref Allocator allocator, T item)
    {
        ref var target = ref Allocator.Assign(ref allocator, Unsafe.SizeOf<T>() + 1);
        NumberModule.Encode(ref target, (uint)Unsafe.SizeOf<T>(), numberLength: 1);
        Unsafe.WriteUnaligned(ref Unsafe.Add(ref target, 1), item);
    }

    public override byte[] Encode(T item)
    {
        var result = new byte[Unsafe.SizeOf<T>()];
        Unsafe.WriteUnaligned(ref MemoryMarshal.GetArrayDataReference(result), item);
        return result;
    }

    public override T Decode(in ReadOnlySpan<byte> span)
    {
        return Unsafe.ReadUnaligned<T>(ref MemoryModule.EnsureLength(span, Unsafe.SizeOf<T>()));
    }

    public override T DecodeAuto(ref ReadOnlySpan<byte> span)
    {
        return Unsafe.ReadUnaligned<T>(ref MemoryModule.EnsureLength(ref span, Unsafe.SizeOf<T>()));
    }

    public override T DecodeWithLengthPrefix(ref ReadOnlySpan<byte> span)
    {
        return Unsafe.ReadUnaligned<T>(ref MemoryModule.EnsureLength(Converter.DecodeWithLengthPrefix(ref span), Unsafe.SizeOf<T>()));
    }

    public override T Decode(byte[]? buffer)
    {
        return Unsafe.ReadUnaligned<T>(ref MemoryModule.EnsureLength(new ReadOnlySpan<byte>(buffer), Unsafe.SizeOf<T>()));
    }

    SequenceAdapter<T> ISequenceAdapterCreator<T>.GetAdapter()
    {
        return new NativeEndianSequenceAdapter<T>();
    }
}
