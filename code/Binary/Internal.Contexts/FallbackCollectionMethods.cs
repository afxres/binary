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

        private static readonly IReadOnlyList<Type> HashSetAssignableDefinitions;

        private static readonly IReadOnlyList<Type> DictionaryAssignableDefinitions;

        private static readonly IReadOnlyList<Type> ArrayOrArraySegmentAssignableDefinitions;

        private static readonly IReadOnlyDictionary<Type, MethodInfo> ImmutableCollectionCreateMethods;

        static FallbackCollectionMethods()
        {
            static MethodInfo Info<T>(Func<IEnumerable<KeyValuePair<object, object>>, T> func)
            {
                return func.Method.GetGenericMethodDefinition();
            }

            static IEnumerable<Type> List<T>()
            {
                var enumerable = new[] { typeof(IEnumerable<object>), typeof(IEnumerable<KeyValuePair<object, object>>) };
                var types = CommonHelper.Concat(typeof(T), typeof(T).GetInterfaces());
                var generic = types.Where(x => x.IsGenericType);
                var assignable = generic.Where(x => enumerable.Any(t => t.IsAssignableFrom(x)));
                var definitions = assignable.Select(x => x.GetGenericTypeDefinition());
                return definitions;
            }

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

            var array = List<object[]>().Intersect(List<List<object>>()).ToArray();
            var set = List<HashSet<object>>().Except(array).ToArray();
            var dictionary = List<Dictionary<object, object>>().Except(array).ToArray();

            InvalidTypeDefinitions = invalid;
            HashSetAssignableDefinitions = set;
            DictionaryAssignableDefinitions = dictionary;
            ArrayOrArraySegmentAssignableDefinitions = array;
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
            var source = Expression.Parameter(typeof(ReadOnlySpan<byte>), "source");
            var invoke = method.Invoke(Expression.Convert(Expression.Call(Expression.Constant(decoder.Target), decoder.Method, source), typeof(I)));
            var lambda = Expression.Lambda<SequenceDecoder<T>>(invoke, source);
            return lambda.Compile();
        }

        private static SequenceDecoder<T> GetDecoder<T, E>(Converter<E> converter, Func<Expression, Expression> method) where T : IEnumerable<E>
        {
            return GetDecoder<T, IEnumerable<E>, IEnumerable<E>>(new EnumerableDecoder<IEnumerable<E>, E>(converter).Decode, method);
        }

        private static SequenceDecoder<T> GetDecoder<T, K, V>(Converter<K> init, Converter<V> tail, int itemLength, Func<Expression, Expression> method) where T : IEnumerable<KeyValuePair<K, V>>
        {
            return GetDecoder<T, IEnumerable<KeyValuePair<K, V>>, IEnumerable<KeyValuePair<K, V>>>(new KeyValueEnumerableDecoder<K, V>(init, tail, itemLength).Decode, method);
        }

        private static SequenceDecoder<T> GetDecoder<T, E>(Converter<E> converter) where T : IEnumerable<E>
        {
            if (CommonHelper.SelectGenericTypeDefinitionOrDefault(typeof(T), ArrayOrArraySegmentAssignableDefinitions.Contains))
                return new EnumerableDecoder<T, E>(converter).Decode;
            if (CommonHelper.SelectGenericTypeDefinitionOrDefault(typeof(T), HashSetAssignableDefinitions.Contains))
                return GetDecoder<T, HashSet<E>>(new HashSetDecoder<E>(converter).Decode);
            if (CommonHelper.SelectGenericTypeDefinitionOrDefault(typeof(T), ImmutableCollectionCreateMethods.GetValueOrDefault) is { } result)
                return GetDecoder<T, E>(converter, x => Expression.Call(result.MakeGenericMethod(typeof(E)), x));
            if (GetConstructorOrDefault(typeof(T), typeof(IEnumerable<E>)) is { } method)
                return GetDecoder<T, E>(converter, method);
            else
                return null;
        }

        private static SequenceDecoder<T> GetDecoder<T, K, V>(Converter<K> init, Converter<V> tail) where T : IEnumerable<KeyValuePair<K, V>>
        {
            var itemLength = ContextMethods.GetItemLength(new IConverter[] { init, tail });
            if (CommonHelper.SelectGenericTypeDefinitionOrDefault(typeof(T), DictionaryAssignableDefinitions.Contains))
                return GetDecoder<T, Dictionary<K, V>>(new DictionaryDecoder<K, V>(init, tail, itemLength).Decode);
            if (CommonHelper.SelectGenericTypeDefinitionOrDefault(typeof(T), ImmutableCollectionCreateMethods.GetValueOrDefault) is { } result)
                return GetDecoder<T, K, V>(init, tail, itemLength, x => Expression.Call(result.MakeGenericMethod(typeof(K), typeof(V)), x));
            if (GetConstructorOrDefault(typeof(T), typeof(IDictionary<K, V>)) is { } target)
                return GetDecoder<T, Dictionary<K, V>, IDictionary<K, V>>(new DictionaryDecoder<K, V>(init, tail, itemLength).Decode, target);
            if (GetConstructorOrDefault(typeof(T), typeof(IEnumerable<KeyValuePair<K, V>>)) is { } method)
                return GetDecoder<T, K, V>(init, tail, itemLength, method);
            else
                return null;
        }

        private static SequenceEncoder<T> GetEncoder<T, E>(Converter<E> converter) where T : IEnumerable<E>
        {
            Func<Expression, Expression, Expression> Invoke()
            {
                var member = Expression.Constant(converter);
                var method = ContextMethods.GetEncodeMethodInfo(typeof(E), nameof(IConverter.EncodeAuto));
                var invoke = new Func<Expression, Expression, Expression>((allocator, current) => Expression.Call(member, method, allocator, current));
                return invoke;
            }

            var handle = new Lazy<Func<Expression, Expression, Expression>>(Invoke);
            var result = GetEncoder<T>(typeof(E), handle);
            return result ?? new EnumerableEncoder<T, E>(converter).Encode;
        }

        private static SequenceEncoder<T> GetEncoder<T, K, V>(Converter<K> init, Converter<V> tail) where T : IEnumerable<KeyValuePair<K, V>>
        {
            Func<Expression, Expression, Expression> Invoke()
            {
                var initMember = Expression.Constant(init);
                var tailMember = Expression.Constant(tail);
                var initMethod = ContextMethods.GetEncodeMethodInfo(typeof(K), nameof(IConverter.EncodeAuto));
                var tailMethod = ContextMethods.GetEncodeMethodInfo(typeof(V), nameof(IConverter.EncodeAuto));
                var initProperty = CommonHelper.GetProperty<KeyValuePair<K, V>, K>(x => x.Key);
                var tailProperty = CommonHelper.GetProperty<KeyValuePair<K, V>, V>(x => x.Value);
                var assign = Expression.Variable(typeof(KeyValuePair<K, V>), "current");
                var invoke = new Func<Expression, Expression, Expression>((allocator, current) => Expression.Block(
                    new[] { assign },
                    Expression.Assign(assign, current),
                    Expression.Call(initMember, initMethod, allocator, Expression.Property(assign, initProperty)),
                    Expression.Call(tailMember, tailMethod, allocator, Expression.Property(assign, tailProperty))));
                return invoke;
            }

            var handle = new Lazy<Func<Expression, Expression, Expression>>(Invoke);
            var result = GetEncoder<T>(typeof(KeyValuePair<K, V>), handle);
            return result ?? new KeyValueEnumerableEncoder<T, K, V>(init, tail).Encode;
        }

        private static SequenceEncoder<T> GetEncoder<T>(Type elementType, Lazy<Func<Expression, Expression, Expression>> handle)
        {
            const BindingFlags Select = BindingFlags.Instance | BindingFlags.Public;
            var initial = typeof(T).GetMethods(Select).FirstOrDefault(x => x.Name is "GetEnumerator" && x.GetParameters().Length is 0);
            if (initial is null)
                return null;
            var enumeratorType = initial.ReturnType;
            if (enumeratorType.IsValueType is false)
                return null;
            var dispose = enumeratorType.GetMethods(Select).FirstOrDefault(x => x.Name is "Dispose" && x.GetParameters().Length is 0 && x.ReturnType == typeof(void));
            var functor = enumeratorType.GetMethods(Select).FirstOrDefault(x => x.Name is "MoveNext" && x.GetParameters().Length is 0 && x.ReturnType == typeof(bool));
            var current = enumeratorType.GetProperties(Select).FirstOrDefault(x => x.Name is "Current" && x.GetGetMethod() is { } method && method.GetParameters().Length is 0 && x.PropertyType == elementType);
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
                    handle.Value.Invoke(allocator, Expression.Property(enumerator, current)),
                    Expression.Break(target)),
                target);
            var source = dispose is null
                ? origin as Expression
                : Expression.TryFinally(origin, Expression.Call(enumerator, dispose));
            var result = Expression.Block(new[] { enumerator }, assign, source);
            var ensure = typeof(T).IsValueType
                ? result as Expression
                : Expression.IfThen(Expression.NotEqual(collection, Expression.Constant(null, typeof(T))), result);
            var lambda = Expression.Lambda<SequenceEncoder<T>>(ensure, allocator, collection);
            return lambda.Compile();
        }

        private static IConverter GetConverter<T, E>(IGeneratorContext context) where T : IEnumerable<E>
        {
            var converter = (Converter<E>)context.GetConverter(typeof(E));
            var encoder = GetEncoder<T, E>(converter);
            var decoder = GetDecoder<T, E>(converter);
            return new SequenceConverter<T>(encoder, decoder);
        }

        private static IConverter GetConverter<T, K, V>(IGeneratorContext context) where T : IEnumerable<KeyValuePair<K, V>>
        {
            var init = (Converter<K>)context.GetConverter(typeof(K));
            var tail = (Converter<V>)context.GetConverter(typeof(V));
            var encoder = GetEncoder<T, K, V>(init, tail);
            var decoder = GetDecoder<T, K, V>(init, tail);
            return new SequenceConverter<T>(encoder, decoder);
        }
    }
}
