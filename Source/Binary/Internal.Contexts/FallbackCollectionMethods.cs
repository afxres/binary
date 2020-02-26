using Mikodev.Binary.Internal.Adapters;
using Mikodev.Binary.Internal.Contexts.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Mikodev.Binary.Internal.Contexts
{
    internal static class FallbackCollectionMethods
    {
        private static readonly MethodInfo CreateSetMethodInfo = typeof(FallbackCollectionMethods).GetMethod(nameof(CreateSetConverter), BindingFlags.Static | BindingFlags.NonPublic);

        private static readonly MethodInfo CreateLinkedListMethodInfo = typeof(FallbackCollectionMethods).GetMethod(nameof(CreateLinkedListConverter), BindingFlags.Static | BindingFlags.NonPublic);

        private static readonly MethodInfo CreateEnumerableMethodInfo = typeof(FallbackCollectionMethods).GetMethod(nameof(CreateEnumerableConverter), BindingFlags.Static | BindingFlags.NonPublic);

        private static readonly MethodInfo CreateDictionaryMethodInfo = typeof(FallbackCollectionMethods).GetMethod(nameof(CreateDictionaryConverter), BindingFlags.Static | BindingFlags.NonPublic);

        internal static Converter GetConverter(IGeneratorContext context, Type type, Type itemType)
        {
            MethodInfo GetMethodInfo(out Type[] arguments)
            {
                if (type.TryGetInterfaceArguments(typeof(IDictionary<,>), out arguments) || type.TryGetInterfaceArguments(typeof(IReadOnlyDictionary<,>), out arguments))
                    return CreateDictionaryMethodInfo;
                arguments = new[] { itemType };
                if (typeof(ISet<>).MakeGenericType(itemType).IsAssignableFrom(type))
                    return CreateSetMethodInfo;
                else if (type == typeof(LinkedList<>).MakeGenericType(itemType))
                    return CreateLinkedListMethodInfo;
                else
                    return CreateEnumerableMethodInfo;
            }

            var methodInfo = GetMethodInfo(out var arguments);
            var converters = arguments.Select(context.GetConverter).ToArray();
            var method = methodInfo.MakeGenericMethod(new[] { type }.Concat(arguments).ToArray());
            var source = Expression.Parameter(typeof(IReadOnlyList<Converter>), "source");
            var lambda = Expression.Lambda<Func<IReadOnlyList<Converter>, Converter>>(Expression.Call(method, source), source);
            return lambda.Compile().Invoke(converters);
        }

        private static Converter CreateSetConverter<T, E>(IReadOnlyList<Converter> converters) where T : ISet<E>
        {
            var converter = (Converter<E>)converters.Single();
            var adapter = new SetAdapter<T, E>(converter);
            var builder = new FallbackEnumerableBuilder<T, HashSet<E>>();
            return new CollectionAdaptedConverter<T, HashSet<E>, E>(adapter, builder, converter.Length);
        }

        private static Converter CreateLinkedListConverter<T, E>(IReadOnlyList<Converter> converters)
        {
            var converter = (Converter<E>)converters.Single();
            var adapter = new LinkedListAdapter<E>(converter);
            var builder = new FallbackEnumerableBuilder<LinkedList<E>, LinkedList<E>>();
            return new CollectionAdaptedConverter<LinkedList<E>, LinkedList<E>, E>(adapter, builder, converter.Length);
        }

        private static Converter CreateEnumerableConverter<T, E>(IReadOnlyList<Converter> converters) where T : IEnumerable<E>
        {
            var converter = (Converter<E>)converters.Single();
            var builder = typeof(T).IsAssignableFrom(typeof(ArraySegment<E>))
                ? new FallbackEnumerableBuilder<T, ArraySegment<E>>() as CollectionBuilder<T, T, ArraySegment<E>>
                : new DelegateCollectionBuilder<T, E>(GetDecodeDelegateAsEnumerable<ToCollection<T, E>>(typeof(T), typeof(IEnumerable<E>)));
            var adapter = new EnumerableAdapter<T, E>(converter);
            return new CollectionAdaptedConverter<T, ArraySegment<E>, E>(adapter, builder, converter.Length);
        }

        private static Converter CreateDictionaryConverter<T, K, V>(IReadOnlyList<Converter> converters) where T : IEnumerable<KeyValuePair<K, V>>
        {
            var itemLength = ContextMethods.GetItemLength(typeof(KeyValuePair<K, V>), converters);
            var adapter = new DictionaryAdapter<T, K, V>((Converter<K>)converters[0], (Converter<V>)converters[1], itemLength);
            var builder = typeof(T).IsAssignableFrom(typeof(Dictionary<K, V>))
                ? new FallbackEnumerableBuilder<T, Dictionary<K, V>>() as CollectionBuilder<T, T, Dictionary<K, V>>
                : new DelegateDictionaryBuilder<T, K, V>(GetDecodeDelegateAsEnumerable<ToDictionary<T, K, V>>(typeof(T), typeof(IDictionary<K, V>)));
            return new CollectionAdaptedConverter<T, Dictionary<K, V>, KeyValuePair<K, V>>(adapter, builder, itemLength);
        }

        private static D GetDecodeDelegateAsEnumerable<D>(Type type, Type enumerableType) where D : Delegate
        {
            if (type.IsAbstract || type.IsInterface)
                return null;
            var constructor = type.GetConstructor(new[] { enumerableType });
            if (constructor is null)
                return null;
            var source = Expression.Parameter(enumerableType, "source");
            var lambda = Expression.Lambda<D>(Expression.New(constructor, source), source);
            return lambda.Compile();
        }
    }
}
