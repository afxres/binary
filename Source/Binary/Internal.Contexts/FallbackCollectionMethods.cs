using Mikodev.Binary.Internal.Contexts.Instance;
using Mikodev.Binary.Internal.Sequence;
using Mikodev.Binary.Internal.Sequence.Adapters;
using Mikodev.Binary.Internal.Sequence.Counters;
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

        internal static IConverter GetConverter(IGeneratorContext context, Type type)
        {
            MethodInfo Method(Type argument, out Type[] arguments)
            {
                if (CommonHelper.TryGetInterfaceArguments(type, typeof(IDictionary<,>), out arguments) || CommonHelper.TryGetInterfaceArguments(type, typeof(IReadOnlyDictionary<,>), out arguments))
                    return CreateDictionaryConverterMethodInfo;
                arguments = new[] { argument };
                if (typeof(ISet<>).MakeGenericType(argument).IsAssignableFrom(type))
                    return CreateSetConverterMethodInfo;
                else if (type == typeof(LinkedList<>).MakeGenericType(argument))
                    return CreateLinkedListConverterMethodInfo;
                else
                    return CreateEnumerableConverterMethodInfo;
            }

            if (CommonHelper.TryGetInterfaceArguments(type, typeof(IEnumerable<>), out var arguments) is false)
                return null;
            if (CommonHelper.IsImplementationOf(type, typeof(Stack<>)) || CommonHelper.IsImplementationOf(type, typeof(ConcurrentStack<>)))
                throw new ArgumentException($"Invalid collection type: {type}");
            var methodInfo = Method(arguments.Single(), out var itemTypes);
            var converters = itemTypes.Select(context.GetConverter).ToArray();
            var method = methodInfo.MakeGenericMethod(CommonHelper.Concat(type, itemTypes));
            var source = Expression.Parameter(typeof(IReadOnlyList<IConverter>), "source");
            var lambda = Expression.Lambda<Func<IReadOnlyList<IConverter>, IConverter>>(Expression.Call(method, source), source);
            return lambda.Compile().Invoke(converters);
        }

        private static SequenceBuilder<T, R> CreateCollectionBuilder<T, R>()
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

        private static SequenceBuilder<T, R> CreateCollectionBuilder<T, R, I>()
        {
            static Func<R, T> Invoke()
            {
                var type = typeof(T);
                var enumerableType = typeof(I);
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

            if (typeof(T).IsAssignableFrom(typeof(R)))
                return CreateCollectionBuilder<T, R>();
            var constructor = Invoke();
            return new DelegateEnumerableBuilder<T, R>(constructor);
        }

        private static SequenceCounter<T> CreateCollectionCounter<T, E>()
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

        private static IConverter CreateSetConverter<T, E>(IReadOnlyList<IConverter> converters) where T : ISet<E>
        {
            var converter = (Converter<E>)converters.Single();
            var adapter = new SetAdapter<T, E>(converter);
            var builder = CreateCollectionBuilder<T, HashSet<E>>();
            var counter = typeof(T) == typeof(HashSet<E>) ? (SequenceCounter<T>)(object)new HashSetCounter<E>() : new CollectionCounter<T, E>();
            return new SequenceConverter<T, HashSet<E>>(adapter, builder, counter, converter.Length);
        }

        private static IConverter CreateLinkedListConverter<T, E>(IReadOnlyList<IConverter> converters)
        {
            Debug.Assert(typeof(T) == typeof(LinkedList<E>));
            var converter = (Converter<E>)converters.Single();
            var adapter = new LinkedListAdapter<E>(converter);
            var builder = new FallbackEnumerableBuilder<LinkedList<E>>();
            var counter = new LinkedListCounter<E>();
            return new SequenceConverter<LinkedList<E>, LinkedList<E>>(adapter, builder, counter, converter.Length);
        }

        private static IConverter CreateEnumerableConverter<T, E>(IReadOnlyList<IConverter> converters) where T : IEnumerable<E>
        {
            var converter = (Converter<E>)converters.Single();
            var builder = CreateCollectionBuilder<T, ArraySegment<E>, IEnumerable<E>>();
            var adapter = new EnumerableAdapter<T, E>(converter);
            var counter = CreateCollectionCounter<T, E>();
            return new SequenceConverter<T, ArraySegment<E>>(adapter, builder, counter, converter.Length);
        }

        private static IConverter CreateDictionaryConverter<T, K, V>(IReadOnlyList<IConverter> converters) where T : IEnumerable<KeyValuePair<K, V>>
        {
            var itemLength = ContextMethods.GetItemLength(converters);
            var adapter = new DictionaryAdapter<T, K, V>((Converter<K>)converters[0], (Converter<V>)converters[1], itemLength);
            var builder = CreateCollectionBuilder<T, Dictionary<K, V>, IDictionary<K, V>>();
            var counter = typeof(T) == typeof(Dictionary<K, V>) ? (SequenceCounter<T>)(object)new DictionaryCounter<K, V>() : CreateCollectionCounter<T, KeyValuePair<K, V>>();
            return new SequenceConverter<T, Dictionary<K, V>>(adapter, builder, counter, itemLength);
        }
    }
}
