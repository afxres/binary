using System;

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
    }
}
