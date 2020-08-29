using Mikodev.Binary.Internal;
using System;

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
            ref var source = ref MemoryHelper.EnsureLength(span);
            var numberLength = MemoryHelper.DecodeNumberLength(source);
            // check bounds via slice method
            span = span.Slice(numberLength);
            return MemoryHelper.DecodeNumber(ref source, numberLength);
        }
    }
}
