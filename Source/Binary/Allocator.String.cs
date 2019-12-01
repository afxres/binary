using Mikodev.Binary.Internal;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Mikodev.Binary
{
    public ref partial struct Allocator
    {
        internal static unsafe void AppendString(ref Allocator allocator, ref char chars, int charCount)
        {
            var encoding = Converter.Encoding;
            fixed (char* srcptr = &chars)
            {
                var dstmax = StringHelper.GetMaxByteCountOrByteCount(encoding, srcptr, charCount);
                if (dstmax != 0)
                {
                    Ensure(ref allocator, dstmax);
                    var offset = allocator.offset;
                    var buffer = allocator.buffer;
                    fixed (byte* bufptr = &MemoryMarshal.GetReference(buffer))
                    {
                        var dstptr = bufptr + offset;
                        var length = encoding.GetBytes(srcptr, charCount, dstptr, dstmax);
                        allocator.offset = offset + length;
                    }
                }
            }
        }

        internal static unsafe void AppendStringWithLengthPrefix(ref Allocator allocator, ref char chars, int charCount)
        {
            var encoding = Converter.Encoding;
            fixed (char* srcptr = &chars)
            {
                var dstmax = StringHelper.GetMaxByteCountOrByteCount(encoding, srcptr, charCount);
                if (dstmax != 0)
                {
                    var prefixLength = PrimitiveHelper.EncodeNumberLength((uint)dstmax);
                    Ensure(ref allocator, dstmax + prefixLength);
                    var offset = allocator.offset;
                    var buffer = allocator.buffer;
                    fixed (byte* bufptr = &MemoryMarshal.GetReference(buffer))
                    {
                        var dstptr = bufptr + offset;
                        var length = encoding.GetBytes(srcptr, charCount, dstptr + prefixLength, dstmax);
                        ref var target = ref Unsafe.AsRef<byte>(dstptr);
                        PrimitiveHelper.EncodeNumber(ref target, prefixLength, (uint)length);
                        allocator.offset = offset + length + prefixLength;
                    }
                }
                else
                {
                    Append(ref allocator, 0);
                }
            }
        }

        internal static unsafe void AppendStringEncoding(ref Allocator allocator, ref char chars, int charCount, Encoding encoding)
        {
            if (encoding is null)
                ThrowHelper.ThrowArgumentEncodingInvalid();
            fixed (char* srcptr = &chars)
            {
                var dstlen = charCount == 0 ? 0 : encoding.GetByteCount(srcptr, charCount);
                if (dstlen != 0)
                {
                    fixed (byte* dstptr = &Assign(ref allocator, dstlen))
                    {
                        _ = encoding.GetBytes(srcptr, charCount, dstptr, dstlen);
                    }
                }
            }
        }

        internal static unsafe void AppendStringEncodingWithLengthPrefix(ref Allocator allocator, ref char chars, int charCount, Encoding encoding)
        {
            if (encoding is null)
                ThrowHelper.ThrowArgumentEncodingInvalid();
            fixed (char* srcptr = &chars)
            {
                var dstlen = charCount == 0 ? 0 : encoding.GetByteCount(srcptr, charCount);
                if (dstlen != 0)
                {
                    var prefixLength = PrimitiveHelper.EncodeNumberLength((uint)dstlen);
                    fixed (byte* dstptr = &Assign(ref allocator, dstlen + prefixLength))
                    {
                        ref var target = ref Unsafe.AsRef<byte>(dstptr);
                        PrimitiveHelper.EncodeNumber(ref target, prefixLength, (uint)dstlen);
                        _ = encoding.GetBytes(srcptr, charCount, dstptr + prefixLength, dstlen);
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
