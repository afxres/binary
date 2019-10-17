using Mikodev.Binary.CollectionAdapters;
using Mikodev.Binary.Internal.Delegates;
using Mikodev.Binary.Internal.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Mikodev.Binary.Internal.Components
{
    internal readonly struct CollectionConverter<T, E> where T : IEnumerable<E>
    {
        private readonly bool reverse;

        private readonly bool byArray;

        private readonly CollectionAdapter<E> adapter;

        private readonly ToArray<T, E> toArray;

        private readonly Converter<E> converter;

        public CollectionConverter(Converter<E> converter, bool reverse)
        {
            static ToArray<T, E> Compile(MethodInfo method)
            {
                var source = Expression.Parameter(typeof(T), "source");
                var invoke = Expression.Call(source, method);
                var lambda = Expression.Lambda<ToArray<T, E>>(invoke, source);
                return lambda.Compile();
            }

            this.reverse = reverse;
            this.converter = converter;
            var method = typeof(T).GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .Where(x => x.Name == "ToArray" && x.ReturnType == typeof(E[]) && x.GetParameters().Length == 0)
                .FirstOrDefault();
            adapter = (CollectionAdapter<E>)CollectionAdapterHelper.Create(converter);
            toArray = method == null ? null : Compile(method);
            byArray = converter.IsOriginalEndiannessConverter() && (method != null || typeof(ICollection<E>).IsAssignableFrom(typeof(T)));
        }

        public void Of(ref Allocator allocator, T item)
        {
            if (item == null)
                return;
            else if (item is E[] array)
                adapter.Of(ref allocator, array);
            else if (item is ArraySegment<E> segment)
                adapter.Of(ref allocator, segment);
            else if (item is IList<E> items)
                for (int i = 0, itemCount = items.Count; i < itemCount; i++)
                    converter.ToBytesWithMark(ref allocator, items[i]);
            else if (byArray)
                adapter.Of(ref allocator, toArray == null ? Enumerable.ToArray(item) : toArray.Invoke(item));
            else
                foreach (var i in item)
                    converter.ToBytesWithMark(ref allocator, i);
        }

        public ArraySegment<E> To(in ReadOnlySpan<byte> span)
        {
            var result = adapter.To(in span);
            if (reverse)
                MemoryExtensions.Reverse((Span<E>)result);
            return result;
        }
    }
}
