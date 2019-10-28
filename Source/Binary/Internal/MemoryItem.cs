using System;
using System.Diagnostics;

namespace Mikodev.Binary.Internal
{
    internal readonly struct MemoryItem<T>
    {
        public readonly T[] Buffer;

        public readonly int Length;

        public MemoryItem(T[] buffer, int length)
        {
            Debug.Assert(buffer != null);
            Debug.Assert(length >= 0 && length <= buffer.Length);
            Buffer = buffer;
            Length = length;
        }

        public ArraySegment<T> AsArraySegment()
        {
            Debug.Assert(Buffer != null);
            Debug.Assert(Length >= 0 && Length <= Buffer.Length);
            Debug.Assert(Length != 0 || ReferenceEquals(Buffer, Array.Empty<T>()));
            return new ArraySegment<T>(Buffer, 0, Length);
        }
    }
}
