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
    internal abstract class CollectionConverter<TCollection, T> : VariableConverter<TCollection> where TCollection : IEnumerable<T>
    {
        private readonly bool reverse;

        private readonly bool byArray;

        private readonly ToArray<TCollection, T> toArray;

        private readonly Converter<T> converter;

        private readonly Adapter<T> adapter;

        protected CollectionConverter(Converter<T> converter, bool reverse)
        {
            this.reverse = reverse;
            this.converter = converter;
            var method = typeof(TCollection).GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .Where(x => x.Name == "ToArray" && x.ReturnType == typeof(T[]) && x.GetParameters().Length == 0)
                .FirstOrDefault();
            adapter = AdapterHelper.Create(converter);
            byArray = converter.IsUnsafePrimitiveConverter() && (method != null || typeof(ICollection<T>).IsAssignableFrom(typeof(TCollection)));
            if (method == null)
                return;
            var source = Expression.Parameter(typeof(TCollection), "source");
            var invoke = Expression.Call(source, method);
            var lambda = Expression.Lambda<ToArray<TCollection, T>>(invoke, source);
            toArray = lambda.Compile();
        }

        protected ArraySegment<T> To(in ReadOnlySpan<byte> span)
        {
            var result = adapter.To(in span);
            if (reverse)
                Array.Reverse(result.Array, result.Offset, result.Count);
            return result;
        }

        public override void ToBytes(ref Allocator allocator, TCollection item)
        {
            if (item == null)
                return;
            else if (item is T[] array)
                adapter.Of(ref allocator, array);
            else if (item is List<T> value)
                adapter.OfList(ref allocator, value);
            else if (item is ArraySegment<T> segment)
                adapter.Of(ref allocator, segment);
            else if (item is IList<T> items)
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
