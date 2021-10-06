namespace Mikodev.Binary.Internal;

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

internal static class NumberModule
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int EncodeLength(uint number)
    {
        Debug.Assert(number <= int.MaxValue);
        if ((number >> 7) is 0)
            return 1;
        else
            return 4;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void Encode(ref byte location, uint number, int numberLength)
    {
        Debug.Assert(number <= int.MaxValue);
        Debug.Assert(numberLength is 1 or 4);
        if (numberLength is 1)
            Unsafe.WriteUnaligned(ref location, (byte)number);
        else
            Unsafe.WriteUnaligned(ref location, MemoryModule.EnsureEndian((int)(number | 0x8000_0000U), BitConverter.IsLittleEndian));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int Decode(ref byte location, ref int offset, int limits)
    {
        Debug.Assert(offset >= 0);
        Debug.Assert(limits >= 0);
        Debug.Assert(limits >= offset);
        if (limits == offset)
            ThrowHelper.ThrowNotEnoughBytes();
        ref var source = ref Unsafe.Add(ref location, offset);
        var header = (uint)Unsafe.ReadUnaligned<byte>(ref source);
        offset += 1;
        if ((header & 0x80U) is 0)
            return (int)header;
        if ((uint)(limits - offset) < 3U)
            ThrowHelper.ThrowNotEnoughBytes();
        var result = (uint)MemoryModule.EnsureEndian(Unsafe.ReadUnaligned<int>(ref source), BitConverter.IsLittleEndian);
        offset += 3;
        return (int)(result & 0x7FFF_FFFFU);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int DecodeEnsureBuffer(ref byte location, ref int offset, int limits)
    {
        Debug.Assert(offset >= 0);
        Debug.Assert(limits >= 0);
        Debug.Assert(limits >= offset);
        var length = Decode(ref location, ref offset, limits);
        if ((uint)(limits - offset) < (uint)length)
            ThrowHelper.ThrowNotEnoughBytes();
        return length;
    }
}
