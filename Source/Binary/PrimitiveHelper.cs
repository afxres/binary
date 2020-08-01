using Mikodev.Binary.Internal;
using System;
using System.Runtime.InteropServices;

namespace Mikodev.Binary
{
    public static partial class PrimitiveHelper
    {
        public static void EncodeNumber(ref Allocator allocator, int number)
        {
            if (number < 0)
                ThrowHelper.ThrowNumberNegative();
            var numberLength = MemoryHelper.EncodeNumberLength((uint)number);
            MemoryHelper.EncodeNumber(ref Allocator.Assign(ref allocator, numberLength), (uint)number, numberLength);
        }

        public static int DecodeNumber(ref ReadOnlySpan<byte> span)
        {
            if (span.IsEmpty)
                return ThrowHelper.ThrowNotEnoughBytes<int>();
            ref var source = ref MemoryMarshal.GetReference(span);
            var numberLength = MemoryHelper.DecodeNumberLength(source);
            // check bounds via slice method
            span = span.Slice(numberLength);
            return MemoryHelper.DecodeNumber(ref source, numberLength);
        }
    }
}
