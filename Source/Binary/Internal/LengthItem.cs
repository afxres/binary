using System.Diagnostics;

namespace Mikodev.Binary.Internal
{
    internal readonly struct LengthItem
    {
        public readonly int Offset;

        public readonly int Length;

        public LengthItem(int offset, int length)
        {
            Debug.Assert(offset >= 1);
            Debug.Assert(length >= 0);
            Offset = offset;
            Length = length;
        }
    }
}
