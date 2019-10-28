using System.Diagnostics;

namespace Mikodev.Binary.Internal
{
    internal readonly struct HybridBuck
    {
        public readonly int Index;

        public readonly byte[] Bytes;

        public HybridBuck(int index, byte[] bytes)
        {
            Debug.Assert(index >= 0);
            Debug.Assert(bytes != null);
            Index = index;
            Bytes = bytes;
        }
    }
}
