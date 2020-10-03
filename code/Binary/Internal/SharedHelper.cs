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
            Debug.Assert(item != null);
            var family = item.AddressFamily;
            return family == AddressFamily.InterNetwork ? 4 : 16;
        }

        internal static void EncodeIPAddress(ref Allocator allocator, IPAddress item)
        {
            Debug.Assert(item != null);
            var size = SizeOfIPAddress(item);
            ref var target = ref Allocator.Assign(ref allocator, size);
            var flag = item.TryWriteBytes(MemoryMarshal.CreateSpan(ref target, size), out var actual);
            Debug.Assert(flag);
            Debug.Assert(size == actual);
        }

        internal static IPAddress DecodeIPAddress(ReadOnlySpan<byte> source)
        {
            return new IPAddress(source);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static string GetString(ReadOnlySpan<byte> source, Encoding encoding)
        {
            Debug.Assert(encoding != null);
            return encoding.GetString(source);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int GetBytes(ref char source, int sourceLength, ref byte target, int targetLength, Encoding encoding)
        {
            Debug.Assert(encoding != null);
            Debug.Assert(sourceLength > 0);
            Debug.Assert(targetLength > 0);
            var targetMemory = MemoryMarshal.CreateSpan(ref target, targetLength);
            var sourceMemory = MemoryMarshal.CreateReadOnlySpan(ref source, sourceLength);
            return encoding.GetBytes(sourceMemory, targetMemory);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int GetMaxByteCount(ReadOnlySpan<char> span, Encoding encoding)
        {
            Debug.Assert(encoding != null);
            var length = span.Length;
            if (length == 0)
                return 0;
            const int Limits = 32;
            if ((uint)length <= Limits && ReferenceEquals(encoding, Encoding))
                return (length + 1) * 3;
            return encoding.GetByteCount(span);
        }
    }
}
