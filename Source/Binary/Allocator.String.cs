using Mikodev.Binary.Internal;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Mikodev.Binary
{
    public ref partial struct Allocator
    {
        internal static void AppendString(ref Allocator allocator, ref char source, int sourceLength, Encoding encoding)
        {
            Debug.Assert(encoding != null);
            Debug.Assert(sourceLength >= 0);
            var targetLimits = StringHelper.GetMaxByteCount(encoding, ref source, sourceLength);
            if (targetLimits == 0)
                return;
            Ensure(ref allocator, targetLimits);
            var offset = allocator.offset;
            var buffer = allocator.buffer;
            ref var target = ref Unsafe.Add(ref MemoryMarshal.GetReference(buffer), offset);
            var targetLength = StringHelper.GetBytes(encoding, ref source, sourceLength, ref target, targetLimits);
            allocator.offset = offset + targetLength;
        }

        internal static void AppendStringWithLengthPrefix(ref Allocator allocator, ref char source, int sourceLength, Encoding encoding)
        {
            Debug.Assert(encoding != null);
            Debug.Assert(sourceLength >= 0);
            var targetLimits = StringHelper.GetMaxByteCount(encoding, ref source, sourceLength);
            var prefixLength = PrimitiveHelper.EncodeNumberLength((uint)targetLimits);
            Ensure(ref allocator, targetLimits + prefixLength);
            var offset = allocator.offset;
            var buffer = allocator.buffer;
            ref var target = ref Unsafe.Add(ref MemoryMarshal.GetReference(buffer), offset);
            var targetLength = targetLimits == 0 ? 0 : StringHelper.GetBytes(encoding, ref source, sourceLength, ref Unsafe.Add(ref target, prefixLength), targetLimits);
            PrimitiveHelper.EncodeNumber(ref target, prefixLength, (uint)targetLength);
            allocator.offset = offset + targetLength + prefixLength;
        }
    }
}
