using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Mikodev.Binary.Internal
{
    internal static class SharedHelper
    {
        internal static readonly Encoding Encoding = Encoding.UTF8;

        internal static int SizeOfIPAddress(IPAddress item)
        {
            Debug.Assert(item is not null);
            var family = item.AddressFamily;
            return family is AddressFamily.InterNetwork ? 4 : 16;
        }

        internal static void EncodeIPAddress(ref Allocator allocator, IPAddress item)
        {
            Debug.Assert(item is not null);
            var size = SizeOfIPAddress(item);
            var flag = item.TryWriteBytes(MemoryMarshal.CreateSpan(ref Allocator.Assign(ref allocator, size), size), out var actual);
            Debug.Assert(flag);
            Debug.Assert(size == actual);
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
}
