using System.Runtime.CompilerServices;

namespace Mikodev.Binary.Internal
{
    internal ref struct LengthReader
    {
        public readonly int Limits;

        public int Offset;

        public int Length;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LengthReader(int limits)
        {
            Limits = limits;
            Offset = 0;
            Length = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Any() => Limits - Offset != Length;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update(ref byte source)
        {
            Offset += Length;
            ref var location = ref Memory.Add(ref source, Offset);
            var prefixLength = PrimitiveHelper.DecodePrefixLength(location);
            if ((uint)(Limits - Offset) < (uint)prefixLength)
                goto fail;
            var length = PrimitiveHelper.DecodeLengthPrefix(ref location, prefixLength);
            Offset += prefixLength;
            if ((uint)(Limits - Offset) < (uint)length)
                goto fail;
            Length = length;
            return;

        fail:
            ThrowHelper.ThrowNotEnoughBytes();
        }
    }
}
