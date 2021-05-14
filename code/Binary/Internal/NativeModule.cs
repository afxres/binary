using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Mikodev.Binary.Internal
{
    internal static class NativeModule
    {
        private sealed class RawListData<T>
        {
            public T[] Data;

            public int Size;
        }

        internal static List<T> CreateList<T>(T[] buffer, int length)
        {
            Debug.Assert((uint)length <= (uint)buffer.Length);
            var list = new List<T>();
            Unsafe.As<RawListData<T>>(list).Data = buffer;
            Unsafe.As<RawListData<T>>(list).Size = length;
            return list;
        }
    }
}
