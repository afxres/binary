using Mikodev.Binary.Internal;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Mikodev.Binary
{
    public ref partial struct Allocator
    {
        internal static unsafe void AppendString(ref Allocator allocator, char* source, int length, Encoding encoding)
        {
            Debug.Assert(encoding != null);
            Debug.Assert(length >= 0);
            var dstmax = StringHelper.GetMaxByteCount(encoding, source, length);
            if (dstmax == 0)
                return;
            Ensure(ref allocator, dstmax);
            var offset = allocator.offset;
            var buffer = allocator.buffer;
            fixed (byte* bufptr = &MemoryMarshal.GetReference(buffer))
            {
                var dstptr = bufptr + offset;
                var dstlen = encoding.GetBytes(source, length, dstptr, dstmax);
                allocator.offset = offset + dstlen;
            }
        }

        internal static unsafe void AppendStringWithLengthPrefix(ref Allocator allocator, char* source, int length, Encoding encoding)
        {
            Debug.Assert(encoding != null);
            Debug.Assert(length >= 0);
            var dstmax = StringHelper.GetMaxByteCount(encoding, source, length);
            var prefixLength = PrimitiveHelper.EncodeNumberLength((uint)dstmax);
            Ensure(ref allocator, dstmax + prefixLength);
            var offset = allocator.offset;
            var buffer = allocator.buffer;
            fixed (byte* bufptr = &MemoryMarshal.GetReference(buffer))
            {
                var dstptr = bufptr + offset;
                var dstlen = dstmax == 0 ? 0 : encoding.GetBytes(source, length, dstptr + prefixLength, dstmax);
                ref var target = ref Unsafe.AsRef<byte>(dstptr);
                PrimitiveHelper.EncodeNumber(ref target, prefixLength, (uint)dstlen);
                allocator.offset = offset + dstlen + prefixLength;
            }
        }
    }
}
