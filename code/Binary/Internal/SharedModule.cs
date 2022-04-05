namespace Mikodev.Binary.Internal;

using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

internal static class SharedModule
{
    internal static readonly Encoding Encoding = Encoding.UTF8;

    internal static int SizeOf(IPAddress item)
    {
        Debug.Assert(item is not null);
        var family = item.AddressFamily;
        if (family is AddressFamily.InterNetwork)
            return 4;
        if (family is AddressFamily.InterNetworkV6)
            return 16;
        throw new ArgumentException($"Invalid address family: {family}");
    }

    internal static void Encode(ref Allocator allocator, IPAddress address, int addressSize)
    {
        Debug.Assert(address is not null);
        Debug.Assert(addressSize is 4 or 16);
        var flag = address.TryWriteBytes(MemoryMarshal.CreateSpan(ref Allocator.Assign(ref allocator, addressSize), addressSize), out var actual);
        Debug.Assert(flag);
        Debug.Assert(addressSize == actual);
    }

    internal static void Encode(ref Allocator allocator, IPAddress address, int addressSize, int port)
    {
        Debug.Assert(address is not null);
        Encode(ref allocator, address, addressSize);
        LittleEndian.Encode(ref allocator, (short)(ushort)port);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int GetMaxByteCount(ReadOnlySpan<char> span, Encoding encoding)
    {
        Debug.Assert(encoding is not null);
        var length = span.Length;
        if (length is 0)
            return 0;
        const int Limits = 32;
        if ((uint)length <= Limits && ReferenceEquals(encoding, Encoding))
            return (length + 1) * 3;
        return encoding.GetByteCount(span);
    }
}
