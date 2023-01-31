namespace Mikodev.Binary.Internal.Contexts;

using Mikodev.Binary.Internal.SpanLike;
using Mikodev.Binary.Internal.SpanLike.Adapters;
using Mikodev.Binary.Internal.SpanLike.Builders;
using Mikodev.Binary.Internal.SpanLike.Contexts;
using Mikodev.Binary.Internal.SpanLike.Decoders;
using Mikodev.Binary.Internal.SpanLike.Encoders;
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
        var unbox = new Func<MethodInfo, object, object>(GetConverter<object>).Method.GetGenericMethodDefinition();
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
        var creator = CommonModule.CreateDelegate<Func<MethodInfo, object, object>>(null, UnboxCreateMethod.MakeGenericMethod(itemType));
        var converter = creator.Invoke(method, itemConverter);
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

    private static SpanLikeEncoder<T> GetEncoder<T, E, A>(Converter<E> converter) where A : struct, ISpanLikeAdapter<T, E>
    {
        var source = SpanLikeContext.GetEncoderOrDefault(converter);
        if (source is not null)
            return new ConstantForwardEncoder<T, E, A>(source, converter.Length);
        return converter.Length is 0
            ? new VariableEncoder<T, E, A>(converter)
            : new ConstantEncoder<T, E, A>(converter);
    }

    private static object GetConverter<E>(MethodInfo method, object data)
    {
        var converter = (Converter<E>)data;
        var target = CommonModule.CreateDelegate<Func<Converter<E>, object>>(null, method);
        return target.Invoke(converter);
    }

    private static SpanLikeConverter<E[]> GetArrayConverter<E>(Converter<E> converter)
    {
        var decoder = GetDecoder<E[], E, ArrayBuilder<E>>(converter);
        var encoder = GetEncoder<E[], E, ArrayAdapter<E>>(converter);
        return new SpanLikeConverter<E[]>(encoder, decoder);
    }

    private static SpanLikeConverter<ArraySegment<E>> GetArraySegmentConverter<E>(Converter<E> converter)
    {
        var decoder = GetDecoder<ArraySegment<E>, E, ArraySegmentBuilder<E>>(converter);
        var encoder = GetEncoder<ArraySegment<E>, E, ArraySegmentAdapter<E>>(converter);
        return new SpanLikeConverter<ArraySegment<E>>(encoder, decoder);
    }

    private static SpanLikeConverter<ImmutableArray<E>> GetImmutableArrayConverter<E>(Converter<E> converter)
    {
        var decoder = new ImmutableArrayDecoder<E>(converter);
        var encoder = GetEncoder<ImmutableArray<E>, E, ImmutableArrayAdapter<E>>(converter);
        return new SpanLikeConverter<ImmutableArray<E>>(encoder, decoder);
    }

    private static SpanLikeConverter<List<E>> GetListConverter<E>(Converter<E> converter)
    {
        var decoder = SpanLikeContext.GetDecoderOrDefault<List<E>, E>(converter) ?? new ListDecoder<E>(converter);
        var encoder = GetEncoder<List<E>, E, ListAdapter<E>>(converter);
        return new SpanLikeConverter<List<E>>(encoder, decoder);
    }

    private static SpanLikeConverter<Memory<E>> GetMemoryConverter<E>(Converter<E> converter)
    {
        var decoder = GetDecoder<Memory<E>, E, MemoryBuilder<E>>(converter);
        var encoder = GetEncoder<Memory<E>, E, MemoryAdapter<E>>(converter);
        return new SpanLikeConverter<Memory<E>>(encoder, decoder);
    }

    private static SpanLikeConverter<ReadOnlyMemory<E>> GetReadOnlyMemoryConverter<E>(Converter<E> converter)
    {
        var decoder = GetDecoder<ReadOnlyMemory<E>, E, ReadOnlyMemoryBuilder<E>>(converter);
        var encoder = GetEncoder<ReadOnlyMemory<E>, E, ReadOnlyMemoryAdapter<E>>(converter);
        return new SpanLikeConverter<ReadOnlyMemory<E>>(encoder, decoder);
    }
}
