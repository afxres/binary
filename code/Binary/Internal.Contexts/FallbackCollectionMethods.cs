namespace Mikodev.Binary.Internal.Contexts;

using Mikodev.Binary.Internal.Metadata;
using Mikodev.Binary.Internal.Sequence;
using Mikodev.Binary.Internal.Sequence.Decoders;
using Mikodev.Binary.Internal.Sequence.Encoders;
using Mikodev.Binary.Internal.SpanLike.Contexts;
using Mikodev.Binary.Internal.SpanLike.Decoders;
using System;
using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

internal static class FallbackCollectionMethods
{
    private static readonly ImmutableArray<Type> InvalidTypeDefinitions;

    private static readonly ImmutableArray<Type> HashSetAssignableDefinitions;

    private static readonly ImmutableArray<Type> DictionaryAssignableDefinitions;

    private static readonly ImmutableArray<Type> ArrayOrListAssignableDefinitions;

    private static readonly FrozenDictionary<Type, MethodInfo> ImmutableCollectionCreateMethods;

    static FallbackCollectionMethods()
    {
        static MethodInfo Info<T>(Func<IEnumerable<KeyValuePair<object, object>>, T> func)
        {
            return func.Method.GetGenericMethodDefinition();
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
            [typeof(FrozenDictionary<,>)] = Info(SequenceMethods.GetFrozenDictionary),
            [typeof(FrozenSet<>)] = Info(SequenceMethods.GetFrozenSet),
        };

        var invalid = ImmutableArray.Create(
        [
            typeof(Stack<>),
            typeof(ConcurrentStack<>),
            typeof(ImmutableStack<>),
            typeof(IImmutableStack<>),
        ]);

        var array = ImmutableArray.Create(
        [
            typeof(IList<>),
            typeof(ICollection<>),
            typeof(IEnumerable<>),
            typeof(IReadOnlyList<>),
            typeof(IReadOnlyCollection<>),
        ]);

        var set = ImmutableArray.Create(
        [
            typeof(HashSet<>),
            typeof(ISet<>),
            typeof(IReadOnlySet<>),
        ]);

        var dictionary = ImmutableArray.Create(
        [
            typeof(Dictionary<,>),
            typeof(IDictionary<,>),
            typeof(IReadOnlyDictionary<,>),
        ]);

        InvalidTypeDefinitions = invalid;
        HashSetAssignableDefinitions = set;
        DictionaryAssignableDefinitions = dictionary;
        ArrayOrListAssignableDefinitions = array;
        ImmutableCollectionCreateMethods = immutable.ToFrozenDictionary();
    }

    [RequiresUnreferencedCode(CommonModule.RequiresUnreferencedCodeMessage)]
    internal static IConverter? GetConverter(IGeneratorContext context, Type type)
    {
        if (CommonModule.TryGetInterfaceArguments(type, typeof(IEnumerable<>), out var arguments) is false)
            return null;
        if (CommonModule.SelectGenericTypeDefinitionOrDefault(type, InvalidTypeDefinitions.Contains))
            throw new ArgumentException($"Invalid collection type: {type}");
        if (CommonModule.TryGetInterfaceArguments(type, typeof(IDictionary<,>), out var types) || CommonModule.TryGetInterfaceArguments(type, typeof(IReadOnlyDictionary<,>), out types))
            return GetConverter(context, GetConverter<IEnumerable<KeyValuePair<object, object>>, object, object>, [type, .. types]);
        else
            return GetConverter(context, GetConverter<IEnumerable<object>, object>, [type, .. arguments]);
    }

    [RequiresUnreferencedCode(CommonModule.RequiresUnreferencedCodeMessage)]
    private static IConverter GetConverter(IGeneratorContext context, Func<IGeneratorContext, IConverter> func, ImmutableArray<Type> types)
    {
        var method = func.Method.GetGenericMethodDefinition().MakeGenericMethod([.. types]);
        var target = CommonModule.CreateDelegate<Func<IGeneratorContext, IConverter>>(null, method);
        return target.Invoke(context);
    }

    private static Func<Expression, Expression>? GetConstructorOrDefault([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type type, Type enumerable)
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
        return CommonModule.CreateDelegate<DecodePassSpanDelegate<T>>(decode.Target, decode.Method);
    }

    private static DecodePassSpanDelegate<T> GetDecodeDelegate<T, R, I>(DecodePassSpanDelegate<R> decode, Func<Expression, Expression> method)
    {
        var source = Expression.Parameter(typeof(ReadOnlySpan<byte>), "source");
        var invoke = method.Invoke(Expression.Convert(Expression.Call(Expression.Constant(decode.Target), decode.Method, source), typeof(I)));
        var lambda = Expression.Lambda<DecodePassSpanDelegate<T>>(invoke, source);
        return lambda.Compile();
    }

