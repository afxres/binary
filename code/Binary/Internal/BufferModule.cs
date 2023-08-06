namespace Mikodev.Binary.Internal;

using System;
using System.Runtime.CompilerServices;

internal sealed class BufferModule
{
    [ThreadStatic]
    private static BufferModule? ThreadStaticInstance;

    private static readonly BufferModule GlobalSharedInstance = new BufferModule(Array.Empty<byte>());

    private readonly byte[] buffer;

    private bool borrow;

    private BufferModule(byte[] buffer) => this.buffer = buffer;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Span<byte> Intent(BufferModule buffer) => new Span<byte>(buffer.buffer);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void Return(BufferModule buffer) => buffer.borrow = false;

    internal static BufferModule Borrow()
    {
        var result = ThreadStaticInstance;
        if (result is null)
            ThreadStaticInstance = result = new BufferModule(new byte[64 * 1024]);
        if (result.borrow)
            return GlobalSharedInstance;
        result.borrow = true;
        return result;
    }
}
