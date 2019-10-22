using Mikodev.Binary.Attributes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using MetaAttributes = System.Collections.Generic.IEnumerable<(System.Reflection.PropertyInfo Property, System.Attribute)>;
using MetaList = System.Collections.Generic.IReadOnlyList<(System.Reflection.PropertyInfo Property, Mikodev.Binary.Converter Converter)>;
using NameDictionary = System.Collections.Generic.IReadOnlyDictionary<System.Reflection.PropertyInfo, string>;

namespace Mikodev.Binary.Internal.Contexts
{
    internal sealed partial class GeneratorContext
    {
        private static readonly IReadOnlyList<Type> keyAttributeTypes = new[] { typeof(NamedKeyAttribute), typeof(TupleKeyAttribute) };

        private static readonly IReadOnlyList<Type> converterAttributeTypes = new[] { typeof(ConverterAttribute), typeof(ConverterCreatorAttribute) };

        private Attribute GetAttribute(Type type)
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

        private bool TryCreateInstance<T>(Type type, out T value, out Exception error) where T : class
        {
            try
            {
                var instance = Activator.CreateInstance(type);
                value = (T)instance;
                error = null;
                return true;
            }
            catch (Exception exception)
            {
                value = null;
                error = exception;
                return false;
            }
        }

        private Converter GetConverterByAttribute(ConverterAttribute attribute, Type type)
        {
            if (!TryCreateInstance<Converter>(attribute.Type, out var converter, out var error))
                throw new ArgumentException($"Can not get custom converter by attribute, expected converter item type: {type}", error);
            if (converter.ItemType != type)
                throw new InvalidOperationException($"Invalid custom converter '{converter.GetType()}', expected converter item type: {type}");
            return converter;
        }

        private Converter GetConverterByAttribute(ConverterCreatorAttribute attribute, Type type)
        {
            var creatorType = attribute.Type;
            if (!TryCreateInstance<IConverterCreator>(creatorType, out var creator, out var error))
                throw new ArgumentException($"Can not get custom converter creator by attribute, expected converter item type: {type}", error);
            var converter = creator.GetConverter(this, type);
            if (converter == null)
                throw new InvalidOperationException($"Invalid return value 'null', creator type: {creatorType}, expected converter item type: {type}");
            if (converter.ItemType != type)
                throw new InvalidOperationException($"Invalid custom converter '{converter.GetType()}', creator type: {creatorType}, expected converter item type: {type}");
            return converter;
        }

        private MetaList GetPropertyConverters(IEnumerable<(PropertyInfo, Attribute)> collection)
        {
            var list = new List<(PropertyInfo, Converter)>();
            foreach (var (property, attribute) in collection)
            {
                var propertyType = property.PropertyType;
                var converter = attribute is ConverterCreatorAttribute creatorAttribute
                    ? GetConverterByAttribute(creatorAttribute, propertyType)
                    : attribute is ConverterAttribute converterAttribute ? GetConverterByAttribute(converterAttribute, propertyType) : GetConverter(propertyType);
                list.Add((property, converter));
                Debug.Assert(converter.ItemType == property.PropertyType);
            }
            return list;
        }

        private void GetPropertiesByNamedKey(Type type, MetaAttributes collection, out IReadOnlyList<PropertyInfo> properties, out NameDictionary dictionary)
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

        private void GetPropertiesByTupleKey(Type type, MetaAttributes collection, out IReadOnlyList<PropertyInfo> properties)
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

        private (PropertyInfo Property, Attribute Key, Attribute Converter) GetPropertyAttributes(Type type, PropertyInfo property, Attribute attribute)
        {
            Debug.Assert(attribute == null || attribute is NamedObjectAttribute || attribute is TupleObjectAttribute);
            var array = property.GetCustomAttributes(false).OfType<Attribute>();
            var keys = array.Where(x => keyAttributeTypes.Contains(x.GetType())).ToList();
            var converters = array.Where(x => converterAttributeTypes.Contains(x.GetType())).ToList();

            if (keys.Count > 1 || converters.Count > 1)
                throw new ArgumentException($"Multiple attributes found, property name: {property.Name}, type: {type}");
            var key = keys.FirstOrDefault();
            var converter = converters.FirstOrDefault();
            if (key == null && converter != null)
                throw new ArgumentException($"Require '{nameof(NamedKeyAttribute)}' or '{nameof(TupleKeyAttribute)}' for '{converter.GetType().Name}', property name: {property.Name}, type: {type}");

            var (origin, target) = (attribute, key) switch
            {
                (null, NamedKeyAttribute _) => (nameof(NamedObjectAttribute), nameof(NamedKeyAttribute)),
                (null, TupleKeyAttribute _) => (nameof(TupleObjectAttribute), nameof(TupleKeyAttribute)),
                (NamedObjectAttribute _, _) when key != null && !(key is NamedKeyAttribute) => (nameof(NamedKeyAttribute), nameof(NamedObjectAttribute)),
                (TupleObjectAttribute _, _) when key != null && !(key is TupleKeyAttribute) => (nameof(TupleKeyAttribute), nameof(TupleObjectAttribute)),
                _ => default,
            };

            if ((origin, target) != default)
                throw new ArgumentException($"Require '{origin}' for '{target}', property name: {property.Name}, type: {type}");
            return attribute == null || key != null
                ? (property, key, converter)
                : default;
        }
    }
}
