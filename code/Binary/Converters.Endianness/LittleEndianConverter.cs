namespace Mikodev.Binary.Converters.Endianness;

using Mikodev.Binary.Internal;
using System;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

internal sealed class LittleEndianConverter<T> : Converter<T> where T : unmanaged
{
    public LittleEndianConverter() : base(Unsafe.SizeOf<T>())
    {
        Debug.Assert(Unsafe.SizeOf<T>() <= 8);
        Debug.Assert(Unsafe.SizeOf<T>() is 1 or 2 or 4 or 8);
        Debug.Assert(NumberHelper.EncodeLength((uint)Unsafe.SizeOf<T>()) is 1);
    }

    public override void Encode(ref Allocator allocator, T item)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static E MakeCast<E>(T data) => Unsafe.As<T, E>(ref data);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static Span<byte> MakeSpan(ref byte location) => MemoryMarshal.CreateSpan(ref location, Unsafe.SizeOf<T>());

        ref var target = ref Allocator.Assign(ref allocator, Unsafe.SizeOf<T>());
        switch (Unsafe.SizeOf<T>())
        {
            case 1: Unsafe.WriteUnaligned(ref target, MakeCast<byte>(item)); break;
            case 2: BinaryPrimitives.WriteInt16LittleEndian(MakeSpan(ref target), MakeCast<Int16>(item)); break;
            case 4: BinaryPrimitives.WriteInt32LittleEndian(MakeSpan(ref target), MakeCast<Int32>(item)); break;
            case 8: BinaryPrimitives.WriteInt64LittleEndian(MakeSpan(ref target), MakeCast<Int64>(item)); break;
            default: throw new NotSupportedException();
        }
    }

    public override T Decode(in ReadOnlySpan<byte> span)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static T MakeCast<E>(E data) => Unsafe.As<E, T>(ref data);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static ReadOnlySpan<byte> MakeSpan(ref byte location) => MemoryMarshal.CreateReadOnlySpan(ref location, Unsafe.SizeOf<T>());

        ref var source = ref MemoryHelper.EnsureLength(span, Unsafe.SizeOf<T>());
        switch (Unsafe.SizeOf<T>())
        {
            case 1: return MakeCast(Unsafe.ReadUnaligned<byte>(ref source));
            case 2: return MakeCast(BinaryPrimitives.ReadInt16LittleEndian(MakeSpan(ref source)));
            case 4: return MakeCast(BinaryPrimitives.ReadInt32LittleEndian(MakeSpan(ref source)));
            case 8: return MakeCast(BinaryPrimitives.ReadInt64LittleEndian(MakeSpan(ref source)));
            default: throw new NotSupportedException();
        }
    }
}
