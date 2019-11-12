using Mikodev.Binary.Creators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Mikodev.Binary.Internal.Adapters
{
    internal sealed class EnumerableAdapter<T, E> : CollectionAdapter<T, MemoryItem<E>, E> where T : IEnumerable<E>
    {
        private readonly bool byArray;

        private readonly ArrayLikeAdapter<E> adapter;

        private readonly ToArray<T, E> toArray;

        private readonly Converter<E> converter;

        public EnumerableAdapter(Converter<E> converter)
        {
            static ToArray<T, E> Compile(MethodInfo method)
            {
                var source = Expression.Parameter(typeof(T), "source");
                var invoke = Expression.Call(source, method);
                var lambda = Expression.Lambda<ToArray<T, E>>(invoke, source);
                return lambda.Compile();
            }

            this.converter = converter;
            var method = typeof(T).GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .Where(x => x.Name == "ToArray" && x.ReturnType == typeof(E[]) && x.GetParameters().Length == 0)
                .FirstOrDefault();
            adapter = ArrayLikeAdapterHelper.Create(converter);
            toArray = method == null ? null : Compile(method);
            byArray = converter.GetType().IsImplementationOf(typeof(OriginalEndiannessConverter<>)) && (method != null || typeof(ICollection<E>).IsAssignableFrom(typeof(T)));
        }

        public override void Of(ref Allocator allocator, T item)
        {
            if (item == null)
                return;
            else if (item is E[] array)
                adapter.Of(ref allocator, array);
            else if (item is IList<E> items)
                for (int i = 0, itemCount = items.Count; i < itemCount; i++)
                    converter.EncodeAuto(ref allocator, items[i]);
            else if (byArray)
                adapter.Of(ref allocator, toArray == null ? Enumerable.ToArray(item) : toArray.Invoke(item));
            else
                foreach (var i in item)
                    converter.EncodeAuto(ref allocator, i);
        }

        public override MemoryItem<E> To(ReadOnlySpan<byte> span) => adapter.To(span);
    }
}
