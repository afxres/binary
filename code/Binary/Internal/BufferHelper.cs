namespace Mikodev.Binary.Internal;

using System;
using System.Runtime.CompilerServices;

internal sealed class BufferHelper
{
    [ThreadStatic]
    private static BufferHelper ThreadStaticInstance;

    private static readonly BufferHelper GlobalSharedInstance = new BufferHelper(Array.Empty<byte>());

    private readonly byte[] buffer;

    private bool borrow;

    private BufferHelper(byte[] buffer) => this.buffer = buffer;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Span<byte> Result(BufferHelper buffer) => new Span<byte>(buffer.buffer);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void Return(BufferHelper buffer) => buffer.borrow = false;

    internal static BufferHelper Borrow()
    {
        var result = ThreadStaticInstance;
        if (result is null)
            ThreadStaticInstance = result = new BufferHelper(new byte[64 * 1024]);
        if (result.borrow)
            return GlobalSharedInstance;
        result.borrow = true;
        return result;
    }
}
