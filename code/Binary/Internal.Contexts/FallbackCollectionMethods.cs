﻿using Mikodev.Binary.Internal.Metadata;
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
        private static readonly ImmutableArray<Type> InvalidTypeDefinitions;

        private static readonly ImmutableArray<Type> HashSetAssignableDefinitions;

        private static readonly ImmutableArray<Type> DictionaryAssignableDefinitions;

        private static readonly ImmutableArray<Type> ArrayOrListAssignableDefinitions;

        private static readonly ImmutableDictionary<Type, MethodInfo> ImmutableCollectionCreateMethods;

        static FallbackCollectionMethods()
        {
            static MethodInfo Info<T>(Func<IEnumerable<KeyValuePair<object, object>>, T> func)
            {
                return func.Method.GetGenericMethodDefinition();
            }

            static IEnumerable<Type> Dump<T>()
            {
                var enumerable = new[] { typeof(IEnumerable<object>), typeof(IEnumerable<KeyValuePair<object, object>>) };
                var types = ImmutableArray.Create(typeof(T)).AddRange(typeof(T).GetInterfaces());
                var generic = types.Where(x => x.IsGenericType);
                var assignable = generic.Where(x => enumerable.Any(t => t.IsAssignableFrom(x)));
                var definitions = assignable.Select(x => x.GetGenericTypeDefinition());
                return definitions;
            }

            var immutable = ImmutableDictionary.CreateRange(new Dictionary<Type, MethodInfo>
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
            });

            var invalid = ImmutableArray.Create(new[]
            {
                typeof(Stack<>),
                typeof(ConcurrentStack<>),
                typeof(ImmutableStack<>),
                typeof(IImmutableStack<>),
            });

            var array = Dump<object[]>().Intersect(Dump<List<object>>()).ToImmutableArray();
            var set = Dump<HashSet<object>>().Except(array).ToImmutableArray();
            var dictionary = Dump<Dictionary<object, object>>().Except(array).ToImmutableArray();

            InvalidTypeDefinitions = invalid;
            HashSetAssignableDefinitions = set;
            DictionaryAssignableDefinitions = dictionary;
            ArrayOrListAssignableDefinitions = array;
            ImmutableCollectionCreateMethods = immutable;
        }

        internal static IConverter GetConverter(IGeneratorContext context, Type type)
        {
            if (CommonHelper.TryGetInterfaceArguments(type, typeof(IEnumerable<>), out var arguments) is false)
                return null;
            if (CommonHelper.SelectGenericTypeDefinitionOrDefault(type, InvalidTypeDefinitions.Contains))
                throw new ArgumentException($"Invalid collection type: {type}");
            if (CommonHelper.TryGetInterfaceArguments(type, typeof(IDictionary<,>), out var types) || CommonHelper.TryGetInterfaceArguments(type, typeof(IReadOnlyDictionary<,>), out types))
                return GetConverter(context, GetConverter<IEnumerable<KeyValuePair<object, object>>, object, object>, ImmutableArray.Create(type).AddRange(types));
            else
                return GetConverter(context, GetConverter<IEnumerable<object>, object>, ImmutableArray.Create(type).AddRange(arguments));
        }

        private static IConverter GetConverter(IGeneratorContext context, Func<IGeneratorContext, IConverter> func, ImmutableArray<Type> types)
        {
            var source = Expression.Parameter(typeof(IGeneratorContext), "context");
            var method = func.Method.GetGenericMethodDefinition().MakeGenericMethod(types.ToArray());
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

        private static DecodePassSpanDelegate<T> GetDecodeDelegate<T, R>(DecodePassSpanDelegate<R> decode)
        {
            return (DecodePassSpanDelegate<T>)Delegate.CreateDelegate(typeof(DecodePassSpanDelegate<T>), decode.Target, decode.Method);
        }

        private static DecodePassSpanDelegate<T> GetDecodeDelegate<T, R, I>(DecodePassSpanDelegate<R> decode, Func<Expression, Expression> method)
        {
            var source = Expression.Parameter(typeof(ReadOnlySpan<byte>), "source");
            var invoke = method.Invoke(Expression.Convert(Expression.Call(Expression.Constant(decode.Target), decode.Method, source), typeof(I)));
            var lambda = Expression.Lambda<DecodePassSpanDelegate<T>>(invoke, source);
            return lambda.Compile();
        }

        private static DecodePassSpanDelegate<T> GetDecodeDelegate<T, E>(Converter<E> converter, Func<Expression, Expression> method) where T : IEnumerable<E>
        {
            return GetDecodeDelegate<T, IEnumerable<E>, IEnumerable<E>>(new EnumerableDecoder<IEnumerable<E>, E>(converter).Decode, method);
        }

        private static DecodePassSpanDelegate<T> GetDelegateDelegate<T, K, V>(Converter<K> init, Converter<V> tail, int itemLength, Func<Expression, Expression> method) where T : IEnumerable<KeyValuePair<K, V>>
        {
            return GetDecodeDelegate<T, IEnumerable<KeyValuePair<K, V>>, IEnumerable<KeyValuePair<K, V>>>(new KeyValueEnumerableDecoder<K, V>(init, tail, itemLength).Decode, method);
        }

        private static DecodePassSpanDelegate<T> GetDecodeDelegate<T, E>(Converter<E> converter) where T : IEnumerable<E>
        {
            if (CommonHelper.SelectGenericTypeDefinitionOrDefault(typeof(T), ArrayOrListAssignableDefinitions.Contains))
                return new EnumerableDecoder<T, E>(converter).Decode;
            if (CommonHelper.SelectGenericTypeDefinitionOrDefault(typeof(T), HashSetAssignableDefinitions.Contains))
                return GetDecodeDelegate<T, HashSet<E>>(new HashSetDecoder<E>(converter).Decode);
            if (CommonHelper.SelectGenericTypeDefinitionOrDefault(typeof(T), ImmutableCollectionCreateMethods.GetValueOrDefault) is { } result)
                return GetDecodeDelegate<T, E>(converter, x => Expression.Call(result.MakeGenericMethod(typeof(E)), x));
            if (GetConstructorOrDefault(typeof(T), typeof(IEnumerable<E>)) is { } method)
                return GetDecodeDelegate<T, E>(converter, method);
            else
                return null;
        }

        private static DecodePassSpanDelegate<T> GetDecodeDelegate<T, K, V>(Converter<K> init, Converter<V> tail) where T : IEnumerable<KeyValuePair<K, V>>
        {
            var itemLength = ContextMethods.GetItemLength(ImmutableArray.Create(new IConverter[] { init, tail }));
            if (CommonHelper.SelectGenericTypeDefinitionOrDefault(typeof(T), DictionaryAssignableDefinitions.Contains))
                return GetDecodeDelegate<T, Dictionary<K, V>>(new DictionaryDecoder<K, V>(init, tail, itemLength).Decode);
            if (CommonHelper.SelectGenericTypeDefinitionOrDefault(typeof(T), ImmutableCollectionCreateMethods.GetValueOrDefault) is { } result)
                return GetDelegateDelegate<T, K, V>(init, tail, itemLength, x => Expression.Call(result.MakeGenericMethod(typeof(K), typeof(V)), x));
            if (GetConstructorOrDefault(typeof(T), typeof(IDictionary<K, V>)) is { } target)
                return GetDecodeDelegate<T, Dictionary<K, V>, IDictionary<K, V>>(new DictionaryDecoder<K, V>(init, tail, itemLength).Decode, target);
            if (GetConstructorOrDefault(typeof(T), typeof(IEnumerable<KeyValuePair<K, V>>)) is { } method)
                return GetDelegateDelegate<T, K, V>(init, tail, itemLength, method);
            else
                return null;
        }

        private static EncodeDelegate<T> GetEncodeDelegate<T, E>(Converter<E> converter) where T : IEnumerable<E>
        {
            Func<Expression, Expression, Expression> Invoke()
            {
                var member = Expression.Constant(converter);
                var method = ((IConverterMetadata)converter).GetMethod(nameof(IConverter.EncodeAuto));
                var invoke = new Func<Expression, Expression, Expression>((allocator, current) => Expression.Call(member, method, allocator, current));
                return invoke;
            }

            var handle = new Lazy<Func<Expression, Expression, Expression>>(Invoke);
            var result = GetEncodeDelegate<T>(typeof(E), handle);
            return result ?? new EnumerableEncoder<T, E>(converter).Encode;
        }

        private static EncodeDelegate<T> GetEncodeDelegate<T, K, V>(Converter<K> init, Converter<V> tail) where T : IEnumerable<KeyValuePair<K, V>>
        {
            Func<Expression, Expression, Expression> Invoke()
            {
                var initMember = Expression.Constant(init);
                var tailMember = Expression.Constant(tail);
                var initMethod = ((IConverterMetadata)init).GetMethod(nameof(IConverter.EncodeAuto));
                var tailMethod = ((IConverterMetadata)tail).GetMethod(nameof(IConverter.EncodeAuto));
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
            var result = GetEncodeDelegate<T>(typeof(KeyValuePair<K, V>), handle);
            return result ?? new KeyValueEnumerableEncoder<T, K, V>(init, tail).Encode;
        }

        private static EncodeDelegate<T> GetEncodeDelegate<T>(Type elementType, Lazy<Func<Expression, Expression, Expression>> handle)
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
            var lambda = Expression.Lambda<EncodeDelegate<T>>(ensure, allocator, collection);
            return lambda.Compile();
        }

        private static IConverter GetConverter<T, E>(IGeneratorContext context) where T : IEnumerable<E>
        {
            var converter = (Converter<E>)context.GetConverter(typeof(E));
            var encode = GetEncodeDelegate<T, E>(converter);
            var decode = GetDecodeDelegate<T, E>(converter);
            return new SequenceConverter<T>(encode, decode);
        }

        private static IConverter GetConverter<T, K, V>(IGeneratorContext context) where T : IEnumerable<KeyValuePair<K, V>>
        {
            var init = (Converter<K>)context.GetConverter(typeof(K));
            var tail = (Converter<V>)context.GetConverter(typeof(V));
            var encode = GetEncodeDelegate<T, K, V>(init, tail);
            var decode = GetDecodeDelegate<T, K, V>(init, tail);
            return new SequenceConverter<T>(encode, decode);
        }
    }
}
