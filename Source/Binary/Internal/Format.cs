using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Mikodev.Binary.Internal
{
    internal static class Format
    {
        internal static byte GetByte(ref ReadOnlySpan<byte> span)
        {
            var item = span[0];
            span = span.Slice(sizeof(byte));
            return item;
        }

        internal static void SetByte(ref Allocator allocator, byte item)
        {
            allocator.AllocateReference(sizeof(byte)) = item;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe string GetText(in ReadOnlySpan<byte> span)
        {
            var byteCount = span.Length;
            if (byteCount == 0)
                return string.Empty;
            var encoding = Converter.Encoding;
            fixed (byte* srcptr = &MemoryMarshal.GetReference(span))
                return encoding.GetString(srcptr, byteCount);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void SetText(ref Allocator allocator, string item)
        {
            allocator.Append(item.AsSpan());
        }
    }
}