    private static DecodePassSpanDelegate<T> GetDecodeDelegate<T, E>(Converter<E> converter, Func<Expression, Expression>? method) where T : IEnumerable<E>
    {
        DecodePassSpanDelegate<IEnumerable<E>> Invoke()
        {
            if (converter is ISpanLikeContextProvider<E> provider)
                return provider.GetDecoder().Invoke;
            return new ListDecoder<E>(converter).Invoke;
        }

        var target = Invoke();
        if (method is null)
            return GetDecodeDelegate<T, IEnumerable<E>>(target);
        return GetDecodeDelegate<T, IEnumerable<E>, IEnumerable<E>>(target, method);
    }

    private static DecodePassSpanDelegate<T> GetDecodeDelegate<T, K, V>(Converter<K> init, Converter<V> tail, Func<Expression, Expression> method) where T : IEnumerable<KeyValuePair<K, V>>
    {
        return GetDecodeDelegate<T, IEnumerable<KeyValuePair<K, V>>, IEnumerable<KeyValuePair<K, V>>>(new KeyValueEnumerableDecoder<K, V>(init, tail).Invoke, method);
    }

    [RequiresUnreferencedCode(CommonModule.RequiresUnreferencedCodeMessage)]
    private static DecodePassSpanDelegate<T>? GetDecodeDelegate<T, E>(Converter<E> converter) where T : IEnumerable<E>
    {
        if (CommonModule.SelectGenericTypeDefinitionOrDefault(typeof(T), ArrayOrListAssignableDefinitions.Contains))
            return GetDecodeDelegate<T, E>(converter, null);
        if (CommonModule.SelectGenericTypeDefinitionOrDefault(typeof(T), HashSetAssignableDefinitions.Contains))
            return GetDecodeDelegate<T, HashSet<E>>(new HashSetDecoder<E>(converter).Invoke);
        if (CommonModule.SelectGenericTypeDefinitionOrDefault(typeof(T), ImmutableCollectionCreateMethods.GetValueOrDefault) is { } result)
            return GetDecodeDelegate<T, E>(converter, x => Expression.Call(result.MakeGenericMethod(typeof(E)), x));
        if (GetConstructorOrDefault(typeof(T), typeof(IEnumerable<E>)) is { } method)
            return GetDecodeDelegate<T, E>(converter, method);
        else
            return null;
    }

    [RequiresUnreferencedCode(CommonModule.RequiresUnreferencedCodeMessage)]
    private static DecodePassSpanDelegate<T>? GetDecodeDelegate<T, K, V>(Converter<K> init, Converter<V> tail) where K : notnull where T : IEnumerable<KeyValuePair<K, V>>
    {
        if (CommonModule.SelectGenericTypeDefinitionOrDefault(typeof(T), DictionaryAssignableDefinitions.Contains))
            return GetDecodeDelegate<T, Dictionary<K, V>>(new DictionaryDecoder<K, V>(init, tail).Invoke);
        if (CommonModule.SelectGenericTypeDefinitionOrDefault(typeof(T), ImmutableCollectionCreateMethods.GetValueOrDefault) is { } result)
            return GetDecodeDelegate<T, K, V>(init, tail, x => Expression.Call(result.MakeGenericMethod(typeof(K), typeof(V)), x));
        if (GetConstructorOrDefault(typeof(T), typeof(IDictionary<K, V>)) is { } target)
            return GetDecodeDelegate<T, Dictionary<K, V>, IDictionary<K, V>>(new DictionaryDecoder<K, V>(init, tail).Invoke, target);
        if (GetConstructorOrDefault(typeof(T), typeof(IReadOnlyDictionary<K, V>)) is { } second)
            return GetDecodeDelegate<T, Dictionary<K, V>, IReadOnlyDictionary<K, V>>(new DictionaryDecoder<K, V>(init, tail).Invoke, second);
        if (GetConstructorOrDefault(typeof(T), typeof(IEnumerable<KeyValuePair<K, V>>)) is { } method)
            return GetDecodeDelegate<T, K, V>(init, tail, method);
        else
            return null;
    }

    [RequiresUnreferencedCode(CommonModule.RequiresUnreferencedCodeMessage)]
    private static AllocatorAction<T?> GetEncodeDelegate<T, E>(Converter<E> converter) where T : IEnumerable<E>
    {
        if (typeof(T) == typeof(HashSet<E>))
            return (AllocatorAction<T?>)(object)new AllocatorAction<HashSet<E>?>(new HashSetEncoder<E>(converter).Encode);
        return GetEncodeDelegate<T, E>(converter.EncodeAuto) ?? new EnumerableEncoder<T, E>(converter).Encode;
    }

