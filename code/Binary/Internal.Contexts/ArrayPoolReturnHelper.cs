namespace Mikodev.Binary.Internal.Contexts;

using System;
using System.Buffers;

internal readonly ref struct ArrayPoolReturnHelper<T>(ArrayPool<T> arrays, ref T[] buffer) : IDisposable
{
    private readonly ArrayPool<T> arrays = arrays;

    private readonly ref T[] buffer = ref buffer;

    public readonly void Dispose()
    {
        this.arrays.Return(this.buffer);
    }
}
