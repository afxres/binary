using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;

namespace Mikodev.Binary.Internal.Contexts
{
    internal sealed class GeneratorContext : IGeneratorContext
    {
        private bool destroyed = false;

        private readonly HashSet<Type> types = new HashSet<Type>();

        private readonly ConcurrentDictionary<Type, IConverter> converters;

        private readonly IReadOnlyCollection<IConverterCreator> creators;

        public GeneratorContext(ConcurrentDictionary<Type, IConverter> converters, IReadOnlyCollection<IConverterCreator> creators)
        {
            this.converters = converters;
            this.creators = creators;
            Debug.Assert(creators is IConverterCreator[]);
        }

        public void Destroy()
        {
            this.destroyed = true;
        }

        public IConverter GetConverter(Type type)
        {
            if (this.destroyed)
                throw new InvalidOperationException("Generator context has been destroyed!");
            var converter = GetOrCreateConverter(type);
            Debug.Assert(converter is not null);
            Debug.Assert(Converter.GetGenericArgument(converter) == type);
            return this.converters.GetOrAdd(type, converter);
        }

        private IConverter GetOrCreateConverter(Type type)
        {
            if (type.IsByRef)
                throw new ArgumentException($"Invalid byref type: {type}");
            if (type.IsByRefLike)
                throw new ArgumentException($"Invalid byref-like type: {type}");
            if (type.IsPointer)
                throw new ArgumentException($"Invalid pointer type: {type}");
            if (type.IsAbstract && type.IsSealed)
                throw new ArgumentException($"Invalid static type: {type}");
            if (type.IsGenericTypeDefinition || type.IsGenericParameter)
                throw new ArgumentException($"Invalid generic type definition: {type}");

            if (this.converters.TryGetValue(type, out var converter))
                return converter;
            if (this.types.Add(type) is false)
                throw new ArgumentException($"Circular type reference detected, type: {type}");
            foreach (var creator in this.creators)
                if ((converter = creator.GetConverter(this, type)) is not null)
                    return CommonHelper.GetConverter(converter, type, creator.GetType());

            if ((converter = FallbackEndiannessMethods.GetConverter(type)) is not null)
                return converter;
            if ((converter = FallbackConvertersMethods.GetConverter(type)) is not null)
                return converter;
            if ((converter = FallbackPrimitivesMethods.GetConverter(this, type)) is not null)
                return converter;
            if ((converter = FallbackSequentialMethods.GetConverter(this, type)) is not null)
                return converter;

            if (type == typeof(Delegate) || type.IsSubclassOf(typeof(Delegate)))
                throw new ArgumentException($"Invalid delegate type: {type}");
            if (type.Assembly == typeof(IConverter).Assembly)
                throw new ArgumentException($"Invalid internal type: {type}");

            if ((converter = FallbackCollectionMethods.GetConverter(this, type)) is not null)
                return converter;

            if (type == typeof(IEnumerable) || typeof(IEnumerable).IsAssignableFrom(type))
                throw new ArgumentException($"Invalid non-generic collection type: {type}");
            if (type.Assembly == typeof(object).Assembly)
                throw new ArgumentException($"Invalid system type: {type}");

            return FallbackAttributesMethods.GetConverter(this, type);
        }
    }
}
