namespace Mikodev.Binary.Internal;

using System;
using System.Runtime.CompilerServices;

internal static class BridgeModule
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static ReadOnlySpan<T> CreateReadOnlySpan<T>(T[]? data)
    {
        return new ReadOnlySpan<T>(data);
    }
}
