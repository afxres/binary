namespace Mikodev.Binary.Internal;

using System;
using System.Runtime.CompilerServices;

internal static class LittleEndian
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void Encode<T>(ref byte location, T item) where T : unmanaged
    {
        Unsafe.WriteUnaligned(ref location, MemoryModule.EnsureEndian(item, BitConverter.IsLittleEndian is false));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static T Decode<T>(ref byte location) where T : unmanaged
    {
        return MemoryModule.EnsureEndian(Unsafe.ReadUnaligned<T>(ref location), BitConverter.IsLittleEndian is false);
    }
}
