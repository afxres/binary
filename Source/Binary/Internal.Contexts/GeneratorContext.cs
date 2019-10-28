using Mikodev.Binary.Internal.Extensions;
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

        private readonly ContextTextCache cache = new ContextTextCache();

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
            if (type.IsByRefLike())
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
            if (converter == null)
                return null;
            if (converter.ItemType != type)
                throw new InvalidOperationException($"Invalid converter '{converter.GetType()}', creator type: {creatorType}, expected converter item type: {type}");
            return converter;
        }

        private Converter GetConverterByDefault(Type type)
        {
            // not supported
            if (type.Assembly == typeof(Converter).Assembly)
                throw new ArgumentException($"Invalid type: {type}");
            if (type.Assembly == typeof(object).Assembly)
                throw new ArgumentException($"Invalid system type: {type}");

            return type.TryGetInterfaceArguments(typeof(IEnumerable<>), out var arguments)
                ? FallbackCollectionMethods.GetConverter(this, type, arguments.Single())
                : FallbackAttributesMethods.GetConverter(this, type, cache);
        }
    }
}
