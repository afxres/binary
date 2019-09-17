using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using uint16 = System.UInt16;
using uint32 = System.UInt32;
using uint64 = System.UInt64;

namespace Mikodev.Binary.Internal
{
    internal static unsafe class Format
    {
        #region private
        private static readonly bool origin = BitConverter.IsLittleEndian == Converter.UseLittleEndian;

        private static void ReverseEndianness(void* target, void* source)
        {
            *(uint32*)((byte*)target + 0) = BinaryPrimitives.ReverseEndianness(*(uint32*)((byte*)source + 0));
            *(uint16*)((byte*)target + 4) = BinaryPrimitives.ReverseEndianness(*(uint16*)((byte*)source + 4));
            *(uint16*)((byte*)target + 6) = BinaryPrimitives.ReverseEndianness(*(uint16*)((byte*)source + 6));
            *(uint64*)((byte*)target + 8) = *(uint64*)((byte*)source + 8);
        }
        #endregion

        public static Guid GetGuid(ref byte source)
        {
            fixed (byte* srcptr = &source)
            {
                if (origin)
                    return *(Guid*)srcptr;
                var result = default(Guid);
                ReverseEndianness(&result, srcptr);
                return result;
            }
        }

        public static void SetGuid(ref byte target, Guid item)
        {
            fixed (byte* dstptr = &target)
            {
                if (origin)
                    *(Guid*)dstptr = item;
                else
                    ReverseEndianness(dstptr, &item);
            }
        }

        public static byte GetByte(ref ReadOnlySpan<byte> span)
        {
            var item = span[0];
            span = span.Slice(sizeof(byte));
            return item;
        }

        public static void SetByte(ref Allocator allocator, byte item)
        {
            allocator.AllocateReference(sizeof(byte)) = item;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetText(in ReadOnlySpan<byte> span)
        {
            var byteCount = span.Length;
            if (byteCount == 0)
                return string.Empty;
            var encoding = Converter.Encoding;
            fixed (byte* srcptr = &MemoryMarshal.GetReference(span))
                return encoding.GetString(srcptr, byteCount);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetText(ref Allocator allocator, string item)
        {
            allocator.Append(item.AsSpan());
        }
    }
}
