namespace Mikodev.Binary.Internal.Contexts;

using Mikodev.Binary.Internal.SpanLike;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

[RequiresDynamicCode(CommonDefine.RequiresDynamicCodeMessage)]
[RequiresUnreferencedCode(CommonDefine.RequiresUnreferencedCodeMessage)]
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

        var array = Info(SpanLikeFactory.GetArrayConverter);
        var unbox = new Func<MethodInfo, object, object>(GetConverter<object>).Method.GetGenericMethodDefinition();
        var create = new Dictionary<Type, MethodInfo>
        {
            [typeof(List<>)] = Info(SpanLikeFactory.GetListConverter),
            [typeof(Memory<>)] = Info(SpanLikeFactory.GetMemoryConverter),
            [typeof(ArraySegment<>)] = Info(SpanLikeFactory.GetArraySegmentConverter),
            [typeof(ReadOnlyMemory<>)] = Info(SpanLikeFactory.GetReadOnlyMemoryConverter),
            [typeof(ImmutableArray<>)] = Info(SpanLikeFactory.GetImmutableArrayConverter),
        };
        CreateMethods = create.ToFrozenDictionary();
        UnboxCreateMethod = unbox;
        ArrayCreateMethod = array;
    }

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
}
