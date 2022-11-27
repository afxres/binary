namespace Mikodev.Binary.Internal;

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

internal static class BridgeModule
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool NotDefaultValue<T>(T? item)
    {
        return EqualityComparer<T>.Default.Equals(item, default) is false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ReadOnlySpan<T> CreateReadOnlySpan<T>(T[]? data)
    {
        return new ReadOnlySpan<T>(data);
    }
}
