using Mikodev.Binary.Delegates;
using System;
using System.Collections.Generic;

namespace Mikodev.Binary.Adapters.Abstractions
{
    internal abstract class Adapter<T> : Adapter
    {
        private readonly GetListItems<T> get = null;

        private readonly SetListItems<T> set = null;

        protected Adapter() => CreateDelegates(ref get, ref set);

        public abstract void OfArray(ref Allocator allocator, in ReadOnlySpan<T> span);

        public abstract T[] ToArray(in ReadOnlySpan<byte> span);

        public virtual void OfValue(ref Allocator allocator, List<T> list)
        {
            int itemCount;
            if (list == null || (itemCount = list.Count) == 0)
                return;
            var array = get == null ? list.ToArray() : get.Invoke(list);
            OfArray(ref allocator, new ReadOnlySpan<T>(array, 0, itemCount));
        }

        public virtual List<T> ToValue(in ReadOnlySpan<byte> span)
        {
            var array = ToArray(span);
            if (set == null)
                return new List<T>(array);
            var list = new List<T>();
            set.Invoke(list, array);
            return list;
        }
    }
}
