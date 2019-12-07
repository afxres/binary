using Mikodev.Binary.Internal;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using u1 = System.Byte;
using u2 = System.UInt16;
using u4 = System.UInt32;
using u8 = System.UInt64;

namespace Mikodev.Binary
{
    public ref partial struct Allocator
    {
        private static unsafe void MemoryMoveForward3(sbyte* source, uint length)
        {
            Debug.Assert(length > 0 && length < 32);

            if ((length & 16) != 0)
            {
                var q0 = *(u8*)(source + 0);
                var q1 = *(u8*)(source + 8);
                *(u8*)(source - 3) = q0;
                *(u8*)(source + 5) = q1;
                source += 16;
            }

            if ((length & 8) != 0)
            {
                *(u8*)(source - 3) = *(u8*)source;
                source += 8;
            }

            if ((length & 4) != 0)
            {
                *(u4*)(source - 3) = *(u4*)source;
                source += 4;
            }

            if ((length & 2) != 0)
            {
                *(u2*)(source - 3) = *(u2*)source;
                source += 2;
            }

            if ((length & 1) != 0)
            {
                *(u1*)(source - 3) = *(u1*)source;
            }
        }

        internal static unsafe void AppendLengthPrefix(ref Allocator allocator, int anchor, bool compact)
        {
            const int Limits = 32;
            var offset = allocator.offset;
            var buffer = allocator.buffer;
            // check bounds manually
            if ((ulong)(uint)anchor + sizeof(uint) > (uint)offset)
                ThrowHelper.ThrowAllocatorAnchorInvalid();
            ref var target = ref Unsafe.Add(ref MemoryMarshal.GetReference(buffer), anchor);
            var length = offset - anchor - sizeof(uint);
            if (compact && length < Limits)
            {
                if (length > 0)
                    fixed (byte* datptr = &target)
                        MemoryMoveForward3((sbyte*)(datptr + sizeof(uint)), (uint)length);
                target = (byte)length;
                allocator.offset = offset - (sizeof(uint) - sizeof(byte));
            }
            else
            {
                PrimitiveHelper.EncodeNumberFixed4(ref target, (uint)length);
            }
        }
    }
}
