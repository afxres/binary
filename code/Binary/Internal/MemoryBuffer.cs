namespace Mikodev.Binary.Internal;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

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

    public readonly ArraySegment<T> GetArraySegment()
    {
        var buffer = this.buffer;
        var length = this.length;
        Debug.Assert((uint)length <= (uint)buffer.Length);
        return new ArraySegment<T>(buffer, 0, length);
    }

    public readonly List<T> GetList()
    {
        var buffer = this.buffer;
        var length = this.length;
        Debug.Assert((uint)length <= (uint)buffer.Length);
        return NativeModule.CreateList(buffer, length);
    }

    public readonly T[] GetArray()
    {
        var buffer = this.buffer;
        var length = this.length;
        Debug.Assert((uint)length <= (uint)buffer.Length);
        if (buffer.Length == length)
            return buffer;
        var target = new T[length];
        Array.Copy(buffer, 0, target, 0, length);
        return target;
    }

    public readonly IEnumerable<T> GetEnumerable()
    {
        var buffer = this.buffer;
        var length = this.length;
        Debug.Assert((uint)length <= (uint)buffer.Length);
        if (buffer.Length == length)
            return buffer;
        return NativeModule.CreateList(buffer, length);
    }
}
