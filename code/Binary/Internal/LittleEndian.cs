namespace Mikodev.Binary.Internal;

using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

internal static class LittleEndian
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static T Decode<T>(ref byte source) where T : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static T MakeCast<E>(E data) where E : unmanaged => Unsafe.BitCast<E, T>(data);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static ReadOnlySpan<byte> MakeSpan(ref byte location) => MemoryMarshal.CreateReadOnlySpan(ref location, Unsafe.SizeOf<T>());

        switch (Unsafe.SizeOf<T>())
        {
            case 0x01: return Unsafe.ReadUnaligned<T>(ref source);
            case 0x02: return MakeCast(BinaryPrimitives.ReadUInt16LittleEndian(MakeSpan(ref source)));
            case 0x04: return MakeCast(BinaryPrimitives.ReadUInt32LittleEndian(MakeSpan(ref source)));
            case 0x08: return MakeCast(BinaryPrimitives.ReadUInt64LittleEndian(MakeSpan(ref source)));
            case 0x10: return MakeCast(BinaryPrimitives.ReadUInt128LittleEndian(MakeSpan(ref source)));
            default: throw new NotSupportedException();
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Encode<T>(ref byte target, T item) where T : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static E MakeCast<E>(T data) where E : unmanaged => Unsafe.BitCast<T, E>(data);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static Span<byte> MakeSpan(ref byte location) => MemoryMarshal.CreateSpan(ref location, Unsafe.SizeOf<T>());

        switch (Unsafe.SizeOf<T>())
        {
            case 0x01: Unsafe.WriteUnaligned(ref target, item); break;
            case 0x02: BinaryPrimitives.WriteUInt16LittleEndian(MakeSpan(ref target), MakeCast<ushort>(item)); break;
            case 0x04: BinaryPrimitives.WriteUInt32LittleEndian(MakeSpan(ref target), MakeCast<uint>(item)); break;
            case 0x08: BinaryPrimitives.WriteUInt64LittleEndian(MakeSpan(ref target), MakeCast<ulong>(item)); break;
            case 0x10: BinaryPrimitives.WriteUInt128LittleEndian(MakeSpan(ref target), MakeCast<UInt128>(item)); break;
            default: throw new NotSupportedException();
        }
    }
}
