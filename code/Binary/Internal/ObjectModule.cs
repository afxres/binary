namespace Mikodev.Binary.Internal;

using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

internal static class ObjectModule
{
    internal static T? GetNullValueOrNotEnoughBytes<T>()
    {
        if (default(T) is not null)
            ThrowHelper.ThrowNotEnoughBytes();
        return default;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool NotDefaultValue<T>(T? item)
    {
        return EqualityComparer<T>.Default.Equals(item, default) is false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void Append(ref Allocator allocator, byte[] data)
    {
        Debug.Assert(data is not null);
        Debug.Assert(data.Length is not 0);
        Unsafe.CopyBlockUnaligned(ref Allocator.Assign(ref allocator, data.Length), ref MemoryMarshal.GetArrayDataReference(data), (uint)data.Length);
    }
}
