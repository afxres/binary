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

        private readonly bool isFixed;

        private readonly bool byArray;

        private readonly ToArray<TCollection, T> toArray;

        private readonly Converter<T> converter;

        private readonly Adapter<T> adapter;

        protected CollectionConverter(Converter<T> converter, bool reverse)
        {
            this.reverse = reverse;
            this.converter = converter;
            adapter = (Adapter<T>)AdapterHelper.Create(converter);

            var method = typeof(TCollection).GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .Where(x => x.Name == "ToArray" && x.ReturnType == typeof(T[]) && x.GetParameters().Length == 0)
                .FirstOrDefault();
            if (method != null)
            {
                var source = Expression.Parameter(typeof(TCollection), "source");
                var invoke = Expression.Call(source, method);
                var lambda = Expression.Lambda<ToArray<TCollection, T>>(invoke, source);
                toArray = lambda.Compile();
            }
            isFixed = converter.Length > 0;
            byArray = converter.IsUnsafePrimitiveConverter() && (toArray != null || typeof(ICollection<T>).IsAssignableFrom(typeof(TCollection)));
        }

        protected IList<T> GetCollection(in ReadOnlySpan<byte> span)
        {
            static T[] ToArray(Adapter<T> adapter, in ReadOnlySpan<byte> span, bool reverse)
            {
                var array = adapter.ToArray(in span);
                if (reverse)
                    Array.Reverse(array);
                return array;
            }

            static List<T> ToList(Adapter<T> adapter, in ReadOnlySpan<byte> span, bool reverse)
            {
                var value = adapter.ToList(in span);
                if (reverse)
                    value.Reverse();
                return value;
            }

            if (span.IsEmpty)
                return Array.Empty<T>();
            else if (isFixed)
                return ToArray(adapter, in span, reverse);
            return ToList(adapter, in span, reverse);
        }

        public override void ToBytes(ref Allocator allocator, TCollection item)
        {
            if (item == null)
                return;
            else if (item is T[] array)
                adapter.Of(ref allocator, array);
            else if (item is List<T> value)
                adapter.OfList(ref allocator, value);
            else if (item is IList<T> items && items.Count is var itemCount)
                for (var i = 0; i < itemCount; i++)
                    converter.ToBytesWithMark(ref allocator, items[i]);
            else if (byArray)
                adapter.Of(ref allocator, toArray == null ? Enumerable.ToArray(item) : toArray.Invoke(item));
            else
                foreach (var i in item)
                    converter.ToBytesWithMark(ref allocator, i);
        }
    }
}
