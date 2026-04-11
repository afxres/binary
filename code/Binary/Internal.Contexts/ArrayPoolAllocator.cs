namespace Mikodev.Binary.Internal.Contexts;

using System;
using System.Buffers;
using System.Runtime.InteropServices;

internal sealed class ArrayPoolAllocator(ArrayPool<byte> arrays) : IAllocator, IDisposable
{
    private const int MinBufferLength = 64 * 1024;

    private readonly ArrayPool<byte> arrays = arrays;

    private byte[]? buffer;

    private bool disposed;

    public ref byte Resize(int length)
    {
        ObjectDisposedException.ThrowIf(this.disposed, GetType());
        ArgumentOutOfRangeException.ThrowIfNegative(length);
        var wanted = Math.Max(length, MinBufferLength);
        var buffer = this.buffer;
        if (buffer == null || wanted > buffer.Length)
        {
            var rented = this.arrays.Rent(wanted);
            if (rented is null || rented.Length < wanted)
                throw new InvalidOperationException($"Invalid array pool implementation detected");
            if (buffer != null)
            {
                Array.Copy(buffer, 0, rented, 0, buffer.Length);
                this.arrays.Return(buffer);
            }
            this.buffer = buffer = rented;
        }
        return ref MemoryMarshal.GetArrayDataReference(buffer);
    }

    public void Dispose()
    {
        this.disposed = true;
        if (this.buffer == null)
            return;
        this.arrays.Return(this.buffer);
        this.buffer = null;
    }
}
