using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Mikodev.Binary.Internal
{
    internal ref struct MemoryBuffer<T>
    {
        private T[] buffer;

        private int cursor;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MemoryBuffer(int capacity)
        {
            this.buffer = new T[capacity];
            this.cursor = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Append(T item)
        {
            static void Expand(ref T[] buffer, T item)
            {
                var source = buffer;
                var cursor = source.Length;
                buffer = new T[checked(cursor * 2)];
                Array.Copy(source, 0, buffer, 0, cursor);
                buffer[cursor] = item;
            }

            if ((uint)this.cursor < (uint)this.buffer.Length)
                this.buffer[this.cursor] = item;
            else
                Expand(ref this.buffer, item);
            this.cursor++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MemoryResult<T> Result()
        {
            Debug.Assert(this.cursor >= 0);
            Debug.Assert(this.cursor <= this.buffer.Length);
            return new MemoryResult<T>(this.buffer, this.cursor);
        }
    }
}
