using Mikodev.Binary.Internal.Adapters;
using Mikodev.Binary.Internal.Contexts.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Mikodev.Binary.Internal.Contexts
{
    internal static class FallbackCollectionMethods
    {
        private static readonly MethodInfo CreateSetConverterMethodInfo = typeof(FallbackCollectionMethods).GetMethod(nameof(CreateSetConverter), BindingFlags.Static | BindingFlags.NonPublic);

        private static readonly MethodInfo CreateLinkedListConverterMethodInfo = typeof(FallbackCollectionMethods).GetMethod(nameof(CreateLinkedListConverter), BindingFlags.Static | BindingFlags.NonPublic);

        private static readonly MethodInfo CreateEnumerableConverterMethodInfo = typeof(FallbackCollectionMethods).GetMethod(nameof(CreateEnumerableConverter), BindingFlags.Static | BindingFlags.NonPublic);

        private static readonly MethodInfo CreateDictionaryConverterMethodInfo = typeof(FallbackCollectionMethods).GetMethod(nameof(CreateDictionaryConverter), BindingFlags.Static | BindingFlags.NonPublic);

        internal static Converter GetConverter(IGeneratorContext context, Type type, Type enumerableArgument)
        {
            MethodInfo GetMethodInfo(out Type[] arguments)
            {
                if (CommonHelper.TryGetInterfaceArguments(type, typeof(IDictionary<,>), out arguments) || CommonHelper.TryGetInterfaceArguments(type, typeof(IReadOnlyDictionary<,>), out arguments))
                    return CreateDictionaryConverterMethodInfo;
                arguments = new[] { enumerableArgument };
                if (typeof(ISet<>).MakeGenericType(enumerableArgument).IsAssignableFrom(type))
                    return CreateSetConverterMethodInfo;
                else if (type == typeof(LinkedList<>).MakeGenericType(enumerableArgument))
                    return CreateLinkedListConverterMethodInfo;
                else
                    return CreateEnumerableConverterMethodInfo;
            }

            if (CommonHelper.IsImplementationOf(type, typeof(Stack<>)) || CommonHelper.IsImplementationOf(type, typeof(ConcurrentStack<>)))
                throw new ArgumentException($"Invalid collection type: {type}");
            var methodInfo = GetMethodInfo(out var itemTypes);
            var converters = itemTypes.Select(context.GetConverter).ToArray();
            var method = methodInfo.MakeGenericMethod(CommonHelper.Concat(type, itemTypes));
            var source = Expression.Parameter(typeof(IReadOnlyList<Converter>), "source");
            var lambda = Expression.Lambda<Func<IReadOnlyList<Converter>, Converter>>(Expression.Call(method, source), source);
            return lambda.Compile().Invoke(converters);
        }

        private static Converter CreateSetConverter<T, E>(IReadOnlyList<Converter> converters) where T : ISet<E>
        {
            var converter = (Converter<E>)converters.Single();
            var adapter = new SetAdapter<T, E>(converter);
            var builder = new FallbackEnumerableBuilder<T, HashSet<E>>();
            return new CollectionAdaptedConverter<T, T, HashSet<E>>(adapter, builder, converter.Length);
        }

        private static Converter CreateLinkedListConverter<T, E>(IReadOnlyList<Converter> converters)
        {
            Debug.Assert(typeof(T) == typeof(LinkedList<E>));
            var converter = (Converter<E>)converters.Single();
            var adapter = new LinkedListAdapter<E>(converter);
            var builder = new FallbackEnumerableBuilder<LinkedList<E>, LinkedList<E>>();
            return new CollectionAdaptedConverter<LinkedList<E>, LinkedList<E>, LinkedList<E>>(adapter, builder, converter.Length);
        }

        private static Converter CreateEnumerableConverter<T, E>(IReadOnlyList<Converter> converters) where T : IEnumerable<E>
        {
            var converter = (Converter<E>)converters.Single();
            var builder = typeof(T).IsAssignableFrom(typeof(ArraySegment<E>))
                ? new FallbackEnumerableBuilder<T, ArraySegment<E>>() as CollectionBuilder<T, T, ArraySegment<E>>
                : new DelegateEnumerableBuilder<T, ArraySegment<E>>(CreateCollectionConstructor<T, ArraySegment<E>>(typeof(T), typeof(IEnumerable<E>)));
            var adapter = new EnumerableAdapter<T, E>(converter);
            return new CollectionAdaptedConverter<T, T, ArraySegment<E>>(adapter, builder, converter.Length);
        }

        private static Converter CreateDictionaryConverter<T, K, V>(IReadOnlyList<Converter> converters) where T : IEnumerable<KeyValuePair<K, V>>
        {
            var itemLength = ContextMethods.GetItemLength(converters);
            var adapter = new DictionaryAdapter<T, K, V>((Converter<K>)converters[0], (Converter<V>)converters[1], itemLength);
            var builder = typeof(T).IsAssignableFrom(typeof(Dictionary<K, V>))
                ? new FallbackEnumerableBuilder<T, Dictionary<K, V>>() as CollectionBuilder<T, T, Dictionary<K, V>>
                : new DelegateEnumerableBuilder<T, Dictionary<K, V>>(CreateCollectionConstructor<T, Dictionary<K, V>>(typeof(T), typeof(IDictionary<K, V>)));
            return new CollectionAdaptedConverter<T, T, Dictionary<K, V>>(adapter, builder, itemLength);
        }

        private static Func<R, T> CreateCollectionConstructor<T, R>(Type type, Type enumerableType)
        {
            if (type.IsAbstract || type.IsInterface)
                return null;
            var constructor = type.GetConstructor(new[] { enumerableType });
            if (constructor is null)
                return null;
            var source = Expression.Parameter(typeof(R), "source");
            var invoke = Expression.New(constructor, Expression.Convert(source, enumerableType));
            var lambda = Expression.Lambda<Func<R, T>>(invoke, source);
            return lambda.Compile();
        }
    }
}
