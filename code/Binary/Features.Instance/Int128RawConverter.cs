namespace Mikodev.Binary.Features.Instance;

using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#if NET7_0_OR_GREATER
internal readonly struct Int128RawConverter : IRawConverter<Int128>
{
    public static int Length => Unsafe.SizeOf<Int128>();

    public static Int128 Decode(ref byte source)
    {
        var lower = BinaryPrimitives.ReadUInt64LittleEndian(MemoryMarshal.CreateReadOnlySpan(ref Unsafe.Add(ref source, 0), sizeof(ulong)));
        var upper = BinaryPrimitives.ReadUInt64LittleEndian(MemoryMarshal.CreateReadOnlySpan(ref Unsafe.Add(ref source, 8), sizeof(ulong)));
        return new Int128(upper, lower);
    }

    public static void Encode(ref byte target, Int128 item)
    {
        ref var source = ref Unsafe.As<Int128, ulong>(ref item);
        BinaryPrimitives.WriteUInt64LittleEndian(MemoryMarshal.CreateSpan(ref Unsafe.Add(ref target, 0), sizeof(ulong)), Unsafe.Add(ref source, 0));
        BinaryPrimitives.WriteUInt64LittleEndian(MemoryMarshal.CreateSpan(ref Unsafe.Add(ref target, 8), sizeof(ulong)), Unsafe.Add(ref source, 1));
    }
}
#endif
