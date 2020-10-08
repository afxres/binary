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
                return Invoke(GetConverter<IDictionary<It, It>, It, It>, context, CommonHelper.Concat(type, types));
            else
                return Invoke(GetConverter<IEnumerable<It>, It>, context, CommonHelper.Concat(type, arguments));
        }

        private static MethodInfo GetMethodInfo<T>(Func<IEnumerable<KeyValuePair<It, It>>, T> func) where T : IEnumerable<KeyValuePair<It, It>>
        {
            return func.Method.GetGenericMethodDefinition();
        }

        private static U Invoke<T, U>(Func<T, U> func, T context, params Type[] types)
        {
            var source = Expression.Parameter(typeof(T), "context");
            var method = func.Method.GetGenericMethodDefinition().MakeGenericMethod(types);
            var lambda = Expression.Lambda<Func<T, U>>(Expression.Call(method, source), source);
            return lambda.Compile().Invoke(context);
        }

        private static IConverter Invoke<K, V>(IGeneratorContext context, Func<Converter<K>, Converter<V>, int, IConverter> func)
        {
            var initConverter = (Converter<K>)context.GetConverter(typeof(K));
            var tailConverter = (Converter<V>)context.GetConverter(typeof(V));
            var itemLength = ContextMethods.GetItemLength(new IConverter[] { initConverter, tailConverter });
            return func.Invoke(initConverter, tailConverter, itemLength);
        }

        private static Func<Expression, Expression> GetNewFuncOrDefault(Type type, Type enumerable)
        {
            if (type.IsInterface || type.IsAbstract)
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
            if (CommonHelper.SelectGenericTypeDefinitionOrDefault(typeof(T), ImmutableCollectionCreateMethods.GetValueOrDefault) is { } method)
                return GetEnumerableConverter<T, E>(context, x => Expression.Call(method.MakeGenericMethod(typeof(E)), x));
            if (typeof(T) == typeof(HashSet<E>))
                return GetHashSetConverter<E>(context);
            if (typeof(T) == typeof(LinkedList<E>))
                return GetLinkedListConverter<E>(context);
            if (typeof(T).IsInterface && typeof(T).IsAssignableFrom(typeof(ArraySegment<E>)))
                return GetEnumerableInterfaceAssignableConverter<T, E>(context);
            if (typeof(T).IsInterface && typeof(T).IsAssignableFrom(typeof(HashSet<E>)))
                return GetHashSetInterfaceAssignableConverter<T, E>(context);
            var func = GetNewFuncOrDefault(typeof(T), typeof(IEnumerable<E>));
            return GetEnumerableConverter<T, E>(context, func);
        }

        private static IConverter GetConverter<T, K, V>(IGeneratorContext context) where T : IEnumerable<KeyValuePair<K, V>>
        {
            if (CommonHelper.SelectGenericTypeDefinitionOrDefault(typeof(T), ImmutableCollectionCreateMethods.GetValueOrDefault) is { } method)
                return GetEnumerableConverter<T, KeyValuePair<K, V>>(context, x => Expression.Call(method.MakeGenericMethod(typeof(K), typeof(V)), x));
            if (typeof(T) == typeof(Dictionary<K, V>))
                return GetDictionaryConverter<K, V>(context);
            if (typeof(T).IsInterface && typeof(T).IsAssignableFrom(typeof(Dictionary<K, V>)))
                return GetDictionaryInterfaceAssignableConverter<T, K, V>(context);
            var func = GetNewFuncOrDefault(typeof(T), typeof(IDictionary<K, V>));
            if (func is null)
                return GetConverter<T, KeyValuePair<K, V>>(context);
            return GetDictionaryConverter<T, K, V>(context, func);
        }

        private static IConverter GetHashSetConverter<E>(IGeneratorContext context)
        {
            var converter = (Converter<E>)context.GetConverter(typeof(E));
            var adapter = new SetAdapter<HashSet<E>, E>(converter);
            return new SequenceConverter<HashSet<E>, HashSet<E>>(adapter, new FallbackBuilder<HashSet<E>>(), new HashSetCounter<E>(), converter.Length);
        }

        private static IConverter GetHashSetInterfaceAssignableConverter<T, E>(IGeneratorContext context) where T : IEnumerable<E>
        {
            var converter = (Converter<E>)context.GetConverter(typeof(E));
            var adapter = new SetAdapter<T, E>(converter);
            var builder = new DelegateBuilder<T, HashSet<E>>(x => (T)(object)x);
            var counter = GetCounter<T, E>();
            return new SequenceConverter<T, HashSet<E>>(adapter, builder, counter, converter.Length);
        }

        private static IConverter GetLinkedListConverter<E>(IGeneratorContext context)
        {
            var converter = (Converter<E>)context.GetConverter(typeof(E));
            var adapter = new LinkedListAdapter<E>(converter);
            return new SequenceConverter<LinkedList<E>, LinkedList<E>>(adapter, new FallbackBuilder<LinkedList<E>>(), new LinkedListCounter<E>(), converter.Length);
        }

        private static IConverter GetEnumerableConverter<T, E>(IGeneratorContext context, Func<Expression, Expression> method) where T : IEnumerable<E>
        {
            var converter = (Converter<E>)context.GetConverter(typeof(E));
            var adapter = new EnumerableAdapter<T, E>(converter);
            var builder = GetBuilder<T, ArraySegment<E>, IEnumerable<E>>(method);
            var counter = GetCounter<T, E>();
            return new SequenceConverter<T, ArraySegment<E>>(adapter, builder, counter, converter.Length);
        }

        private static IConverter GetEnumerableInterfaceAssignableConverter<T, E>(IGeneratorContext context) where T : IEnumerable<E>
        {
            var converter = (Converter<E>)context.GetConverter(typeof(E));
            var adapter = new EnumerableAdapter<T, E>(converter);
            var builder = new DelegateBuilder<T, ArraySegment<E>>(x => (T)(object)x);
            var counter = GetCounter<T, E>();
            return new SequenceConverter<T, ArraySegment<E>>(adapter, builder, counter, converter.Length);
        }

        private static IConverter GetDictionaryConverter<K, V>(IGeneratorContext context)
        {
            return Invoke<K, V>(context, (initConverter, tailConverter, itemLength) =>
            {
                var adapter = new DictionaryAdapter<Dictionary<K, V>, K, V>(initConverter, tailConverter, itemLength);
                var builder = new FallbackBuilder<Dictionary<K, V>>();
                var counter = new DictionaryCounter<K, V>();
                return new SequenceConverter<Dictionary<K, V>, Dictionary<K, V>>(adapter, builder, counter, itemLength);
            });
        }

        private static IConverter GetDictionaryConverter<T, K, V>(IGeneratorContext context, Func<Expression, Expression> method) where T : IEnumerable<KeyValuePair<K, V>>
        {
            return Invoke<K, V>(context, (initConverter, tailConverter, itemLength) =>
            {
                var adapter = new DictionaryAdapter<T, K, V>(initConverter, tailConverter, itemLength);
                var builder = GetBuilder<T, Dictionary<K, V>, IDictionary<K, V>>(method);
                var counter = GetCounter<T, KeyValuePair<K, V>>();
                return new SequenceConverter<T, Dictionary<K, V>>(adapter, builder, counter, itemLength);
            });
        }

        private static IConverter GetDictionaryInterfaceAssignableConverter<T, K, V>(IGeneratorContext context) where T : IEnumerable<KeyValuePair<K, V>>
        {
            return Invoke<K, V>(context, (initConverter, tailConverter, itemLength) =>
            {
                var adapter = new DictionaryAdapter<T, K, V>(initConverter, tailConverter, itemLength);
                var builder = new DelegateBuilder<T, Dictionary<K, V>>(x => (T)(object)x);
                var counter = GetCounter<T, KeyValuePair<K, V>>();
                return new SequenceConverter<T, Dictionary<K, V>>(adapter, builder, counter, itemLength);
            });
        }
    }
}
