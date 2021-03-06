﻿using Mikodev.Binary.Internal.SpanLike;
using Mikodev.Binary.Internal.SpanLike.Builders;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Mikodev.Binary.Internal.Contexts
{
    internal static class FallbackSequentialMethods
    {
        private static readonly ImmutableDictionary<Type, Type> Types = ImmutableDictionary.CreateRange(new Dictionary<Type, Type>
        {
            [typeof(List<>)] = typeof(ListBuilder<>),
            [typeof(Memory<>)] = typeof(MemoryBuilder<>),
            [typeof(ArraySegment<>)] = typeof(ArraySegmentBuilder<>),
            [typeof(ReadOnlyMemory<>)] = typeof(ReadOnlyMemoryBuilder<>),
        });

        private static object CreateArrayBuilder(Type type, Type elementType)
        {
            if (type != elementType.MakeArrayType())
                throw new NotSupportedException($"Only single dimensional zero based arrays are supported, type: {type}");
            return Activator.CreateInstance(typeof(ArrayBuilder<>).MakeGenericType(elementType));
        }

        internal static IConverter GetConverter(IGeneratorContext context, Type type)
        {
            object Invoke()
            {
                if (type.IsArray && type.GetElementType() is { } elementType)
                    return CreateArrayBuilder(type, elementType);
                if (CommonHelper.SelectGenericTypeDefinitionOrDefault(type, Types.GetValueOrDefault) is { } result)
                    return Activator.CreateInstance(result.MakeGenericType(type.GetGenericArguments()));
                return null;
            }

            var create = Invoke();
            if (create is null)
                return null;
            var itemType = create.GetType().GetGenericArguments().Single();
            var itemConverter = context.GetConverter(itemType);

            var converterType = typeof(SpanLikeConverter<,>).MakeGenericType(type, itemType);
            var converterArguments = new object[] { create, itemConverter };
            var converter = Activator.CreateInstance(converterType, converterArguments);
            return (IConverter)converter;
        }
    }
}
