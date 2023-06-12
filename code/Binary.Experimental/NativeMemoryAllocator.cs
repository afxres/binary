namespace Mikodev.Binary.Experimental;

using System;
using System.Runtime.InteropServices;

public sealed class NativeMemoryAllocator : IAllocator, IDisposable
{
    private bool disposed = false;

    private unsafe void* location = null;

    public unsafe ref byte Resize(int length)
    {
        ObjectDisposedException.ThrowIf(this.disposed, typeof(NativeMemoryAllocator));
        this.location = NativeMemory.Realloc(this.location, (uint)length);
        return ref *(byte*)(this.location);
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
