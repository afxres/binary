namespace Mikodev.Binary.Features.Fallback;

using System;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UInt16 = System.UInt16;
using UInt32 = System.UInt32;
using UInt64 = System.UInt64;

internal static class LittleEndianFallback
{
#if NET7_0_OR_GREATER
    private struct Raw128Data
    {
        public UInt64 Lower;

        public UInt64 Upper;
    }

    private static T Decode128<T>(ref byte source) where T : unmanaged
    {
        Debug.Assert(typeof(T) == typeof(Int128) || typeof(T) == typeof(UInt128));
        var result = default(T);
        Unsafe.As<T, Raw128Data>(ref result).Lower = BinaryPrimitives.ReadUInt64LittleEndian(MemoryMarshal.CreateReadOnlySpan(ref Unsafe.Add(ref source, 0), sizeof(UInt64)));
        Unsafe.As<T, Raw128Data>(ref result).Upper = BinaryPrimitives.ReadUInt64LittleEndian(MemoryMarshal.CreateReadOnlySpan(ref Unsafe.Add(ref source, 8), sizeof(UInt64)));
        return result;
    }

    private static void Encode128<T>(ref byte target, T item) where T : unmanaged
    {
        Debug.Assert(typeof(T) == typeof(Int128) || typeof(T) == typeof(UInt128));
        BinaryPrimitives.WriteUInt64LittleEndian(MemoryMarshal.CreateSpan(ref Unsafe.Add(ref target, 0), sizeof(UInt64)), Unsafe.As<T, Raw128Data>(ref item).Lower);
        BinaryPrimitives.WriteUInt64LittleEndian(MemoryMarshal.CreateSpan(ref Unsafe.Add(ref target, 8), sizeof(UInt64)), Unsafe.As<T, Raw128Data>(ref item).Upper);
    }
#endif

    internal static T Decode<T>(ref byte source) where T : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static T MakeCast<E>(E data) => Unsafe.As<E, T>(ref data);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static ReadOnlySpan<byte> MakeSpan(ref byte location) => MemoryMarshal.CreateReadOnlySpan(ref location, Unsafe.SizeOf<T>());

        switch (Unsafe.SizeOf<T>())
        {
            case 0x01: return Unsafe.ReadUnaligned<T>(ref source);
            case 0x02: return MakeCast(BinaryPrimitives.ReadUInt16LittleEndian(MakeSpan(ref source)));
            case 0x04: return MakeCast(BinaryPrimitives.ReadUInt32LittleEndian(MakeSpan(ref source)));
            case 0x08: return MakeCast(BinaryPrimitives.ReadUInt64LittleEndian(MakeSpan(ref source)));
#if NET7_0_OR_GREATER
            case 0x10: return Decode128<T>(ref source);
#endif
            default: throw new NotSupportedException();
        }
    }

    internal static void Encode<T>(ref byte target, T item) where T : unmanaged
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static E MakeCast<E>(T data) => Unsafe.As<T, E>(ref data);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static Span<byte> MakeSpan(ref byte location) => MemoryMarshal.CreateSpan(ref location, Unsafe.SizeOf<T>());

        switch (Unsafe.SizeOf<T>())
        {
            case 0x01: Unsafe.WriteUnaligned(ref target, item); break;
            case 0x02: BinaryPrimitives.WriteUInt16LittleEndian(MakeSpan(ref target), MakeCast<UInt16>(item)); break;
            case 0x04: BinaryPrimitives.WriteUInt32LittleEndian(MakeSpan(ref target), MakeCast<UInt32>(item)); break;
            case 0x08: BinaryPrimitives.WriteUInt64LittleEndian(MakeSpan(ref target), MakeCast<UInt64>(item)); break;
#if NET7_0_OR_GREATER
            case 0x10: Encode128(ref target, item); break;
#endif
            default: throw new NotSupportedException();
        }
    }
}
