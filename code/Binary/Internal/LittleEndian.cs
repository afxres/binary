namespace Mikodev.Binary.Internal;

using System;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BIN2 = System.UInt16;
using BIN4 = System.UInt32;
using BIN8 = System.UInt64;

internal static class LittleEndian
{
    private struct Raw128Data
    {
        public ulong Lower;

        public ulong Upper;
    }

    private static T Decode128<T>(ref byte source) where T : unmanaged
    {
        Debug.Assert(Unsafe.SizeOf<T>() is 16);
        var result = default(T);
        Unsafe.As<T, Raw128Data>(ref result).Lower = BinaryPrimitives.ReadUInt64LittleEndian(MemoryMarshal.CreateReadOnlySpan(ref Unsafe.Add(ref source, 0), sizeof(ulong)));
        Unsafe.As<T, Raw128Data>(ref result).Upper = BinaryPrimitives.ReadUInt64LittleEndian(MemoryMarshal.CreateReadOnlySpan(ref Unsafe.Add(ref source, 8), sizeof(ulong)));
        return result;
    }

    private static void Encode128<T>(ref byte target, T item) where T : unmanaged
    {
        Debug.Assert(Unsafe.SizeOf<T>() is 16);
        BinaryPrimitives.WriteUInt64LittleEndian(MemoryMarshal.CreateSpan(ref Unsafe.Add(ref target, 0), sizeof(ulong)), Unsafe.As<T, Raw128Data>(ref item).Lower);
        BinaryPrimitives.WriteUInt64LittleEndian(MemoryMarshal.CreateSpan(ref Unsafe.Add(ref target, 8), sizeof(ulong)), Unsafe.As<T, Raw128Data>(ref item).Upper);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static T Decode<T>(ref byte source) where T : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static T MakeCast<E>(E data) => Unsafe.ReadUnaligned<T>(ref Unsafe.As<E, byte>(ref data));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static ReadOnlySpan<byte> MakeSpan(ref byte location) => MemoryMarshal.CreateReadOnlySpan(ref location, Unsafe.SizeOf<T>());

        switch (Unsafe.SizeOf<T>())
        {
            case 0x01: return Unsafe.ReadUnaligned<T>(ref source);
            case 0x02: return MakeCast(BinaryPrimitives.ReadUInt16LittleEndian(MakeSpan(ref source)));
            case 0x04: return MakeCast(BinaryPrimitives.ReadUInt32LittleEndian(MakeSpan(ref source)));
            case 0x08: return MakeCast(BinaryPrimitives.ReadUInt64LittleEndian(MakeSpan(ref source)));
            case 0x10: return Decode128<T>(ref source);
            default: throw new NotSupportedException();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void Encode<T>(ref byte target, T item) where T : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static E MakeCast<E>(T data) => Unsafe.ReadUnaligned<E>(ref Unsafe.As<T, byte>(ref data));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static Span<byte> MakeSpan(ref byte location) => MemoryMarshal.CreateSpan(ref location, Unsafe.SizeOf<T>());

        switch (Unsafe.SizeOf<T>())
        {
            case 0x01: Unsafe.WriteUnaligned(ref target, item); break;
            case 0x02: BinaryPrimitives.WriteUInt16LittleEndian(MakeSpan(ref target), MakeCast<BIN2>(item)); break;
            case 0x04: BinaryPrimitives.WriteUInt32LittleEndian(MakeSpan(ref target), MakeCast<BIN4>(item)); break;
            case 0x08: BinaryPrimitives.WriteUInt64LittleEndian(MakeSpan(ref target), MakeCast<BIN8>(item)); break;
            case 0x10: Encode128(ref target, item); break;
            default: throw new NotSupportedException();
        }
    }
}