    [RequiresUnreferencedCode(CommonModule.RequiresUnreferencedCodeMessage)]
    private static AllocatorAction<T?> GetEncodeDelegate<T, K, V>(Converter<K> init, Converter<V> tail) where K : notnull where T : IEnumerable<KeyValuePair<K, V>>
    {
        if (typeof(T) == typeof(Dictionary<K, V>))
            return (AllocatorAction<T?>)(object)new AllocatorAction<Dictionary<K, V>>(new DictionaryEncoder<K, V>(init, tail).Encode);
        var source = new KeyValueEnumerableEncoder<T, K, V>(init, tail);
        return GetEncodeDelegate<T, KeyValuePair<K, V>>(source.EncodeKeyValuePairAuto) ?? source.Encode;
    }

    [RequiresUnreferencedCode(CommonModule.RequiresUnreferencedCodeMessage)]
    private static AllocatorAction<T?>? GetEncodeDelegate<T, E>(AllocatorAction<E> adapter)
    {
        var initial = typeof(T).GetMethods(CommonModule.PublicInstanceBindingFlags).FirstOrDefault(x => x.Name is "GetEnumerator" && x.GetParameters().Length is 0);
        if (initial is null)
            return null;
        var enumeratorType = initial.ReturnType;
        if (enumeratorType.IsValueType is false)
            return null;
        var methods = enumeratorType.GetMethods(CommonModule.PublicInstanceBindingFlags);
        var properties = enumeratorType.GetProperties(CommonModule.PublicInstanceBindingFlags);
        var dispose = methods.FirstOrDefault(x => x.Name is "Dispose" && x.GetParameters().Length is 0 && x.ReturnType == typeof(void));
        var functor = methods.FirstOrDefault(x => x.Name is "MoveNext" && x.GetParameters().Length is 0 && x.ReturnType == typeof(bool));
        var current = properties.FirstOrDefault(x => x.Name is "Current" && x.GetGetMethod() is { } method && method.GetParameters().Length is 0 && x.PropertyType == typeof(E));
        if (functor is null || current is null)
            return null;

        var allocator = Expression.Parameter(typeof(Allocator).MakeByRefType(), "allocator");
        var collection = Expression.Parameter(typeof(T), "collection");
        var enumerator = Expression.Variable(enumeratorType, "enumerator");
        var target = Expression.Label("target");
        var encode = Expression.Call(Expression.Constant(adapter.Target), adapter.Method, allocator, Expression.Property(enumerator, current));
        var origin = Expression.Loop(Expression.IfThenElse(Expression.Call(enumerator, functor), encode, Expression.Break(target)), target);
        var source = dispose is null
            ? origin as Expression
            : Expression.TryFinally(origin, Expression.Call(enumerator, dispose));
        var assign = Expression.Assign(enumerator, Expression.Call(collection, initial));
        var result = Expression.Block([enumerator], assign, source);
        var ensure = typeof(T).IsValueType
            ? result as Expression
            : Expression.IfThen(Expression.NotEqual(collection, Expression.Constant(null, typeof(T))), result);
        var lambda = Expression.Lambda<AllocatorAction<T?>>(ensure, allocator, collection);
        return lambda.Compile();
    }

    [RequiresUnreferencedCode(CommonModule.RequiresUnreferencedCodeMessage)]
    private static SequenceConverter<T> GetConverter<T, E>(IGeneratorContext context) where T : IEnumerable<E>
    {
        var converter = (Converter<E>)context.GetConverter(typeof(E));
        var encode = GetEncodeDelegate<T, E>(converter);
        var decode = GetDecodeDelegate<T, E>(converter);
        return new SequenceConverter<T>(encode, decode);
    }

    [RequiresUnreferencedCode(CommonModule.RequiresUnreferencedCodeMessage)]
    private static SequenceConverter<T> GetConverter<T, K, V>(IGeneratorContext context) where K : notnull where T : IEnumerable<KeyValuePair<K, V>>
    {
        var init = (Converter<K>)context.GetConverter(typeof(K));
        var tail = (Converter<V>)context.GetConverter(typeof(V));
        var encode = GetEncodeDelegate<T, K, V>(init, tail);
        var decode = GetDecodeDelegate<T, K, V>(init, tail);
        return new SequenceConverter<T>(encode, decode);
    }
}
