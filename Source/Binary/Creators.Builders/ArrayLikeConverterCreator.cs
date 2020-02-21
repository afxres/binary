﻿using Mikodev.Binary.Internal.Adapters;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mikodev.Binary.Creators.Builders
{
    internal sealed class ArrayLikeConverterCreator : IConverterCreator
    {
        private static readonly IReadOnlyDictionary<Type, Type> Types = new Dictionary<Type, Type>
        {
            [typeof(ArraySegment<>)] = typeof(ArraySegmentBuilder<>),
            [typeof(Memory<>)] = typeof(MemoryBuilder<>),
            [typeof(ReadOnlyMemory<>)] = typeof(ReadOnlyMemoryBuilder<>),
        };

        public Converter GetConverter(IGeneratorContext context, Type type)
        {
            if (!type.IsGenericType || !Types.TryGetValue(type.GetGenericTypeDefinition(), out var definition))
                return null;
            var itemType = type.GetGenericArguments().Single();
            var itemConverter = context.GetConverter(itemType);
            var builderType = definition.MakeGenericType(itemType);
            var builder = Activator.CreateInstance(builderType);
            var converterArguments = new object[] { builder, itemConverter };
            var converterType = typeof(ArrayLikeAdaptedConverter<,>).MakeGenericType(type, itemType);
            var converter = Activator.CreateInstance(converterType, converterArguments);
            return (Converter)converter;
        }
    }
}
