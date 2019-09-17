using System;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using uint16 = System.UInt16;
using uint32 = System.UInt32;
using uint64 = System.UInt64;

namespace Mikodev.Binary.Internal
{
    internal static unsafe class Endian<T> where T : unmanaged
    {
        #region private members
        private static readonly bool origin = BitConverter.IsLittleEndian == Converter.UseLittleEndian || sizeof(T) == 1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ReverseEndianness(void* target, void* source)
        {
            switch (sizeof(T))
            {
                case sizeof(uint16):
                    *(uint16*)target = BinaryPrimitives.ReverseEndianness(*(uint16*)source);
                    break;
                case sizeof(uint32):
                    *(uint32*)target = BinaryPrimitives.ReverseEndianness(*(uint32*)source);
                    break;
                case sizeof(uint64):
                    *(uint64*)target = BinaryPrimitives.ReverseEndianness(*(uint64*)source);
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ReverseEndianness(void* target, void* source, int byteCount)
        {
            Debug.Assert((byteCount % sizeof(T)) == 0);
            switch (sizeof(T))
            {
                case sizeof(uint16):
                    for (var i = 0; i < byteCount; i += sizeof(uint16))
                        *(uint16*)((byte*)target + i) = BinaryPrimitives.ReverseEndianness(*(uint16*)((byte*)source + i));
                    break;
                case sizeof(uint32):
                    for (var i = 0; i < byteCount; i += sizeof(uint32))
                        *(uint32*)((byte*)target + i) = BinaryPrimitives.ReverseEndianness(*(uint32*)((byte*)source + i));
                    break;
                case sizeof(uint64):
                    for (var i = 0; i < byteCount; i += sizeof(uint64))
                        *(uint64*)((byte*)target + i) = BinaryPrimitives.ReverseEndianness(*(uint64*)((byte*)source + i));
                    break;
                default:
                    throw new NotSupportedException();
            }
        }
        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Copy(ref byte target, ref byte source, int byteCount)
        {
            Debug.Assert((byteCount % sizeof(T)) == 0);
            if (origin)
                Memory.Copy(ref target, ref source, byteCount);
            else
                fixed (byte* dstptr = &target)
                fixed (byte* srcptr = &source)
                    ReverseEndianness(dstptr, srcptr, byteCount);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static T Get(ref byte source)
        {
            fixed (byte* srcptr = &source)
            {
                if (origin)
                    return *(T*)srcptr;
                T result;
                ReverseEndianness(&result, srcptr);
                return result;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Set(ref byte target, T item)
        {
            fixed (byte* dstptr = &target)
            {
                if (origin)
                    *(T*)dstptr = item;
                else
                    ReverseEndianness(dstptr, &item);
            }
        }
    }
}
