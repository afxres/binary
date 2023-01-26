namespace Mikodev.Binary.Internal.Contexts;

using Mikodev.Binary.Internal.SpanLike;
using Mikodev.Binary.Internal.SpanLike.Adapters;
using Mikodev.Binary.Internal.SpanLike.Builders;
using Mikodev.Binary.Internal.SpanLike.Contexts;
using Mikodev.Binary.Internal.SpanLike.Decoders;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

internal static class FallbackSequentialMethods
{
    private static readonly MethodInfo ArrayCreateMethod;

    private static readonly MethodInfo UnboxCreateMethod;

    private static readonly ImmutableDictionary<Type, MethodInfo> CreateMethods;

    static FallbackSequentialMethods()
    {
        static MethodInfo Info(Func<Converter<object>, object> func)
        {
            return func.Method.GetGenericMethodDefinition();
        }

        var array = Info(GetArrayConverter);
        var unbox = new Func<object, object, object>(GetConverter<object>).Method.GetGenericMethodDefinition();
        var create = ImmutableDictionary.CreateRange(new Dictionary<Type, MethodInfo>
        {
            [typeof(List<>)] = Info(GetListConverter),
            [typeof(Memory<>)] = Info(GetMemoryConverter),
            [typeof(ArraySegment<>)] = Info(GetArraySegmentConverter),
            [typeof(ReadOnlyMemory<>)] = Info(GetReadOnlyMemoryConverter),
            [typeof(ImmutableArray<>)] = Info(GetImmutableArrayConverter),
        });
        CreateMethods = create;
        UnboxCreateMethod = unbox;
        ArrayCreateMethod = array;
    }

    [RequiresUnreferencedCode(CommonModule.RequiresUnreferencedCodeMessage)]
    internal static IConverter? GetConverter(IGeneratorContext context, Type type)
    {
        MethodInfo? Invoke()
        {
            if (type.IsArray && type.GetElementType() is { } elementType)
                return GetArrayMethodInfo(type, elementType);
            if (CommonModule.SelectGenericTypeDefinitionOrDefault(type, CreateMethods.GetValueOrDefault) is { } result)
                return result.MakeGenericMethod(type.GetGenericArguments());
            return null;
        }

        var method = Invoke();
        if (method is null)
            return null;
        var itemType = method.GetGenericArguments().Single();
        var itemConverter = context.GetConverter(itemType);
        var functorType = typeof(Func<,>).MakeGenericType(typeof(Converter<>).MakeGenericType(itemType), typeof(object));
        var functor = Delegate.CreateDelegate(functorType, method);
        var creator = (Func<object, object, object>)Delegate.CreateDelegate(typeof(Func<object, object, object>), UnboxCreateMethod.MakeGenericMethod(itemType));
        var converter = creator.Invoke(functor, itemConverter);
        return (IConverter)converter;
    }

    private static MethodInfo GetArrayMethodInfo(Type type, Type elementType)
    {
        if (type != elementType.MakeArrayType())
            throw new NotSupportedException($"Only single dimensional zero based arrays are supported, type: {type}");
        return ArrayCreateMethod.MakeGenericMethod(elementType);
    }

    private static SpanLikeDecoder<T> GetDecoder<T, E, B>(Converter<E> converter) where B : struct, ISpanLikeBuilder<T, E>
    {
        var source = SpanLikeContext.GetDecoderOrDefault<E[], E>(converter);
        if (source is SpanLikeDecoder<T> actual)
            return actual;
        return source is null
            ? new ArrayDecoder<T, E, B>(converter)
            : new ArrayForwardDecoder<T, E, B>(source);
    }

    private static object GetConverter<E>(object method, object data)
    {
        return ((Func<Converter<E>, object>)method).Invoke((Converter<E>)data);
    }

    private static SpanLikeConverter<T, E> GetConverter<T, E>(Converter<E> converter, SpanLikeAdapter<T, E> adapter, Func<Converter<E>, SpanLikeDecoder<T>> func)
    {
        var decoder = func.Invoke(converter);
        var encoder = SpanLikeContext.GetEncoder(converter);
        return new SpanLikeConverter<T, E>(encoder, decoder, adapter, converter);
    }

    private static SpanLikeConverter<E[], E> GetArrayConverter<E>(Converter<E> converter)
    {
        return GetConverter(converter, new ArrayAdapter<E>(), GetDecoder<E[], E, ArrayBuilder<E>>);
    }

    private static SpanLikeConverter<ArraySegment<E>, E> GetArraySegmentConverter<E>(Converter<E> converter)
    {
        return GetConverter(converter, new ArraySegmentAdapter<E>(), GetDecoder<ArraySegment<E>, E, ArraySegmentBuilder<E>>);
    }

    private static SpanLikeConverter<ImmutableArray<E>, E> GetImmutableArrayConverter<E>(Converter<E> converter)
    {
        return GetConverter(converter, new ImmutableArrayAdapter<E>(), x => new ImmutableArrayDecoder<E>(x));
    }

    private static SpanLikeConverter<List<E>, E> GetListConverter<E>(Converter<E> converter)
    {
        return GetConverter(converter, new ListAdapter<E>(), x => new ListDecoder<E>(x));
    }

    private static SpanLikeConverter<Memory<E>, E> GetMemoryConverter<E>(Converter<E> converter)
    {
        return GetConverter(converter, new MemoryAdapter<E>(), GetDecoder<Memory<E>, E, MemoryBuilder<E>>);
    }

    private static SpanLikeConverter<ReadOnlyMemory<E>, E> GetReadOnlyMemoryConverter<E>(Converter<E> converter)
    {
        return GetConverter(converter, new ReadOnlyMemoryAdapter<E>(), GetDecoder<ReadOnlyMemory<E>, E, ReadOnlyMemoryBuilder<E>>);
    }
}
