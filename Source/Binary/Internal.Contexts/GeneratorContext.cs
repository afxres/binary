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
            if (CommonHelper.IsByRefLike(type))
                throw new ArgumentException($"Invalid byref-like type: {type}");
            if (type.IsPointer)
                throw new ArgumentException($"Invalid pointer type: {type}");
            if (type.IsAbstract && type.IsSealed)
                throw new ArgumentException($"Invalid static type: {type}");
            if (type.IsGenericTypeDefinition || type.IsGenericParameter)
                throw new ArgumentException($"Invalid generic type definition: {type}");

            if (converters.TryGetValue(type, out var result))
                return result;
            if (!types.Add(type))
                throw new ArgumentException($"Circular type reference detected, type: {type}");

            var converter = GetConverterInternal(type);
            Debug.Assert(converter != null);
            Debug.Assert(ConverterHelper.GetGenericArgument(converter) == type);
            return converters.GetOrAdd(type, converter);
        }

        private IConverter GetConverterInternal(Type type)
        {
            // fetch all converter creators
            var (converter, creatorType) = creators
                .Select(x => (Converter: x.GetConverter(this, type), x.GetType()))
                .FirstOrDefault(x => x.Converter != null);
            if ((converter, creatorType) != default)
                return ContextMethods.EnsureConverter(converter, type, creatorType);

            // tuple types
            if ((converter = FallbackPrimitivesMethods.GetConverter(this, type)) != null)
                return converter;
            // not supported
            if (type.Assembly == typeof(IConverter).Assembly)
                throw new ArgumentException($"Invalid internal type: {type}");
            // collections and others
            if (CommonHelper.TryGetInterfaceArguments(type, typeof(IEnumerable<>), out var arguments))
                return FallbackCollectionMethods.GetConverter(this, type, arguments.Single());
            // system types
            if (type.Assembly == typeof(object).Assembly)
                throw new ArgumentException($"Invalid system type: {type}");

            // create converter or throw
            return FallbackAttributesMethods.GetConverter(this, type);
        }
    }
}
