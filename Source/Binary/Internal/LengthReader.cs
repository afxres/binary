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
            if ((uint)(Limits - Offset) < sizeof(int))
                ThrowHelper.ThrowNotEnoughBytes();
            var length = Endian<int>.Get(ref Memory.Add(ref source, Offset));
            Offset += sizeof(int);
            if ((uint)(Limits - Offset) < (uint)length)
                ThrowHelper.ThrowNotEnoughBytes();
            Length = length;
        }
    }
}
