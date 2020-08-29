﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;

namespace Mikodev.Binary.Internal.Contexts
{
    internal sealed class GeneratorContext : IGeneratorContext
    {
        private readonly HashSet<Type> types = new HashSet<Type>();

        private readonly ConcurrentDictionary<Type, IConverter> converters;

        private readonly IReadOnlyCollection<IConverterCreator> creators;

        public GeneratorContext(ConcurrentDictionary<Type, IConverter> converters, IReadOnlyCollection<IConverterCreator> creators)
        {
            this.converters = converters;
            this.creators = creators;
            Debug.Assert(creators is IConverterCreator[]);
        }

        public IConverter GetConverter(Type type)
        {
            var converter = GetOrCreateConverter(type);
            Debug.Assert(converter != null);
            Debug.Assert(ConverterHelper.GetGenericArgument(converter) == type);
            return converters.GetOrAdd(type, converter);
        }

        private IConverter GetOrCreateConverter(Type type)
        {
            if (CommonHelper.IsByRefLike(type))
                throw new ArgumentException($"Invalid byref-like type: {type}");
            if (type.IsAbstract && type.IsSealed)
                throw new ArgumentException($"Invalid static type: {type}");
            if (type.IsGenericTypeDefinition || type.IsGenericParameter)
                throw new ArgumentException($"Invalid generic type definition: {type}");

            if (converters.TryGetValue(type, out var converter))
                return converter;
            if (types.Add(type) is false)
                throw new ArgumentException($"Circular type reference detected, type: {type}");
            foreach (var creator in creators)
                if ((converter = creator.GetConverter(this, type)) != null)
                    return ContextMethods.EnsureConverter(converter, type, creator.GetType());

            if ((converter = FallbackConvertersMethods.GetConverter(type)) != null)
                return converter;
            if ((converter = FallbackPrimitivesMethods.GetConverter(this, type)) != null)
                return converter;
            if ((converter = FallbackSequentialMethods.GetConverter(this, type)) != null)
                return converter;

            if (type.IsPointer)
                throw new ArgumentException($"Invalid pointer type: {type}");
            if (type.Assembly == typeof(IConverter).Assembly)
                throw new ArgumentException($"Invalid internal type: {type}");

            if ((converter = FallbackCollectionMethods.GetConverter(this, type)) != null)
                return converter;

            if (type.Assembly == typeof(object).Assembly)
                throw new ArgumentException($"Invalid system type: {type}");
            return FallbackAttributesMethods.GetConverter(this, type);
        }
    }
}
