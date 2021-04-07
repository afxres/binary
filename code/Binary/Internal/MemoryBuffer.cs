using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Mikodev.Binary.Internal
{
    internal ref struct MemoryBuffer<T>
    {
        private T[] buffer;

        private int length;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MemoryBuffer(int capacity)
        {
            this.buffer = new T[capacity];
            this.length = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MemoryBuffer(T[] buffer, int length)
        {
            Debug.Assert((uint)length <= (uint)buffer.Length);
            this.buffer = buffer;
            this.length = length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(T item)
        {
            static void Expand(ref T[] buffer, T item)
            {
                var source = buffer;
                var cursor = source.Length;
                buffer = new T[checked(cursor * 2)];
                Array.Copy(source, 0, buffer, 0, cursor);
                buffer[cursor] = item;
            }

            var buffer = this.buffer;
            var length = this.length;
            if ((uint)length < (uint)buffer.Length)
                buffer[length] = item;
            else
                Expand(ref this.buffer, item);
            this.length++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Deconstruct(out T[] buffer, out int length)
        {
            Debug.Assert(this.length >= 0);
            Debug.Assert(this.length <= this.buffer.Length);
            buffer = this.buffer;
            length = this.length;
        }
    }
}
