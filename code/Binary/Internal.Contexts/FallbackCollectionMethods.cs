using Mikodev.Binary.Internal.Sequence;
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
        private static readonly IReadOnlyList<Type> InvalidTypeDefinitions;

        private static readonly IReadOnlyList<Type> EnumerableInterfaceDefinitions;

        private static readonly IReadOnlyDictionary<Type, MethodInfo> ImmutableCollectionCreateMethods;

        static FallbackCollectionMethods()
        {
            static MethodInfo Info<T>(Func<IEnumerable<KeyValuePair<object, object>>, T> func) => func.Method.GetGenericMethodDefinition();

            static List<Type> List<T>() => typeof(T).GetInterfaces().Where(typeof(IEnumerable<object>).IsAssignableFrom).Select(x => x.GetGenericTypeDefinition()).ToList();

            var immutable = new Dictionary<Type, MethodInfo>
            {
                [typeof(IImmutableDictionary<,>)] = Info(ImmutableDictionary.CreateRange),
                [typeof(IImmutableList<>)] = Info(ImmutableList.CreateRange),
                [typeof(IImmutableQueue<>)] = Info(ImmutableQueue.CreateRange),
                [typeof(IImmutableSet<>)] = Info(ImmutableHashSet.CreateRange),
                [typeof(ImmutableArray<>)] = Info(ImmutableArray.CreateRange),
                [typeof(ImmutableDictionary<,>)] = Info(ImmutableDictionary.CreateRange),
                [typeof(ImmutableHashSet<>)] = Info(ImmutableHashSet.CreateRange),
                [typeof(ImmutableList<>)] = Info(ImmutableList.CreateRange),
                [typeof(ImmutableQueue<>)] = Info(ImmutableQueue.CreateRange),
                [typeof(ImmutableSortedDictionary<,>)] = Info(ImmutableSortedDictionary.CreateRange),
                [typeof(ImmutableSortedSet<>)] = Info(ImmutableSortedSet.CreateRange),
            };

            var invalid = new[]
            {
                typeof(Stack<>),
                typeof(ConcurrentStack<>),
                typeof(ImmutableStack<>),
                typeof(IImmutableStack<>),
            };

            var interfaces = List<object[]>().Intersect(List<ArraySegment<object>>()).ToArray();

            InvalidTypeDefinitions = invalid;
            EnumerableInterfaceDefinitions = interfaces;
            ImmutableCollectionCreateMethods = immutable;
        }

        internal static IConverter GetConverter(IGeneratorContext context, Type type)
        {
            if (CommonHelper.TryGetInterfaceArguments(type, typeof(IEnumerable<>), out var arguments) is false)
                return null;
            if (CommonHelper.SelectGenericTypeDefinitionOrDefault(type, InvalidTypeDefinitions.Contains))
                throw new ArgumentException($"Invalid collection type: {type}");
            if (CommonHelper.TryGetInterfaceArguments(type, typeof(IDictionary<,>), out var types) || CommonHelper.TryGetInterfaceArguments(type, typeof(IReadOnlyDictionary<,>), out types))
                return GetConverter(context, GetConverter<IEnumerable<KeyValuePair<object, object>>, object, object>, CommonHelper.Concat(type, types));
            else
                return GetConverter(context, GetConverter<IEnumerable<object>, object>, CommonHelper.Concat(type, arguments));
        }

        private static IConverter GetConverter(IGeneratorContext context, Func<IGeneratorContext, IConverter> func, params Type[] types)
        {
            var source = Expression.Parameter(typeof(IGeneratorContext), "context");
            var method = func.Method.GetGenericMethodDefinition().MakeGenericMethod(types);
            var lambda = Expression.Lambda<Func<IGeneratorContext, IConverter>>(Expression.Call(method, source), source);
            return lambda.Compile().Invoke(context);
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

        private static SequenceDecoder<T> GetDecoder<T, R>(SequenceDecoder<R> decoder)
        {
            return (SequenceDecoder<T>)Delegate.CreateDelegate(typeof(SequenceDecoder<T>), decoder.Target, decoder.Method);
        }

        private static SequenceDecoder<T> GetDecoder<T, R, I>(SequenceDecoder<R> decoder, Func<Expression, Expression> method)
        {
            if (method is null)
                return new SequenceDecoder<T>(ThrowHelper.ThrowNoSuitableConstructor<T>);
            var source = Expression.Parameter(typeof(ReadOnlySpan<byte>), "source");
            var invoke = method.Invoke(Expression.Convert(Expression.Call(Expression.Constant(decoder.Target), decoder.Method, source), typeof(I)));
            var lambda = Expression.Lambda<SequenceDecoder<T>>(invoke, source);
            return lambda.Compile();
        }

        private static SequenceEncoder<T> GetEncoder<T, E>(Converter<E> converter) where T : IEnumerable<E>
        {
            var member = Expression.Constant(converter);
            var method = ContextMethods.GetEncodeMethodInfo(typeof(E), nameof(IConverter.EncodeAuto));
            var result = GetEncoder<T>(typeof(E), (allocator, current) => Expression.Call(member, method, allocator, current));
            return result ?? new EnumerableEncoder<T, E>(converter).Encode;
        }

        private static SequenceEncoder<T> GetEncoder<T, K, V>(Converter<K> init, Converter<V> tail) where T : IEnumerable<KeyValuePair<K, V>>
        {
            var initMember = Expression.Constant(init);
            var tailMember = Expression.Constant(tail);
            var initMethod = ContextMethods.GetEncodeMethodInfo(typeof(K), nameof(IConverter.EncodeAuto));
            var tailMethod = ContextMethods.GetEncodeMethodInfo(typeof(V), nameof(IConverter.EncodeAuto));
            var initProperty = CommonHelper.GetProperty<KeyValuePair<K, V>, K>(x => x.Key);
            var tailProperty = CommonHelper.GetProperty<KeyValuePair<K, V>, V>(x => x.Value);
            var assign = Expression.Variable(typeof(KeyValuePair<K, V>), "current");
            var result = GetEncoder<T>(typeof(KeyValuePair<K, V>), (allocator, current) => Expression.Block(
                new[] { assign },
                Expression.Assign(assign, current),
                Expression.Call(initMember, initMethod, allocator, Expression.Property(assign, initProperty)),
                Expression.Call(tailMember, tailMethod, allocator, Expression.Property(assign, tailProperty))));
            return result ?? new KeyValueEnumerableEncoder<T, K, V>(init, tail).Encode;
        }

        private static SequenceEncoder<T> GetEncoder<T>(Type elementType, Func<Expression, Expression, Expression> func)
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
            var lambda = Expression.Lambda<SequenceEncoder<T>>(result, allocator, collection);
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
            if (typeof(T).IsInterface && CommonHelper.SelectGenericTypeDefinitionOrDefault(typeof(T), EnumerableInterfaceDefinitions.Contains))
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
            var decoder = new SequenceDecoder<HashSet<E>>(new HashSetDecoder<E>(converter).Decode);
            return new SequenceConverter<HashSet<E>>(encoder, decoder);
        }

        private static IConverter GetHashSetInterfaceAssignableConverter<T, E>(Converter<E> converter) where T : IEnumerable<E>
        {
            var encoder = GetEncoder<T, E>(converter);
            var decoder = GetDecoder<T, HashSet<E>>(new HashSetDecoder<E>(converter).Decode);
            return new SequenceConverter<T>(encoder, decoder);
        }

        private static IConverter GetLinkedListConverter<E>(Converter<E> converter)
        {
            var encoder = new SequenceEncoder<LinkedList<E>>(new LinkedListEncoder<E>(converter).Encode);
            var decoder = new SequenceDecoder<LinkedList<E>>(new LinkedListDecoder<E>(converter).Decode);
            return new SequenceConverter<LinkedList<E>>(encoder, decoder);
        }

        private static IConverter GetEnumerableConverter<T, E>(Converter<E> converter, Func<Expression, Expression> method) where T : IEnumerable<E>
        {
            var encoder = GetEncoder<T, E>(converter);
            var decoder = GetDecoder<T, IEnumerable<E>, IEnumerable<E>>(new EnumerableDecoder<IEnumerable<E>, E>(converter).Decode, method);
            return new SequenceConverter<T>(encoder, decoder);
        }

        private static IConverter GetEnumerableInterfaceAssignableConverter<T, E>(Converter<E> converter) where T : IEnumerable<E>
        {
            var encoder = GetEncoder<T, E>(converter);
            var decoder = new SequenceDecoder<T>(new EnumerableDecoder<T, E>(converter).Decode);
            return new SequenceConverter<T>(encoder, decoder);
        }

        private static IConverter GetDictionaryConverter<K, V>(Converter<K> init, Converter<V> tail, int itemLength)
        {
            var encoder = GetEncoder<Dictionary<K, V>, K, V>(init, tail);
            var decoder = new SequenceDecoder<Dictionary<K, V>>(new DictionaryDecoder<K, V>(init, tail, itemLength).Decode);
            return new SequenceConverter<Dictionary<K, V>>(encoder, decoder);
        }

        private static IConverter GetDictionaryConverter<T, K, V>(Converter<K> init, Converter<V> tail, int itemLength, Func<Expression, Expression> method) where T : IEnumerable<KeyValuePair<K, V>>
        {
            var encoder = GetEncoder<T, K, V>(init, tail);
            var decoder = GetDecoder<T, Dictionary<K, V>, IDictionary<K, V>>(new DictionaryDecoder<K, V>(init, tail, itemLength).Decode, method);
            return new SequenceConverter<T>(encoder, decoder);
        }

        private static IConverter GetDictionaryInterfaceAssignableConverter<T, K, V>(Converter<K> init, Converter<V> tail, int itemLength) where T : IEnumerable<KeyValuePair<K, V>>
        {
            var encoder = GetEncoder<T, K, V>(init, tail);
            var decoder = GetDecoder<T, Dictionary<K, V>>(new DictionaryDecoder<K, V>(init, tail, itemLength).Decode);
            return new SequenceConverter<T>(encoder, decoder);
        }

        private static IConverter GetKeyValueEnumerableConverter<T, K, V>(Converter<K> init, Converter<V> tail, int itemLength, Func<Expression, Expression> method) where T : IEnumerable<KeyValuePair<K, V>>
        {
            var encoder = GetEncoder<T, K, V>(init, tail);
            var decoder = GetDecoder<T, IEnumerable<KeyValuePair<K, V>>, IEnumerable<KeyValuePair<K, V>>>(new KeyValueEnumerableDecoder<K, V>(init, tail, itemLength).Decode, method);
            return new SequenceConverter<T>(encoder, decoder);
        }
    }
}
