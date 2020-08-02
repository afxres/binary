using Mikodev.Binary.Attributes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Mikodev.Binary.Internal.Contexts
{
    internal static class FallbackAttributesMethods
    {
        private static readonly IReadOnlyCollection<Type> KeyAttributeTypes = new[] { typeof(NamedKeyAttribute), typeof(TupleKeyAttribute) };

        private static readonly IReadOnlyCollection<Type> ConverterAttributeTypes = new[] { typeof(ConverterAttribute), typeof(ConverterCreatorAttribute) };

        internal static IConverter GetConverter(IGeneratorContext context, Type type)
        {
            var attribute = GetAttribute(type);
            if (attribute is ConverterAttribute converterAttribute)
                return GetConverterByConverterAttribute(type, converterAttribute);
            if (attribute is ConverterCreatorAttribute creatorAttribute)
                return GetConverterByConverterCreatorAttribute(context, type, creatorAttribute);

            // find available properties
            var properties = (IReadOnlyList<PropertyInfo>)type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(x => x.GetGetMethod()?.GetParameters().Length == 0)
                .OrderBy(x => x.Name)
                .ToList();
            if (!properties.Any())
                throw new ArgumentException($"No available property found, type: {type}");
            var collection = properties
                .Select(x => GetPropertyAttributes(type, x, attribute))
                .Where(x => x.Property != null)
                .ToDictionary(x => x.Property, x => (x.Key, x.Converter));
            var enumerable = collection.Select(x => (x.Key, x.Value.Key)).ToList();
            var dictionary = default(IReadOnlyDictionary<PropertyInfo, string>);
            var (origin, target) = collection.Any() ? default : attribute switch
            {
                NamedObjectAttribute _ => (nameof(NamedKeyAttribute), nameof(NamedObjectAttribute)),
                TupleObjectAttribute _ => (nameof(TupleKeyAttribute), nameof(TupleObjectAttribute)),
                _ => default,
            };

            if ((origin, target) != default)
                throw new ArgumentException($"Require '{origin}' for '{target}', type: {type}");
            if (attribute is NamedObjectAttribute)
                GetSortedPropertiesByNamedKey(type, enumerable, out properties, out dictionary);
            else if (attribute is TupleObjectAttribute)
                GetSortedPropertiesByTupleKey(type, enumerable, out properties);

            Debug.Assert(collection.Any());
            Debug.Assert(properties.Any());
            var (constructor, indexes) = GetConstructorWithProperties(type, properties);
            var converters = GetPropertyConverters(context, properties.Select(x => (x, collection[x].Converter)).ToList());

            // converter as tuple object
            if (attribute is TupleObjectAttribute)
                return ContextMethodsOfTupleObject.GetConverterAsTupleObject(type, properties, converters, constructor, indexes, properties.Select(x => new Func<Expression, Expression>(e => Expression.Property(e, x))).ToList());

            if (dictionary is null)
                dictionary = properties.ToDictionary(x => x, x => x.Name);
            // converter as named object (or default)
            return ContextMethodsOfNamedObject.GetConverterAsNamedObject(context, type, properties, converters, constructor, indexes, dictionary);
        }

        private static Attribute GetAttribute(Type type)
        {
            var attributeTypes = new[] { typeof(ConverterAttribute), typeof(ConverterCreatorAttribute), typeof(NamedObjectAttribute), typeof(TupleObjectAttribute) };
            var attributes = type.GetCustomAttributes(false).OfType<Attribute>().Where(x => attributeTypes.Contains(x.GetType())).ToList();
            if (attributes.Count == 0)
                return null;
            if (attributes.Count > 1)
                throw new ArgumentException($"Multiple attributes found, type: {type}");
            return attributes.Single();
        }

        private static Exception GetInstanceOrException<T>(Type type, out T result) where T : class
        {
            try
            {
                result = (T)Activator.CreateInstance(type);
                return null;
            }
            catch (Exception exception)
            {
                result = null;
                return exception;
            }
        }

        private static IConverter GetConverterByConverterAttribute(Type type, ConverterAttribute attribute)
        {
            if (GetInstanceOrException<IConverter>(attribute.Type, out var converter) is { } exception)
                throw new ArgumentException($"Can not get custom converter by attribute, expected converter item type: {type}", exception);
            if (ConverterHelper.GetGenericArgument(converter) != type)
                throw new ArgumentException($"Invalid custom converter '{converter.GetType()}', expected converter item type: {type}");
            return converter;
        }

        private static IConverter GetConverterByConverterCreatorAttribute(IGeneratorContext context, Type type, ConverterCreatorAttribute attribute)
        {
            var creatorType = attribute.Type;
            if (GetInstanceOrException<IConverterCreator>(creatorType, out var creator) is { } exception)
                throw new ArgumentException($"Can not get custom converter creator by attribute, expected converter item type: {type}", exception);
            var converter = creator.GetConverter(context, type);
            if (converter is null)
                throw new ArgumentException($"Invalid return value 'null', creator type: {creatorType}, expected converter item type: {type}");
            if (ConverterHelper.GetGenericArgument(converter) != type)
                throw new ArgumentException($"Invalid return value '{converter.GetType()}', creator type: {creatorType}, expected converter item type: {type}");
            return converter;
        }

        private static IReadOnlyList<IConverter> GetPropertyConverters(IGeneratorContext context, IReadOnlyList<(PropertyInfo, Attribute)> collection)
        {
            var converters = new List<IConverter>();
            foreach (var (property, attribute) in collection)
            {
                var propertyType = property.PropertyType;
                var converter = attribute switch
                {
                    ConverterAttribute converterAttribute => GetConverterByConverterAttribute(propertyType, converterAttribute),
                    ConverterCreatorAttribute creatorAttribute => GetConverterByConverterCreatorAttribute(context, propertyType, creatorAttribute),
                    _ => context.GetConverter(propertyType)
                };
                converters.Add(converter);
                Debug.Assert(ConverterHelper.GetGenericArgument(converter) == property.PropertyType);
            }
            return converters;
        }

        private static void GetSortedPropertiesByNamedKey(Type type, IReadOnlyCollection<(PropertyInfo, Attribute)> collection, out IReadOnlyList<PropertyInfo> properties, out IReadOnlyDictionary<PropertyInfo, string> dictionary)
        {
            Debug.Assert(collection.Any());
            var map = new SortedDictionary<string, PropertyInfo>();
            foreach (var (property, attribute) in collection)
            {
                var key = ((NamedKeyAttribute)attribute).Key;
                if (string.IsNullOrEmpty(key))
                    throw new ArgumentException($"Named key can not be null or empty, property name: {property.Name}, type: {type}");
                if (map.ContainsKey(key))
                    throw new ArgumentException($"Named key '{key}' already exists, property name: {property.Name}, type: {type}");
                map.Add(key, property);
            }
            properties = map.Values.ToList();
            dictionary = map.ToDictionary(x => x.Value, x => x.Key);
        }

        private static void GetSortedPropertiesByTupleKey(Type type, IReadOnlyCollection<(PropertyInfo, Attribute)> collection, out IReadOnlyList<PropertyInfo> properties)
        {
            Debug.Assert(collection.Any());
            var map = new SortedDictionary<int, PropertyInfo>();
            foreach (var (property, attribute) in collection)
            {
                var key = ((TupleKeyAttribute)attribute).Key;
                if (map.ContainsKey(key))
                    throw new ArgumentException($"Tuple key '{key}' already exists, property name: {property.Name}, type: {type}");
                map.Add(key, property);
            }
            var keys = map.Keys.ToList();
            if (keys.First() != 0 || keys.Last() != keys.Count - 1)
                throw new ArgumentException($"Tuple key must be start at zero and must be sequential, type: {type}");
            properties = map.Values.ToList();
        }

        private static (PropertyInfo Property, Attribute Key, Attribute Converter) GetPropertyAttributes(Type type, PropertyInfo property, Attribute attribute)
        {
            Debug.Assert(attribute is null || attribute is NamedObjectAttribute || attribute is TupleObjectAttribute);
            var attributes = property.GetCustomAttributes(false).OfType<Attribute>().ToList();
            var keys = attributes.Where(x => KeyAttributeTypes.Contains(x.GetType())).ToList();
            var converters = attributes.Where(x => ConverterAttributeTypes.Contains(x.GetType())).ToList();

            if (keys.Count > 1 || converters.Count > 1)
                throw new ArgumentException($"Multiple attributes found, property name: {property.Name}, type: {type}");
            var key = keys.FirstOrDefault();
            var converter = converters.FirstOrDefault();
            if (key is null && converter != null)
                throw new ArgumentException($"Require '{nameof(NamedKeyAttribute)}' or '{nameof(TupleKeyAttribute)}' for '{converter.GetType().Name}', property name: {property.Name}, type: {type}");

            Debug.Assert(key is null || key is NamedKeyAttribute || key is TupleKeyAttribute);
            var (origin, target) = (attribute, key) switch
            {
                (null, NamedKeyAttribute _) => (nameof(NamedObjectAttribute), nameof(NamedKeyAttribute)),
                (null, TupleKeyAttribute _) => (nameof(TupleObjectAttribute), nameof(TupleKeyAttribute)),
                (NamedObjectAttribute _, TupleKeyAttribute _) => (nameof(NamedKeyAttribute), nameof(NamedObjectAttribute)),
                (TupleObjectAttribute _, NamedKeyAttribute _) => (nameof(TupleKeyAttribute), nameof(TupleObjectAttribute)),
                _ => default,
            };

            if ((origin, target) != default)
                throw new ArgumentException($"Require '{origin}' for '{target}', property name: {property.Name}, type: {type}");
            return attribute is null || key != null
                ? (property, key, converter)
                : default;
        }

        private static (ConstructorInfo, IReadOnlyList<int>) GetConstructorWithProperties(Type type, IReadOnlyList<PropertyInfo> properties)
        {
            static (ConstructorInfo Constructor, IReadOnlyList<PropertyInfo>) CanCreate(ConstructorInfo constructor, Dictionary<string, PropertyInfo> properties)
            {
                int parameterCount;
                var parameters = constructor.GetParameters();
                if ((parameterCount = parameters.Length) != properties.Count)
                    return default;
                var collection = new PropertyInfo[parameterCount];
                for (var i = 0; i < parameterCount; i++)
                {
                    var parameter = parameters[i];
                    var parameterName = parameter.Name.ToUpperInvariant();
                    if (!properties.TryGetValue(parameterName, out var property) || property.PropertyType != parameter.ParameterType)
                        return default;
                    collection[i] = property;
                }
                Debug.Assert(collection.All(x => x != null));
                return (constructor, collection);
            }

            if (type.IsAbstract || type.IsInterface)
                return default;
            var selector = new Func<PropertyInfo, string>(x => x.Name.ToUpperInvariant());
            if (properties.Select(selector).Distinct().Count() != properties.Count)
                return default;
            var names = properties.ToDictionary(selector);
            var query = type.GetConstructors().Select(x => CanCreate(x, names)).Where(x => x.Constructor != null).ToList();
            if (query.Count == 0)
                return default;
            if (query.Count != 1)
                throw new ArgumentException($"Multiple constructors found, type: {type}");
            var (constructor, collection) = query.Single();
            var value = properties.Select((x, i) => (Key: x, Value: i)).ToDictionary(x => x.Key, x => x.Value);
            Debug.Assert(properties.Count == collection.Count);
            var array = collection.Select(x => value[x]).ToArray();
            return (constructor, array);
        }
    }
}
