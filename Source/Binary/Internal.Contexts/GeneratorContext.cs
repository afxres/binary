using Mikodev.Binary.Attributes;
using Mikodev.Binary.Converters.Unsafe.Generic;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using NameDictionary = System.Collections.Generic.IReadOnlyDictionary<System.Reflection.PropertyInfo, string>;

namespace Mikodev.Binary.Internal.Contexts
{
    internal sealed partial class GeneratorContext : IGeneratorContext
    {
        private readonly HashSet<Type> types = new HashSet<Type>();

        private readonly Dictionary<string, byte[]> texts = new Dictionary<string, byte[]>();

        private readonly ConcurrentDictionary<Type, Converter> converters;

        private readonly IEnumerable<IConverterCreator> creators;

        public GeneratorContext(ConcurrentDictionary<Type, Converter> converters, IEnumerable<IConverterCreator> creators)
        {
            this.converters = converters;
            this.creators = creators;
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

        private byte[] GetOrCache(string key)
        {
            if (!texts.TryGetValue(key, out var bytes))
                texts.Add(key, bytes = Converter.Encoding.GetBytes(key));
            return bytes;
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
            // enum
            if (type.IsEnum)
                return (Converter)Activator.CreateInstance(typeof(UnsafeNativeConverter<>).MakeGenericType(type));
            // collection
            if (type.TryGetInterfaceArguments(typeof(IEnumerable<>), out var arguments))
                return ContextMethodsOfCollections.GetConverterAsCollection(this, type, arguments.Single());

            var attribute = GetAttribute(type);
            if (attribute is ConverterAttribute converterAttribute)
                return GetConverterByAttribute(converterAttribute, type);
            if (attribute is ConverterCreatorAttribute creatorAttribute)
                return GetConverterByAttribute(creatorAttribute, type);

            // find available properties
            var properties = (IReadOnlyList<PropertyInfo>)type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(x => x.GetGetMethod()?.GetParameters().Length == 0)
                .ToList();
            if (!properties.Any())
                throw new ArgumentException($"No available property found, type: {type}");
            var collection = properties
                .Select(x => GetPropertyAttributes(x, attribute))
                .Where(x => x.Property != null)
                .ToDictionary(x => x.Property, x => (x.Key, x.Converter));
            var collectionQuery = collection.Select(x => (x.Key, x.Value.Key));
            var dictionary = default(NameDictionary);

            if (attribute is NamedObjectAttribute)
                GetPropertiesByNamedKey(type, collectionQuery, out properties, out dictionary);
            else if (attribute is TupleObjectAttribute)
                GetPropertiesByTupleKey(type, collectionQuery, out properties);

            Debug.Assert(collection.Any());
            Debug.Assert(properties.Any());
            var (constructor, indexes) = ContextMethods.GetConstructor(type, properties);
            var metadata = GetPropertyConverters(properties.Select(x => (x, collection[x].Converter)));

            // converter as tuple object
            if (attribute is TupleObjectAttribute)
                return ContextMethodsOfTupleObject.GetConverterAsTupleObject(type, constructor, indexes, metadata);
            // converter as named object (or default)
            return ContextMethodsOfNamedObject.GetConverterAsNamedObject(type, constructor, indexes, metadata, dictionary, GetOrCache);
        }
    }
}
