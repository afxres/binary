using Mikodev.Binary.Internal.SpanLike;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Mikodev.Binary.Internal.Sequence.Adapters
{
    internal sealed class EnumerableAdapter<T, E> : SequenceAdapter<T, ArraySegment<E>> where T : IEnumerable<E>
    {
        private readonly Func<T, E[]> functor;

        private readonly Converter<E> converter;

        private readonly SpanLikeAdapter<E> adapter;

        public EnumerableAdapter(Converter<E> converter)
        {
            static Func<T, E[]> Invoke()
            {
                var method = typeof(T).GetMethods(BindingFlags.Instance | BindingFlags.Public).FirstOrDefault(x => x.Name == "ToArray" && x.ReturnType == typeof(E[]) && x.GetParameters().Length == 0);
                if (method is null)
                    return null;
                var source = Expression.Parameter(typeof(T), "source");
                var lambda = Expression.Lambda<Func<T, E[]>>(Expression.Call(source, method), source);
                return lambda.Compile();
            }

            this.converter = converter;
            this.adapter = SpanLikeAdapterHelper.Create(converter);
            this.functor = Invoke();
        }

        public override void Encode(ref Allocator allocator, T item)
        {
            static E[] Invoke(T item, Func<T, E[]> func)
            {
                if (item is E[] result)
                    return result;
                if (func != null)
                    return func.Invoke(item);
                if (item is ICollection<E> data)
                    return SequenceMethods.GetContents(data);
                return null;
            }

            if (item is null)
                return;
            if (Invoke(item, functor) is { } result)
                adapter.Encode(ref allocator, new ReadOnlySpan<E>(result));
            else
                foreach (var i in item)
                    converter.EncodeAuto(ref allocator, i);
        }

        public override ArraySegment<E> Decode(ReadOnlySpan<byte> span)
        {
            var data = adapter.Decode(span);
            Debug.Assert((uint)data.Length <= (uint)data.Memory.Length);
            return new ArraySegment<E>(data.Memory, 0, data.Length);
        }
    }
}
