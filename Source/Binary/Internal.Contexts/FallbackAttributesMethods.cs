using Mikodev.Binary.Attributes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using ItemIndexes = System.Collections.Generic.IReadOnlyList<int>;
using MetaAttributes = System.Collections.Generic.IEnumerable<(System.Reflection.PropertyInfo Property, System.Attribute)>;
using MetaList = System.Collections.Generic.IReadOnlyList<(System.Reflection.PropertyInfo Property, Mikodev.Binary.Converter Converter)>;
using NameDictionary = System.Collections.Generic.IReadOnlyDictionary<System.Reflection.PropertyInfo, string>;

namespace Mikodev.Binary.Internal.Contexts
{
    internal static class FallbackAttributesMethods
    {
        private static readonly IReadOnlyCollection<Type> keyAttributeTypes = new[] { typeof(NamedKeyAttribute), typeof(TupleKeyAttribute) };

        private static readonly IReadOnlyCollection<Type> converterAttributeTypes = new[] { typeof(ConverterAttribute), typeof(ConverterCreatorAttribute) };

        internal static Converter GetConverter(IGeneratorContext context, Type type)
        {
            var attribute = GetAttribute(type);
            if (attribute is ConverterAttribute converterAttribute)
                return GetConverterByConverterAttribute(converterAttribute, type);
            if (attribute is ConverterCreatorAttribute creatorAttribute)
                return GetConverterByConverterCreatorAttribute(context, creatorAttribute, type);

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
            var enumerable = collection.Select(x => (x.Key, x.Value.Key));
            var dictionary = default(NameDictionary);
            var (origin, target) = collection.Any() ? default : attribute switch
            {
                NamedObjectAttribute _ => (nameof(NamedKeyAttribute), nameof(NamedObjectAttribute)),
                TupleObjectAttribute _ => (nameof(TupleKeyAttribute), nameof(TupleObjectAttribute)),
                _ => default,
            };

            if ((origin, target) != default)
                throw new ArgumentException($"Require '{origin}' for '{target}', type: {type}");
            if (attribute is NamedObjectAttribute)
                GetPropertiesByNamedKey(type, enumerable, out properties, out dictionary);
            else if (attribute is TupleObjectAttribute)
                GetPropertiesByTupleKey(type, enumerable, out properties);

            Debug.Assert(collection.Any());
            Debug.Assert(properties.Any());
            var (constructor, indexes) = GetConstructorWithProperties(type, properties);
            var metadata = GetPropertyConverters(context, properties.Select(x => (x, collection[x].Converter)));

            // converter as tuple object
            if (attribute is TupleObjectAttribute)
                return ContextMethodsOfTupleObject.GetConverterAsTupleObject(type, constructor, indexes, metadata.Select(x => ((MemberInfo)x.Property, x.Converter)).ToList());

            if (dictionary == null)
                dictionary = metadata.Select(x => x.Property).ToDictionary(x => x, x => x.Name);
            // converter as named object (or default)
            return ContextMethodsOfNamedObject.GetConverterAsNamedObject(context, type, constructor, indexes, metadata, dictionary);
        }

        private static Attribute GetAttribute(Type type)
        {
            var array = type.GetCustomAttributes(false);
            if (array == null || array.Length == 0)
                return null;
            var attributeTypes = new[] { typeof(ConverterAttribute), typeof(ConverterCreatorAttribute), typeof(NamedObjectAttribute), typeof(TupleObjectAttribute) };
            var attributes = array.OfType<Attribute>().Where(x => attributeTypes.Contains(x.GetType())).ToList();
            if (attributes.Count == 0)
                return null;
            if (attributes.Count > 1)
                throw new ArgumentException($"Multiple attributes found, type: {type}");
            return attributes.Single();
        }

        private static bool TryCreateInstance<T>(Type type, out T value, out Exception error) where T : class
        {
            try
            {
                var instance = Activator.CreateInstance(type);
                value = (T)instance;
                error = null;
                return true;
            }
            catch (Exception ex)
            {
                value = null;
                error = ex;
                return false;
            }
        }

