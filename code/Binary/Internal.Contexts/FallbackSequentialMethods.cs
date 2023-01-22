﻿namespace Mikodev.Binary.Internal.Contexts;

using Mikodev.Binary.Internal.SpanLike;
using Mikodev.Binary.Internal.SpanLike.Builders;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

internal static class FallbackSequentialMethods
{
    private static readonly ImmutableDictionary<Type, Type> Types = ImmutableDictionary.CreateRange(new Dictionary<Type, Type>
    {
        [typeof(List<>)] = typeof(ListBuilder<>),
        [typeof(Memory<>)] = typeof(MemoryBuilder<>),
        [typeof(ArraySegment<>)] = typeof(ArraySegmentBuilder<>),
        [typeof(ReadOnlyMemory<>)] = typeof(ReadOnlyMemoryBuilder<>),
        [typeof(ImmutableArray<>)] = typeof(ImmutableArrayBuilder<>),
    });

    private static object CreateArrayBuilder(Type type, Type elementType)
    {
        if (type != elementType.MakeArrayType())
            throw new NotSupportedException($"Only single dimensional zero based arrays are supported, type: {type}");
        return CommonModule.CreateInstance(typeof(ArrayBuilder<>).MakeGenericType(elementType), null);
    }

    [RequiresUnreferencedCode(CommonModule.RequiresUnreferencedCodeMessage)]
    internal static IConverter? GetConverter(IGeneratorContext context, Type type)
    {
        object? Invoke()
        {
            if (type.IsArray && type.GetElementType() is { } elementType)
                return CreateArrayBuilder(type, elementType);
            if (CommonModule.SelectGenericTypeDefinitionOrDefault(type, Types.GetValueOrDefault) is { } result)
                return CommonModule.CreateInstance(result.MakeGenericType(type.GetGenericArguments()), null);
            return null;
        }

        var create = Invoke();
        if (create is null)
            return null;
        var itemType = create.GetType().GetGenericArguments().Single();
        var itemConverter = context.GetConverter(itemType);

        var converterType = typeof(SpanLikeConverter<,>).MakeGenericType(type, itemType);
        var converterArguments = new object[] { create, itemConverter };
        var converter = CommonModule.CreateInstance(converterType, converterArguments);
        return (IConverter)converter;
    }
}
