namespace Mikodev.Binary.Experimental;

using System;
using System.Runtime.InteropServices;

public sealed class NativeMemoryAllocator : IAllocator, IDisposable
{
    private bool disposed = false;

    private unsafe void* location = null;

    public unsafe ref byte Allocate(int required)
    {
        if (this.disposed)
            throw new ObjectDisposedException(nameof(NativeMemoryAllocator));
        return ref *(byte*)(this.location = NativeMemory.Realloc(this.location, (uint)required));
    }

    public unsafe void Dispose()
    {
        if (this.disposed)
            return;
        NativeMemory.Free(this.location);
        this.location = null;
        this.disposed = true;
    }
}
