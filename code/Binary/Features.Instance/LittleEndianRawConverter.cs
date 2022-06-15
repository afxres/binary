namespace Mikodev.Binary.Features.Instance;

using Mikodev.Binary.Features;
using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#if NET7_0_OR_GREATER
internal readonly struct LittleEndianRawConverter<T> : IRawConverter<T> where T : unmanaged
{
    public static int Length => Unsafe.SizeOf<T>();

    public static T Decode(ref byte source)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static T MakeCast<E>(E data) => Unsafe.As<E, T>(ref data);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static ReadOnlySpan<byte> MakeSpan(ref byte location) => MemoryMarshal.CreateReadOnlySpan(ref location, Unsafe.SizeOf<T>());

        switch (Unsafe.SizeOf<T>())
        {
            case 1: return MakeCast(Unsafe.ReadUnaligned<byte>(ref source));
            case 2: return MakeCast(BinaryPrimitives.ReadInt16LittleEndian(MakeSpan(ref source)));
            case 4: return MakeCast(BinaryPrimitives.ReadInt32LittleEndian(MakeSpan(ref source)));
            case 8: return MakeCast(BinaryPrimitives.ReadInt64LittleEndian(MakeSpan(ref source)));
            default: throw new NotSupportedException();
        }
    }

    public static void Encode(ref byte target, T item)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static E MakeCast<E>(T data) => Unsafe.As<T, E>(ref data);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static Span<byte> MakeSpan(ref byte location) => MemoryMarshal.CreateSpan(ref location, Unsafe.SizeOf<T>());

        switch (Unsafe.SizeOf<T>())
        {
            case 1: Unsafe.WriteUnaligned(ref target, MakeCast<byte>(item)); break;
            case 2: BinaryPrimitives.WriteInt16LittleEndian(MakeSpan(ref target), MakeCast<Int16>(item)); break;
            case 4: BinaryPrimitives.WriteInt32LittleEndian(MakeSpan(ref target), MakeCast<Int32>(item)); break;
            case 8: BinaryPrimitives.WriteInt64LittleEndian(MakeSpan(ref target), MakeCast<Int64>(item)); break;
            default: throw new NotSupportedException();
        }
    }
}
#endif
