using Mikodev.Binary.Internal;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Mikodev.Binary
{
    public ref partial struct Allocator
    {
        internal static unsafe void AppendString(ref Allocator allocator, ref char source, int length, Encoding encoding)
        {
            if (encoding is null)
                ThrowHelper.ThrowArgumentEncodingInvalid();
            fixed (char* srcptr = &source)
            {
                var dstmax = StringHelper.GetMaxByteCount(encoding, srcptr, length);
                if (dstmax != 0)
                {
                    Ensure(ref allocator, dstmax);
                    var offset = allocator.offset;
                    var buffer = allocator.buffer;
                    fixed (byte* bufptr = &MemoryMarshal.GetReference(buffer))
                    {
                        var dstptr = bufptr + offset;
                        var dstlen = encoding.GetBytes(srcptr, length, dstptr, dstmax);
                        allocator.offset = offset + dstlen;
                    }
                }
            }
        }

        internal static unsafe void AppendStringWithLengthPrefix(ref Allocator allocator, ref char source, int length, Encoding encoding)
        {
            if (encoding is null)
                ThrowHelper.ThrowArgumentEncodingInvalid();
            fixed (char* srcptr = &source)
            {
                var dstmax = StringHelper.GetMaxByteCount(encoding, srcptr, length);
                if (dstmax != 0)
                {
                    var prefixLength = PrimitiveHelper.EncodeNumberLength((uint)dstmax);
                    Ensure(ref allocator, dstmax + prefixLength);
                    var offset = allocator.offset;
                    var buffer = allocator.buffer;
                    fixed (byte* bufptr = &MemoryMarshal.GetReference(buffer))
                    {
                        var dstptr = bufptr + offset;
                        var dstlen = encoding.GetBytes(srcptr, length, dstptr + prefixLength, dstmax);
                        ref var target = ref Unsafe.AsRef<byte>(dstptr);
                        PrimitiveHelper.EncodeNumber(ref target, prefixLength, (uint)dstlen);
                        allocator.offset = offset + dstlen + prefixLength;
                    }
                }
                else
                {
                    Append(ref allocator, 0);
                }
            }
        }
    }
}
