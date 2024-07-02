namespace Mikodev.Binary.Internal.Contexts;

using Mikodev.Binary.Creators.Endianness;
using Mikodev.Binary.Internal.SpanLike;
using Mikodev.Binary.Internal.SpanLike.Adapters;
using Mikodev.Binary.Internal.SpanLike.Contexts;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

internal static class FallbackSequentialMethods
{
    private static readonly MethodInfo ArrayCreateMethod;

    private static readonly MethodInfo UnboxCreateMethod;

    private static readonly FrozenDictionary<Type, MethodInfo> CreateMethods;

    static FallbackSequentialMethods()
    {
        static MethodInfo Info(Func<Converter<object>, object> func)
        {
            return func.Method.GetGenericMethodDefinition();
        }

        var array = Info(GetArrayConverter);
        var unbox = new Func<MethodInfo, object, object>(GetConverter<object>).Method.GetGenericMethodDefinition();
        var create = new Dictionary<Type, MethodInfo>
        {
            [typeof(List<>)] = Info(GetListConverter),
            [typeof(Memory<>)] = Info(GetMemoryConverter),
            [typeof(ArraySegment<>)] = Info(GetArraySegmentConverter),
            [typeof(ReadOnlyMemory<>)] = Info(GetReadOnlyMemoryConverter),
            [typeof(ImmutableArray<>)] = Info(GetImmutableArrayConverter),
        };
        CreateMethods = create.ToFrozenDictionary();
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

    [RequiresUnreferencedCode(CommonModule.RequiresUnreferencedCodeMessage)]
    private static MethodInfo GetArrayMethodInfo(Type type, Type elementType)
    {
        if (type.IsSZArray is false)
            throw new NotSupportedException($"Only single dimensional zero based arrays are supported, type: {type}");
        return ArrayCreateMethod.MakeGenericMethod(elementType);
    }

    private static object GetConverter<E>(MethodInfo method, object data)
    {
        var converter = (Converter<E>)data;
        var target = CommonModule.CreateDelegate<Func<Converter<E>, object>>(null, method);
        return target.Invoke(converter);
    }

    private static Converter<T> GetConverter<T, E, A>(Converter<E> converter) where A : ISpanLikeAdapter<T, E>
    {
        return converter is NativeEndianConverter<E> ? new ArrayBasedNativeEndianConverter<T, E, A>() : new ArrayBasedConverter<T, E, A>(converter);
    }

    internal static Converter<E[]> GetArrayConverter<E>(Converter<E> converter)
    {
        return GetConverter<E[], E, ArrayAdapter<E>>(converter);
    }

    internal static Converter<ArraySegment<E>> GetArraySegmentConverter<E>(Converter<E> converter)
    {
        return GetConverter<ArraySegment<E>, E, ArraySegmentAdapter<E>>(converter);
    }

    internal static Converter<ImmutableArray<E>> GetImmutableArrayConverter<E>(Converter<E> converter)
    {
        return GetConverter<ImmutableArray<E>, E, ImmutableArrayAdapter<E>>(converter);
    }

    internal static Converter<List<E>> GetListConverter<E>(Converter<E> converter)
    {
        return converter is NativeEndianConverter<E> ? new ListNativeEndianConverter<E>() : new ListConverter<E>(converter);
    }

    internal static Converter<Memory<E>> GetMemoryConverter<E>(Converter<E> converter)
    {
        return GetConverter<Memory<E>, E, MemoryAdapter<E>>(converter);
    }

    internal static Converter<ReadOnlyMemory<E>> GetReadOnlyMemoryConverter<E>(Converter<E> converter)
    {
        return GetConverter<ReadOnlyMemory<E>, E, ReadOnlyMemoryAdapter<E>>(converter);
    }
}