        private static Converter GetConverterByConverterAttribute(ConverterAttribute attribute, Type type)
        {
            if (!TryCreateInstance<Converter>(attribute.Type, out var converter, out var error))
                throw new ArgumentException($"Can not get custom converter by attribute, expected converter item type: {type}", error);
            if (converter.ItemType != type)
                throw new ArgumentException($"Invalid custom converter '{converter.GetType()}', expected converter item type: {type}");
            return converter;
        }

        private static Converter GetConverterByConverterCreatorAttribute(IGeneratorContext context, ConverterCreatorAttribute attribute, Type type)
        {
            var creatorType = attribute.Type;
            if (!TryCreateInstance<IConverterCreator>(creatorType, out var creator, out var error))
                throw new ArgumentException($"Can not get custom converter creator by attribute, expected converter item type: {type}", error);
            var converter = creator.GetConverter(context, type);
            if (converter == null)
                throw new ArgumentException($"Invalid return value 'null', creator type: {creatorType}, expected converter item type: {type}");
            if (converter.ItemType != type)
                throw new ArgumentException($"Invalid custom converter '{converter.GetType()}', creator type: {creatorType}, expected converter item type: {type}");
            return converter;
        }

        private static MetaList GetPropertyConverters(IGeneratorContext context, IEnumerable<(PropertyInfo, Attribute)> collection)
        {
            var list = new List<(PropertyInfo, Converter)>();
            foreach (var (property, attribute) in collection)
            {
                var propertyType = property.PropertyType;
                var converter = attribute is ConverterCreatorAttribute creatorAttribute
                    ? GetConverterByConverterCreatorAttribute(context, creatorAttribute, propertyType)
                    : attribute is ConverterAttribute converterAttribute ? GetConverterByConverterAttribute(converterAttribute, propertyType) : context.GetConverter(propertyType);
                list.Add((property, converter));
                Debug.Assert(converter.ItemType == property.PropertyType);
            }
            return list;
        }

        private static void GetPropertiesByNamedKey(Type type, MetaAttributes collection, out IReadOnlyList<PropertyInfo> properties, out NameDictionary dictionary)
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

        private static void GetPropertiesByTupleKey(Type type, MetaAttributes collection, out IReadOnlyList<PropertyInfo> properties)
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
                throw new ArgumentException($"Tuple key must be start from zero and must be sequential, type: {type}");
            properties = map.Values.ToList();
        }

        private static (PropertyInfo Property, Attribute Key, Attribute Converter) GetPropertyAttributes(Type type, PropertyInfo property, Attribute attribute)
        {
            Debug.Assert(attribute is null || attribute is NamedObjectAttribute || attribute is TupleObjectAttribute);
            var array = property.GetCustomAttributes(false).OfType<Attribute>();
            var keys = array.Where(x => keyAttributeTypes.Contains(x.GetType())).ToList();
            var converters = array.Where(x => converterAttributeTypes.Contains(x.GetType())).ToList();

            if (keys.Count > 1 || converters.Count > 1)
                throw new ArgumentException($"Multiple attributes found, property name: {property.Name}, type: {type}");
            var key = keys.FirstOrDefault();
            var converter = converters.FirstOrDefault();
            if (key == null && converter != null)
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
            return attribute == null || key != null
                ? (property, key, converter)
                : default;
        }

        private static (ConstructorInfo, ItemIndexes) GetConstructorWithProperties(Type type, IReadOnlyList<PropertyInfo> properties)
        {
            static (ConstructorInfo Constructor, IReadOnlyList<PropertyInfo>) CanCreate(ConstructorInfo constructor, Dictionary<string, PropertyInfo> properties)
            {
                int parameterCount;
                var parameters = constructor.GetParameters();
                if (parameters == null || (parameterCount = parameters.Length) != properties.Count)
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
            var constructors = type.GetConstructors();
            if (constructors == null || constructors.Length == 0)
                return default;
            var names = properties.ToDictionary(x => x.Name.ToUpperInvariant());
            var query = constructors.Select(x => CanCreate(x, names)).Where(x => x.Constructor != null).ToList();
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
