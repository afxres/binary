using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Mikodev.Binary.Internal.Adapters
{
    internal sealed class EnumerableAdapter<T, E> : CollectionAdapter<T, ArraySegment<E>> where T : IEnumerable<E>
    {
        private readonly Func<T, E[]> array;

        private readonly Converter<E> converter;

        private readonly ArrayLikeAdapter<E> adapter;

        public EnumerableAdapter(Converter<E> converter)
        {
            this.converter = converter;
            this.adapter = ArrayLikeAdapterHelper.Create(converter);
            this.array = CreateArrayFunction();
        }

        internal static Func<T, E[]> CreateArrayFunction()
        {
            if (typeof(T).GetInterfaces().Contains(typeof(ICollection<E>)))
                return source => source.ToArray();
            var method = typeof(T).GetMethods(BindingFlags.Instance | BindingFlags.Public).FirstOrDefault(x => x.Name == "ToArray" && x.ReturnType == typeof(E[]) && x.GetParameters().Length == 0);
            if (method is null)
                return null;
            var source = Expression.Parameter(typeof(T), "source");
            var lambda = Expression.Lambda<Func<T, E[]>>(Expression.Call(source, method), source);
            return lambda.Compile();
        }

        internal E[] Array(T item)
        {
            Debug.Assert(item != null);
            if (item is E[] result)
                return result;
            if (array != null)
                return array.Invoke(item);
            if (!(item is ICollection<E> collection))
                return null;
            var target = new E[collection.Count];
            collection.CopyTo(target, default);
            return target;
        }

        public override int Count(T item) => item switch
        {
            null => 0,
            ICollection<E> { Count: var alpha } => alpha,
            IReadOnlyCollection<E> { Count: var bravo } => bravo,
            _ => -1,
        };

        public override void Of(ref Allocator allocator, T item)
        {
            if (item is null)
                return;
            else if (!(Array(item) is { } result))
                foreach (var i in item)
                    converter.EncodeAuto(ref allocator, i);
            else
                adapter.Of(ref allocator, new ReadOnlyMemory<E>(result));
        }

        public override ArraySegment<E> To(ReadOnlySpan<byte> span) => adapter.To(span);
    }
}
