using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Mikodev.Binary.Internal.Adapters
{
    internal sealed class EnumerableAdapter<T, E> : CollectionAdapter<T, MemoryItem<E>> where T : IEnumerable<E>
    {
        private readonly Func<T, E[]> array;

        private readonly Func<T, int> count;

        private readonly Converter<E> converter;

        private readonly ArrayLikeAdapter<E> adapter;

        public EnumerableAdapter(Converter<E> converter)
        {
            var arrayExpression = CreateArrayExpression();
            var countExpression = CreateCountExpression();
            this.converter = converter;
            this.adapter = ArrayLikeAdapterHelper.Create(converter);
            this.array = arrayExpression?.Compile();
            this.count = countExpression?.Compile();
        }

        private static Expression<Func<T, E[]>> CreateArrayExpression()
        {
            if (typeof(T).GetInterfaces().Contains(typeof(ICollection<E>)))
                return source => source.ToArray();
            var method = typeof(T).GetMethods(BindingFlags.Instance | BindingFlags.Public).FirstOrDefault(x => x.Name == "ToArray" && x.ReturnType == typeof(E[]) && x.GetParameters().Length == 0);
            if (method is null)
                return null;
            var source = Expression.Parameter(typeof(T), "source");
            var lambda = Expression.Lambda<Func<T, E[]>>(Expression.Call(source, method), source);
            return lambda;
        }

        private static Expression<Func<T, int>> CreateCountExpression()
        {
            var interfaces = typeof(T).GetInterfaces();
            if (interfaces.Contains(typeof(ICollection<E>)))
                return source => ((ICollection<E>)source).Count;
            if (interfaces.Contains(typeof(IReadOnlyCollection<E>)))
                return source => ((IReadOnlyCollection<E>)source).Count;
            return null;
        }

        public override void Of(ref Allocator allocator, T item)
        {
            const int Limits = 8;
            if (item is null)
                return;
            else if (item is E[] data)
                adapter.Of(ref allocator, new ReadOnlyMemory<E>(data));
            else if (array is null || (count != null && count.Invoke(item) < Limits))
                foreach (var i in item)
                    converter.EncodeAuto(ref allocator, i);
            else
                adapter.Of(ref allocator, new ReadOnlyMemory<E>(array.Invoke(item)));
        }

        public override MemoryItem<E> To(ReadOnlySpan<byte> span) => adapter.To(span);
    }
}
