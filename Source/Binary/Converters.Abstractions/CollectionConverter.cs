using Mikodev.Binary.Abstractions;
using Mikodev.Binary.Adapters;
using Mikodev.Binary.Delegates;
using Mikodev.Binary.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Mikodev.Binary.Converters.Abstractions
{
    internal abstract class CollectionConverter<R, E> : VariableConverter<R> where R : IEnumerable<E>
    {
        private readonly bool reverse;

        private readonly bool byArray;

        private readonly ToArray<R, E> toArray;

        private readonly Converter<E> converter;

        private readonly Adapter<E> adapter;

        protected CollectionConverter(Converter<E> converter, bool reverse)
        {
            this.reverse = reverse;
            this.converter = converter;
            var method = typeof(R).GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .Where(x => x.Name == "ToArray" && x.ReturnType == typeof(E[]) && x.GetParameters().Length == 0)
                .FirstOrDefault();
            adapter = AdapterHelper.Create(converter);
            byArray = converter.IsUnsafePrimitiveConverter() && (method != null || typeof(ICollection<E>).IsAssignableFrom(typeof(R)));
            if (method == null)
                return;
            var source = Expression.Parameter(typeof(R), "source");
            var invoke = Expression.Call(source, method);
            var lambda = Expression.Lambda<ToArray<R, E>>(invoke, source);
            toArray = lambda.Compile();
        }

        protected ArraySegment<E> To(in ReadOnlySpan<byte> span)
        {
            var result = adapter.To(in span);
            if (reverse)
                MemoryExtensions.Reverse((Span<E>)result);
            return result;
        }

        public override void ToBytes(ref Allocator allocator, R item)
        {
            if (item == null)
                return;
            else if (item is E[] array)
                adapter.Of(ref allocator, array);
            else if (item is List<E> value)
                adapter.OfList(ref allocator, value);
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
    }
}
