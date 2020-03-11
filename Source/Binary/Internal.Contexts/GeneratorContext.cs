using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Mikodev.Binary.Internal.Contexts
{
    internal sealed class GeneratorContext : IGeneratorContext
    {
        private readonly HashSet<Type> types = new HashSet<Type>();

        private readonly ConcurrentDictionary<Type, Converter> converters;

        private readonly IEnumerable<IConverterCreator> creators;

        public GeneratorContext(ConcurrentDictionary<Type, Converter> converters, IEnumerable<IConverterCreator> creators)
        {
            this.converters = converters;
            this.creators = creators;
            Debug.Assert(creators is IConverterCreator[]);
        }

        public Converter GetConverter(Type type)
        {
            if (CommonHelper.IsByRefLike(type))
                throw new ArgumentException($"Invalid byref-like type: {type}");
            if (type.IsAbstract && type.IsSealed)
                throw new ArgumentException($"Invalid static type: {type}");
            if (type.IsGenericTypeDefinition || type.IsGenericParameter)
                throw new ArgumentException($"Invalid generic type definition: {type}");

            if (converters.TryGetValue(type, out var result))
                return result;
            if (!types.Add(type))
                throw new ArgumentException($"Circular type reference detected, type: {type}");

            var converter = GetConverterByCreator(type) ?? GetConverterByDefault(type);
            Debug.Assert(converter != null);
            Debug.Assert(converter.ItemType == type);
            return converters.GetOrAdd(type, converter);
        }

        private Converter GetConverterByCreator(Type type)
        {
            var (converter, creatorType) = creators
                .Select(x => (Converter: x.GetConverter(this, type), x.GetType()))
                .FirstOrDefault(x => x.Converter != null);
            if (converter is null)
                return null;
            if (converter.ItemType != type)
                throw new ArgumentException($"Invalid converter '{converter.GetType()}', creator type: {creatorType}, expected converter item type: {type}");
            return converter;
        }

        private Converter GetConverterByDefault(Type type)
        {
            // tuple types
            if (FallbackPrimitivesMethods.GetConverter(this, type) is { } converter)
                return converter;
            // not supported
            if (type.Assembly == typeof(Converter).Assembly)
                throw new ArgumentException($"Invalid type: {type}");
            // collections and others
            if (CommonHelper.TryGetInterfaceArguments(type, typeof(IEnumerable<>), out var arguments))
                return FallbackCollectionMethods.GetConverter(this, type, arguments.Single());
            // system types
            if (type.Assembly == typeof(object).Assembly)
                throw new ArgumentException($"Invalid system type: {type}");
            else
                return FallbackAttributesMethods.GetConverter(this, type);
        }
    }
}
