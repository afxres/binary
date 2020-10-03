using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Mikodev.Binary.Internal
{
    internal readonly ref struct MemoryResult<T>
    {
        public readonly T[] Memory;

        public readonly int Length;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MemoryResult(T[] memory, int length)
        {
            Debug.Assert((uint)length <= (uint)memory.Length);
            this.Memory = memory;
            this.Length = length;
        }
    }
}
