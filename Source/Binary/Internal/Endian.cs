using System;
using System.Buffers.Binary;
using uint16 = System.UInt16;
using uint32 = System.UInt32;
using uint64 = System.UInt64;

namespace Mikodev.Binary.Internal
{
    internal static unsafe class Endian
    {
        private static readonly bool origin = BitConverter.IsLittleEndian == Converter.UseLittleEndian;

        private static void ReverseEndianness(void* target, void* source)
        {
            *(uint32*)((byte*)target + 0) = BinaryPrimitives.ReverseEndianness(*(uint32*)((byte*)source + 0));
            *(uint16*)((byte*)target + 4) = BinaryPrimitives.ReverseEndianness(*(uint16*)((byte*)source + 4));
            *(uint16*)((byte*)target + 6) = BinaryPrimitives.ReverseEndianness(*(uint16*)((byte*)source + 6));
            *(uint64*)((byte*)target + 8) = *(uint64*)((byte*)source + 8);
        }

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
    }
}
