using Mikodev.Binary.Internal.Sequence;
using Mikodev.Binary.Internal.Sequence.Counters;
using Mikodev.Binary.Internal.Sequence.Decoders;
using Mikodev.Binary.Internal.Sequence.Encoders;
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
            [typeof(IImmutableDictionary<,>)] = GetMethod(ImmutableDictionary.CreateRange),
            [typeof(IImmutableList<>)] = GetMethod(ImmutableList.CreateRange),
            [typeof(IImmutableQueue<>)] = GetMethod(ImmutableQueue.CreateRange),
            [typeof(IImmutableSet<>)] = GetMethod(ImmutableHashSet.CreateRange),
            [typeof(ImmutableArray<>)] = GetMethod(ImmutableArray.CreateRange),
            [typeof(ImmutableDictionary<,>)] = GetMethod(ImmutableDictionary.CreateRange),
            [typeof(ImmutableHashSet<>)] = GetMethod(ImmutableHashSet.CreateRange),
            [typeof(ImmutableList<>)] = GetMethod(ImmutableList.CreateRange),
            [typeof(ImmutableQueue<>)] = GetMethod(ImmutableQueue.CreateRange),
            [typeof(ImmutableSortedDictionary<,>)] = GetMethod(ImmutableSortedDictionary.CreateRange),
            [typeof(ImmutableSortedSet<>)] = GetMethod(ImmutableSortedSet.CreateRange),
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

        private static IConverter GetConverter(IGeneratorContext context, Func<IGeneratorContext, IConverter> func, params Type[] types)
        {
            var source = Expression.Parameter(typeof(IGeneratorContext), "context");
            var method = func.Method.GetGenericMethodDefinition().MakeGenericMethod(types);
            var lambda = Expression.Lambda<Func<IGeneratorContext, IConverter>>(Expression.Call(method, source), source);
            return lambda.Compile().Invoke(context);
        }

        private static MethodInfo GetMethod<T>(Func<IEnumerable<KeyValuePair<It, It>>, T> func) where T : IEnumerable<KeyValuePair<It, It>>
        {
            return func.Method.GetGenericMethodDefinition();
        }

        private static Func<Expression, Expression> GetConstructorOrDefault(Type type, Type enumerable)
        {
            if (type.IsAbstract || type.IsInterface)
                return null;
            var constructor = type.GetConstructors().FirstOrDefault(x => x.GetParameters() is { Length: 1 } data && data.Single().ParameterType == enumerable);
            if (constructor is null)
                return null;
            return x => Expression.New(constructor, x);
        }

        private static SequenceDecoder<T> GetDecoder<T, R, I>(SequenceDecoder<R> decoder, Func<Expression, Expression> method)
        {
            static Func<R, T> Invoke(Func<Expression, Expression> method)
            {
                var source = Expression.Parameter(typeof(R), "source");
                var invoke = method.Invoke(Expression.Convert(source, typeof(I)));
                var lambda = Expression.Lambda<Func<R, T>>(invoke, source);
                return lambda.Compile();
            }

            return method is null ? new FallbackDecoder<T>() : new DelegateDecoder<T, R>(decoder, Invoke(method));
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

        private static SequenceEncoder<T> GetEncoder<T, E>(Converter<E> converter) where T : IEnumerable<E>
        {
            var member = Expression.Constant(converter);
            var method = ContextMethods.GetEncodeMethodInfo(typeof(E), auto: true);
            var result = GetEncoder<T>(typeof(E), (allocator, current) => Expression.Call(member, method, allocator, current));
            return result is null ? new EnumerableEncoder<T, E>(converter) : new DelegateEncoder<T>(result);
        }

        private static SequenceEncoder<T> GetEncoder<T, K, V>(Converter<K> init, Converter<V> tail) where T : IEnumerable<KeyValuePair<K, V>>
        {
            var initMember = Expression.Constant(init);
            var tailMember = Expression.Constant(tail);
            var initMethod = ContextMethods.GetEncodeMethodInfo(typeof(K), auto: true);
            var tailMethod = ContextMethods.GetEncodeMethodInfo(typeof(V), auto: true);
            var initProperty = typeof(KeyValuePair<K, V>).GetProperty(nameof(KeyValuePair<K, V>.Key));
            var tailProperty = typeof(KeyValuePair<K, V>).GetProperty(nameof(KeyValuePair<K, V>.Value));
            var assign = Expression.Variable(typeof(KeyValuePair<K, V>), "current");
            var result = GetEncoder<T>(typeof(KeyValuePair<K, V>), (allocator, current) => Expression.Block(
                new[] { assign },
                Expression.Assign(assign, current),
                Expression.Call(initMember, initMethod, allocator, Expression.Property(assign, initProperty)),
                Expression.Call(tailMember, tailMethod, allocator, Expression.Property(assign, tailProperty))));
            return result is null ? new KeyValueEnumerableEncoder<T, K, V>(init, tail) : new DelegateEncoder<T>(result);
        }

        private static ContextCollectionEncoder<T> GetEncoder<T>(Type elementType, Func<Expression, Expression, Expression> func)
        {
            const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public;
            var initial = typeof(T).GetMethods(Flags).FirstOrDefault(x => x.Name is "GetEnumerator" && x.GetParameters().Length is 0);
            if (initial is null)
                return null;
            var enumeratorType = initial.ReturnType;
            if (enumeratorType.IsValueType is false)
                return null;
            var dispose = enumeratorType.GetMethods(Flags).FirstOrDefault(x => x.Name is "Dispose" && x.GetParameters().Length is 0 && x.ReturnType == typeof(void));
            var functor = enumeratorType.GetMethods(Flags).FirstOrDefault(x => x.Name is "MoveNext" && x.GetParameters().Length is 0 && x.ReturnType == typeof(bool));
            var current = enumeratorType.GetProperties(Flags).FirstOrDefault(x => x.Name is "Current" && x.GetGetMethod() is { } method && method.GetParameters().Length is 0 && x.PropertyType == elementType);
            if (functor is null || current is null)
                return null;

            var allocator = Expression.Parameter(typeof(Allocator).MakeByRefType(), "allocator");
            var collection = Expression.Parameter(typeof(T), "collection");
            var enumerator = Expression.Variable(enumeratorType, "enumerator");
            var assign = Expression.Assign(enumerator, Expression.Call(collection, initial));
            var target = Expression.Label("target");
            var origin = Expression.Loop(
                Expression.IfThenElse(
                    Expression.Call(enumerator, functor),
                    func.Invoke(allocator, Expression.Property(enumerator, current)),
                    Expression.Break(target)),
                target);
            var source = dispose is null
                ? origin as Expression
                : Expression.TryFinally(origin, Expression.Call(enumerator, dispose));
            var result = Expression.Block(new[] { enumerator }, assign, source);
            var lambda = Expression.Lambda<ContextCollectionEncoder<T>>(result, allocator, collection);
            return lambda.Compile();
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
            return GetEnumerableConverter<T, E>(converter, GetConstructorOrDefault(typeof(T), typeof(IEnumerable<E>)));
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
            if (GetConstructorOrDefault(typeof(T), typeof(IDictionary<K, V>)) is { } result)
                return GetDictionaryConverter<T, K, V>(init, tail, itemLength, result);
            return GetKeyValueEnumerableConverter<T, K, V>(init, tail, itemLength, GetConstructorOrDefault(typeof(T), typeof(IEnumerable<KeyValuePair<K, V>>)));
        }

        private static IConverter GetHashSetConverter<E>(Converter<E> converter)
        {
            var encoder = GetEncoder<HashSet<E>, E>(converter);
            var decoder = new HashSetDecoder<E>(converter);
            var counter = new HashSetCounter<E>();
            return new SequenceConverter<HashSet<E>>(encoder, decoder, counter, converter.Length);
        }

        private static IConverter GetHashSetInterfaceAssignableConverter<T, E>(Converter<E> converter) where T : IEnumerable<E>
        {
            var encoder = GetEncoder<T, E>(converter);
            var decoder = new AssignableDecoder<T, HashSet<E>>(new HashSetDecoder<E>(converter));
            var counter = GetCounter<T, E>();
            return new SequenceConverter<T>(encoder, decoder, counter, converter.Length);
        }

        private static IConverter GetLinkedListConverter<E>(Converter<E> converter)
        {
            var encoder = new LinkedListEncoder<E>(converter);
            var decoder = new LinkedListDecoder<E>(converter);
            var counter = new LinkedListCounter<E>();
            return new SequenceConverter<LinkedList<E>>(encoder, decoder, counter, converter.Length);
        }

        private static IConverter GetEnumerableConverter<T, E>(Converter<E> converter, Func<Expression, Expression> method) where T : IEnumerable<E>
        {
            var encoder = GetEncoder<T, E>(converter);
            var decoder = GetDecoder<T, ArraySegment<E>, IEnumerable<E>>(new ArraySegmentDecoder<E>(converter), method);
            var counter = GetCounter<T, E>();
            return new SequenceConverter<T>(encoder, decoder, counter, converter.Length);
        }

        private static IConverter GetEnumerableInterfaceAssignableConverter<T, E>(Converter<E> converter) where T : IEnumerable<E>
        {
            var encoder = GetEncoder<T, E>(converter);
            var decoder = new AssignableDecoder<T, ArraySegment<E>>(new ArraySegmentDecoder<E>(converter));
            var counter = GetCounter<T, E>();
            return new SequenceConverter<T>(encoder, decoder, counter, converter.Length);
        }

        private static IConverter GetDictionaryConverter<K, V>(Converter<K> init, Converter<V> tail, int itemLength)
        {
            var encoder = GetEncoder<Dictionary<K, V>, K, V>(init, tail);
            var decoder = new DictionaryDecoder<K, V>(init, tail, itemLength);
            var counter = new DictionaryCounter<K, V>();
            return new SequenceConverter<Dictionary<K, V>>(encoder, decoder, counter, itemLength);
        }

        private static IConverter GetDictionaryConverter<T, K, V>(Converter<K> init, Converter<V> tail, int itemLength, Func<Expression, Expression> method) where T : IEnumerable<KeyValuePair<K, V>>
        {
            var encoder = GetEncoder<T, K, V>(init, tail);
            var decoder = GetDecoder<T, Dictionary<K, V>, IDictionary<K, V>>(new DictionaryDecoder<K, V>(init, tail, itemLength), method);
            var counter = GetCounter<T, KeyValuePair<K, V>>();
            return new SequenceConverter<T>(encoder, decoder, counter, itemLength);
        }

        private static IConverter GetDictionaryInterfaceAssignableConverter<T, K, V>(Converter<K> init, Converter<V> tail, int itemLength) where T : IEnumerable<KeyValuePair<K, V>>
        {
            var encoder = GetEncoder<T, K, V>(init, tail);
            var decoder = new AssignableDecoder<T, Dictionary<K, V>>(new DictionaryDecoder<K, V>(init, tail, itemLength));
            var counter = GetCounter<T, KeyValuePair<K, V>>();
            return new SequenceConverter<T>(encoder, decoder, counter, itemLength);
        }

        private static IConverter GetKeyValueEnumerableConverter<T, K, V>(Converter<K> init, Converter<V> tail, int itemLength, Func<Expression, Expression> method) where T : IEnumerable<KeyValuePair<K, V>>
        {
            var encoder = GetEncoder<T, K, V>(init, tail);
            var decoder = GetDecoder<T, IEnumerable<KeyValuePair<K, V>>, IEnumerable<KeyValuePair<K, V>>>(new KeyValueEnumerableDecoder<K, V>(init, tail, itemLength), method);
            var counter = GetCounter<T, KeyValuePair<K, V>>();
            return new SequenceConverter<T>(encoder, decoder, counter, itemLength);
        }
    }
}
