using Mikodev.Binary.Internal.Contexts.Instance;
using Mikodev.Binary.Internal.Sequence;
using Mikodev.Binary.Internal.Sequence.Adapters;
using Mikodev.Binary.Internal.Sequence.Counters;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Mikodev.Binary.Internal.Contexts
{
    internal static class FallbackCollectionMethods
    {
        private sealed class Placeholder { }

        private static readonly MethodInfo GetSetConverterMethodInfo = GetMethodInfo(GetSetConverter<ISet<Placeholder>, Placeholder>);

        private static readonly MethodInfo GetLinkedListConverterMethodInfo = GetMethodInfo(GetLinkedListConverter<LinkedList<Placeholder>, Placeholder>);

        private static readonly MethodInfo GetEnumerableConverterMethodInfo = GetMethodInfo(GetEnumerableConverter<IEnumerable<Placeholder>, Placeholder>);

        private static readonly MethodInfo GetDictionaryConverterMethodInfo = GetMethodInfo(GetDictionaryConverter<IEnumerable<KeyValuePair<Placeholder, Placeholder>>, Placeholder, Placeholder>);

        private static readonly IReadOnlyDictionary<Type, MethodInfo> CreateMethods = new Dictionary<Type, MethodInfo>
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

        private static readonly IReadOnlyList<Type> InvalidTypes = new[]
        {
            typeof(IImmutableStack<>),
            typeof(ImmutableStack<>),
            typeof(Stack<>),
            typeof(ConcurrentStack<>),
        };

        internal static IConverter GetConverter(IGeneratorContext context, Type type)
        {
            MethodInfo Method(Type argument, out Type[] arguments)
            {
                if (CommonHelper.TryGetInterfaceArguments(type, typeof(IDictionary<,>), out arguments) || CommonHelper.TryGetInterfaceArguments(type, typeof(IReadOnlyDictionary<,>), out arguments))
                    return GetDictionaryConverterMethodInfo;
                arguments = new[] { argument };
                if (type == typeof(LinkedList<>).MakeGenericType(argument))
                    return GetLinkedListConverterMethodInfo;
                if (type == typeof(HashSet<>).MakeGenericType(argument))
                    return GetSetConverterMethodInfo;
                if (type.IsInterface && type.IsAssignableFrom(typeof(ArraySegment<>).MakeGenericType(argument)))
                    return GetEnumerableConverterMethodInfo;
                if (type.IsInterface && type.IsAssignableFrom(typeof(HashSet<>).MakeGenericType(argument)))
                    return GetSetConverterMethodInfo;
                else
                    return GetEnumerableConverterMethodInfo;
            }

            if (CommonHelper.TryGetInterfaceArguments(type, typeof(IEnumerable<>), out var arguments) is false)
                return null;
            if (type.IsGenericType && InvalidTypes.Contains(type.GetGenericTypeDefinition()))
                throw new ArgumentException($"Invalid collection type: {type}");
            var methodInfo = Method(arguments.Single(), out var itemTypes);
            var converters = itemTypes.Select(context.GetConverter).ToList();
            var method = methodInfo.MakeGenericMethod(CommonHelper.Concat(type, itemTypes));
            var source = Expression.Parameter(typeof(IReadOnlyList<IConverter>), "source");
            var lambda = Expression.Lambda<Func<IReadOnlyList<IConverter>, IConverter>>(Expression.Call(method, source), source);
            return lambda.Compile().Invoke(converters);
        }

        private static MethodInfo GetMethodInfo<T>(Func<IEnumerable<KeyValuePair<Placeholder, Placeholder>>, T> func) where T : IEnumerable<KeyValuePair<Placeholder, Placeholder>>
        {
            return func.Method.GetGenericMethodDefinition();
        }

        private static MethodInfo GetMethodInfo(Func<IReadOnlyList<IConverter>, IConverter> func)
        {
            return func.Method.GetGenericMethodDefinition();
        }

        private static SequenceBuilder<T, R> GetAssignableBuilder<T, R>()
        {
            static Func<R, T> Invoke()
            {
                var source = Expression.Parameter(typeof(R), "source");
                var invoke = Expression.Convert(source, typeof(T));
                var lambda = Expression.Lambda<Func<R, T>>(invoke, source);
                return lambda.Compile();
            }

            if (typeof(T) == typeof(R))
                return (SequenceBuilder<T, R>)(object)new FallbackEnumerableBuilder<T>();
            var constructor = Invoke();
            return new DelegateEnumerableBuilder<T, R>(constructor);
        }

        private static SequenceBuilder<T, R> GetBuilder<T, R, I>()
        {
            static Func<Expression, Expression> Method()
            {
                var type = typeof(T);
                if (type.IsGenericType && CreateMethods.TryGetValue(type.GetGenericTypeDefinition(), out var method))
                    return x => Expression.Call(method.MakeGenericMethod(type.GetGenericArguments()), x);
                if (type.IsAbstract || type.IsInterface)
                    return null;
                var types = new[] { typeof(I) };
                if (type.GetConstructor(types) is { } constructor)
                    return x => Expression.New(constructor, x);
                return null;
            }

            static Func<R, T> Invoke()
            {
                if (Method() is not { } method)
                    return null;
                var source = Expression.Parameter(typeof(R), "source");
                var invoke = method.Invoke(Expression.Convert(source, typeof(I)));
                var lambda = Expression.Lambda<Func<R, T>>(invoke, source);
                return lambda.Compile();
            }

            if (typeof(T).IsAssignableFrom(typeof(R)))
                return GetAssignableBuilder<T, R>();
            var constructor = Invoke();
            return new DelegateEnumerableBuilder<T, R>(constructor);
        }

        private static SequenceCounter<T> GetCounter<T, E>()
        {
            static Type Invoke()
            {
                if (CommonHelper.TryGetInterfaceArguments(typeof(T), typeof(ICollection<>), out _))
                    return typeof(CollectionCounter<,>);
                else if (CommonHelper.TryGetInterfaceArguments(typeof(T), typeof(IReadOnlyCollection<>), out _))
                    return typeof(ReadOnlyCollectionCounter<,>);
                else
                    return null;
            }

            return Invoke() is { } type ? (SequenceCounter<T>)Activator.CreateInstance(type.MakeGenericType(typeof(T), typeof(E))) : null;
        }

        private static IConverter GetSetConverter<T, E>(IReadOnlyList<IConverter> converters) where T : ISet<E>
        {
            var converter = (Converter<E>)converters.Single();
            var adapter = new SetAdapter<T, E>(converter);
            var builder = GetAssignableBuilder<T, HashSet<E>>();
            var counter = typeof(T) == typeof(HashSet<E>) ? (SequenceCounter<T>)(object)new HashSetCounter<E>() : new CollectionCounter<T, E>();
            return new SequenceConverter<T, HashSet<E>>(adapter, builder, counter, converter.Length);
        }

        private static IConverter GetLinkedListConverter<T, E>(IReadOnlyList<IConverter> converters)
        {
            Debug.Assert(typeof(T) == typeof(LinkedList<E>));
            var converter = (Converter<E>)converters.Single();
            var adapter = new LinkedListAdapter<E>(converter);
            var builder = new FallbackEnumerableBuilder<LinkedList<E>>();
            var counter = new LinkedListCounter<E>();
            return new SequenceConverter<LinkedList<E>, LinkedList<E>>(adapter, builder, counter, converter.Length);
        }

        private static IConverter GetEnumerableConverter<T, E>(IReadOnlyList<IConverter> converters) where T : IEnumerable<E>
        {
            var converter = (Converter<E>)converters.Single();
            var builder = GetBuilder<T, ArraySegment<E>, IEnumerable<E>>();
            var adapter = new EnumerableAdapter<T, E>(converter);
            var counter = GetCounter<T, E>();
            return new SequenceConverter<T, ArraySegment<E>>(adapter, builder, counter, converter.Length);
        }

        private static IConverter GetDictionaryConverter<T, K, V>(IReadOnlyList<IConverter> converters) where T : IEnumerable<KeyValuePair<K, V>>
        {
            var itemLength = ContextMethods.GetItemLength(converters);
            var adapter = new DictionaryAdapter<T, K, V>((Converter<K>)converters[0], (Converter<V>)converters[1], itemLength);
            var builder = GetBuilder<T, Dictionary<K, V>, IDictionary<K, V>>();
            var counter = typeof(T) == typeof(Dictionary<K, V>) ? (SequenceCounter<T>)(object)new DictionaryCounter<K, V>() : GetCounter<T, KeyValuePair<K, V>>();
            return new SequenceConverter<T, Dictionary<K, V>>(adapter, builder, counter, itemLength);
        }
    }
}
