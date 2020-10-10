using Mikodev.Binary.Internal.Contexts.Instance;
using Mikodev.Binary.Internal.Sequence;
using Mikodev.Binary.Internal.Sequence.Adapters;
using Mikodev.Binary.Internal.Sequence.Counters;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Mikodev.Binary.Internal.Contexts
{
    internal static class FallbackCollectionMethods
    {
        private sealed class It { }

        private static readonly IReadOnlyDictionary<Type, MethodInfo> ImmutableCollectionCreateMethods = new Dictionary<Type, MethodInfo>
        {
            [typeof(IImmutableDictionary<,>)] = GetMethodInfo(ImmutableDictionary.CreateRange),
            [typeof(IImmutableList<>)] = GetMethodInfo(ImmutableList.CreateRange),
            [typeof(IImmutableQueue<>)] = GetMethodInfo(ImmutableQueue.CreateRange),
            [typeof(IImmutableSet<>)] = GetMethodInfo(ImmutableHashSet.CreateRange),
            [typeof(ImmutableArray<>)] = GetMethodInfo(ImmutableArray.CreateRange),
            [typeof(ImmutableDictionary<,>)] = GetMethodInfo(ImmutableDictionary.CreateRange),
            [typeof(ImmutableHashSet<>)] = GetMethodInfo(ImmutableHashSet.CreateRange),
            [typeof(ImmutableList<>)] = GetMethodInfo(ImmutableList.CreateRange),
            [typeof(ImmutableQueue<>)] = GetMethodInfo(ImmutableQueue.CreateRange),
            [typeof(ImmutableSortedDictionary<,>)] = GetMethodInfo(ImmutableSortedDictionary.CreateRange),
            [typeof(ImmutableSortedSet<>)] = GetMethodInfo(ImmutableSortedSet.CreateRange),
        };

        private static readonly IReadOnlyList<Type> InvalidTypeDefinitions = new[]
        {
            typeof(Stack<>),
            typeof(ConcurrentStack<>),
            typeof(ImmutableStack<>),
            typeof(IImmutableStack<>),
        };

        internal static IConverter GetConverter(IGeneratorContext context, Type type)
        {
            if (CommonHelper.TryGetInterfaceArguments(type, typeof(IEnumerable<>), out var arguments) is false)
                return null;
            if (CommonHelper.SelectGenericTypeDefinitionOrDefault(type, InvalidTypeDefinitions.Contains))
                throw new ArgumentException($"Invalid collection type: {type}");
            if (CommonHelper.TryGetInterfaceArguments(type, typeof(IDictionary<,>), out var types) || CommonHelper.TryGetInterfaceArguments(type, typeof(IReadOnlyDictionary<,>), out types))
                return GetConverter(context, GetConverter<IEnumerable<KeyValuePair<It, It>>, It, It>, CommonHelper.Concat(type, types));
            else
                return GetConverter(context, GetConverter<IEnumerable<It>, It>, CommonHelper.Concat(type, arguments));
        }

        private static MethodInfo GetMethodInfo<T>(Func<IEnumerable<KeyValuePair<It, It>>, T> func) where T : IEnumerable<KeyValuePair<It, It>>
        {
            return func.Method.GetGenericMethodDefinition();
        }

        private static IConverter GetConverter(IGeneratorContext context, Func<IGeneratorContext, IConverter> func, params Type[] types)
        {
            var source = Expression.Parameter(typeof(IGeneratorContext), "context");
            var method = func.Method.GetGenericMethodDefinition().MakeGenericMethod(types);
            var lambda = Expression.Lambda<Func<IGeneratorContext, IConverter>>(Expression.Call(method, source), source);
            return lambda.Compile().Invoke(context);
        }

        private static Func<Expression, Expression> GetNewFuncOrDefault(Type type, Type enumerable)
        {
            if (type.IsAbstract || type.IsInterface)
                return null;
            var constructor = type.GetConstructors().FirstOrDefault(x => x.GetParameters() is { Length: 1 } data && data.Single().ParameterType == enumerable);
            if (constructor is null)
                return null;
            return x => Expression.New(constructor, x);
        }

        private static SequenceBuilder<T, R> GetBuilder<T, R, I>(Func<Expression, Expression> method)
        {
            static Func<R, T> Invoke(Func<Expression, Expression> method)
            {
                var source = Expression.Parameter(typeof(R), "source");
                var invoke = method.Invoke(Expression.Convert(source, typeof(I)));
                var lambda = Expression.Lambda<Func<R, T>>(invoke, source);
                return lambda.Compile();
            }

            return new DelegateBuilder<T, R>(method is null ? null : Invoke(method));
        }

        private static SequenceCounter<T> GetCounter<T, E>()
        {
            static Type Invoke()
            {
                if (typeof(ICollection<E>).IsAssignableFrom(typeof(T)))
                    return typeof(CollectionCounter<,>);
                if (typeof(IReadOnlyCollection<E>).IsAssignableFrom(typeof(T)))
                    return typeof(ReadOnlyCollectionCounter<,>);
                else
                    return null;
            }

            if (Invoke() is not { } type)
                return null;
            return (SequenceCounter<T>)Activator.CreateInstance(type.MakeGenericType(typeof(T), typeof(E)));
        }

        private static IConverter GetConverter<T, E>(IGeneratorContext context) where T : IEnumerable<E>
        {
            var converter = (Converter<E>)context.GetConverter(typeof(E));
            if (CommonHelper.SelectGenericTypeDefinitionOrDefault(typeof(T), ImmutableCollectionCreateMethods.GetValueOrDefault) is { } method)
                return GetEnumerableConverter<T, E>(converter, x => Expression.Call(method.MakeGenericMethod(typeof(E)), x));
            if (typeof(T) == typeof(HashSet<E>))
                return GetHashSetConverter(converter);
            if (typeof(T) == typeof(LinkedList<E>))
                return GetLinkedListConverter(converter);
            if (typeof(T).IsInterface && typeof(T).IsAssignableFrom(typeof(ArraySegment<E>)))
                return GetEnumerableInterfaceAssignableConverter<T, E>(converter);
            if (typeof(T).IsInterface && typeof(T).IsAssignableFrom(typeof(HashSet<E>)))
                return GetHashSetInterfaceAssignableConverter<T, E>(converter);
            return GetEnumerableConverter<T, E>(converter, GetNewFuncOrDefault(typeof(T), typeof(IEnumerable<E>)));
        }

        private static IConverter GetConverter<T, K, V>(IGeneratorContext context) where T : IEnumerable<KeyValuePair<K, V>>
        {
            var init = (Converter<K>)context.GetConverter(typeof(K));
            var tail = (Converter<V>)context.GetConverter(typeof(V));
            var itemLength = ContextMethods.GetItemLength(new IConverter[] { init, tail });
            if (CommonHelper.SelectGenericTypeDefinitionOrDefault(typeof(T), ImmutableCollectionCreateMethods.GetValueOrDefault) is { } method)
                return GetKeyValueEnumerableConverter<T, K, V>(init, tail, itemLength, x => Expression.Call(method.MakeGenericMethod(typeof(K), typeof(V)), x));
            if (typeof(T) == typeof(Dictionary<K, V>))
                return GetDictionaryConverter(init, tail, itemLength);
            if (typeof(T).IsInterface && typeof(T).IsAssignableFrom(typeof(Dictionary<K, V>)))
                return GetDictionaryInterfaceAssignableConverter<T, K, V>(init, tail, itemLength);
            if (GetNewFuncOrDefault(typeof(T), typeof(IDictionary<K, V>)) is { } result)
                return GetDictionaryConverter<T, K, V>(init, tail, itemLength, result);
            return GetKeyValueEnumerableConverter<T, K, V>(init, tail, itemLength, GetNewFuncOrDefault(typeof(T), typeof(IEnumerable<KeyValuePair<K, V>>)));
        }

        private static IConverter GetHashSetConverter<E>(Converter<E> converter)
        {
            var adapter = new SetAdapter<HashSet<E>, E>(converter);
            var builder = new FallbackBuilder<HashSet<E>>();
            var counter = new HashSetCounter<E>();
            return new SequenceConverter<HashSet<E>, HashSet<E>>(adapter, builder, counter, converter.Length);
        }

        private static IConverter GetHashSetInterfaceAssignableConverter<T, E>(Converter<E> converter) where T : IEnumerable<E>
        {
            var adapter = new SetAdapter<T, E>(converter);
            var builder = new DelegateBuilder<T, HashSet<E>>(x => (T)(object)x);
            var counter = GetCounter<T, E>();
            return new SequenceConverter<T, HashSet<E>>(adapter, builder, counter, converter.Length);
        }

        private static IConverter GetLinkedListConverter<E>(Converter<E> converter)
        {
            var adapter = new LinkedListAdapter<E>(converter);
            var builder = new FallbackBuilder<LinkedList<E>>();
            var counter = new LinkedListCounter<E>();
            return new SequenceConverter<LinkedList<E>, LinkedList<E>>(adapter, builder, counter, converter.Length);
        }

        private static IConverter GetEnumerableConverter<T, E>(Converter<E> converter, Func<Expression, Expression> method) where T : IEnumerable<E>
        {
            var adapter = new EnumerableAdapter<T, E>(converter);
            var builder = GetBuilder<T, ArraySegment<E>, IEnumerable<E>>(method);
            var counter = GetCounter<T, E>();
            return new SequenceConverter<T, ArraySegment<E>>(adapter, builder, counter, converter.Length);
        }

        private static IConverter GetEnumerableInterfaceAssignableConverter<T, E>(Converter<E> converter) where T : IEnumerable<E>
        {
            var adapter = new EnumerableAdapter<T, E>(converter);
            var builder = new DelegateBuilder<T, ArraySegment<E>>(x => (T)(object)x);
            var counter = GetCounter<T, E>();
            return new SequenceConverter<T, ArraySegment<E>>(adapter, builder, counter, converter.Length);
        }

        private static IConverter GetDictionaryConverter<K, V>(Converter<K> init, Converter<V> tail, int itemLength)
        {
            var adapter = new DictionaryAdapter<Dictionary<K, V>, K, V>(init, tail, itemLength);
            var builder = new FallbackBuilder<Dictionary<K, V>>();
            var counter = new DictionaryCounter<K, V>();
            return new SequenceConverter<Dictionary<K, V>, Dictionary<K, V>>(adapter, builder, counter, itemLength);
        }

        private static IConverter GetDictionaryConverter<T, K, V>(Converter<K> init, Converter<V> tail, int itemLength, Func<Expression, Expression> method) where T : IEnumerable<KeyValuePair<K, V>>
        {
            var adapter = new DictionaryAdapter<T, K, V>(init, tail, itemLength);
            var builder = GetBuilder<T, Dictionary<K, V>, IDictionary<K, V>>(method);
            var counter = GetCounter<T, KeyValuePair<K, V>>();
            return new SequenceConverter<T, Dictionary<K, V>>(adapter, builder, counter, itemLength);
        }

        private static IConverter GetDictionaryInterfaceAssignableConverter<T, K, V>(Converter<K> init, Converter<V> tail, int itemLength) where T : IEnumerable<KeyValuePair<K, V>>
        {
            var adapter = new DictionaryAdapter<T, K, V>(init, tail, itemLength);
            var builder = new DelegateBuilder<T, Dictionary<K, V>>(x => (T)(object)x);
            var counter = GetCounter<T, KeyValuePair<K, V>>();
            return new SequenceConverter<T, Dictionary<K, V>>(adapter, builder, counter, itemLength);
        }

        private static IConverter GetKeyValueEnumerableConverter<T, K, V>(Converter<K> init, Converter<V> tail, int itemLength, Func<Expression, Expression> method) where T : IEnumerable<KeyValuePair<K, V>>
        {
            var adapter = new KeyValueEnumerableAdapter<T, K, V>(init, tail, itemLength);
            var builder = GetBuilder<T, IEnumerable<KeyValuePair<K, V>>, IEnumerable<KeyValuePair<K, V>>>(method);
            var counter = GetCounter<T, KeyValuePair<K, V>>();
            return new SequenceConverter<T, IEnumerable<KeyValuePair<K, V>>>(adapter, builder, counter, itemLength);
        }
    }
}
