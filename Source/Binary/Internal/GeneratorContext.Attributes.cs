using Mikodev.Binary.Attributes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using MetaAttributes = System.Collections.Generic.IEnumerable<(System.Reflection.PropertyInfo, System.Attribute)>;
using MetaList = System.Collections.Generic.IReadOnlyList<(System.Reflection.PropertyInfo Property, Mikodev.Binary.Converter Converter)>;
using NameDictionary = System.Collections.Generic.IReadOnlyDictionary<System.Reflection.PropertyInfo, string>;

namespace Mikodev.Binary.Internal
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

        private (T, Exception) GetConverterOrCreator<T>(Type type) where T : class
        {
            try
            {
                return ((T)Activator.CreateInstance(type), null);
            }
            catch (Exception ex)
            {
                return (null, ex);
            }
        }

        private Converter GetConverterByAttribute(ConverterAttribute attribute, Type type)
        {
            var (converter, error) = GetConverterOrCreator<Converter>(attribute.Type);
            if (error != null)
                throw new ArgumentException($"Can not get custom converter by attribute, expected converter item type: {type}", error);
            if (converter.ItemType != type)
                throw new InvalidOperationException($"Invalid custom converter '{converter.GetType()}', expected converter item type: {type}");
            return converter;
        }

        private Converter GetConverterByAttribute(ConverterCreatorAttribute attribute, Type type)
        {
            var creatorType = attribute.Type;
            var (creator, error) = GetConverterOrCreator<IConverterCreator>(creatorType);
            if (error != null)
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

        private static void RequireAttribute(string origin, string target, Type type)
        {
            throw new ArgumentException($"Require '{origin}' for '{target}', type: {type}");
        }

        private static void RequireAttribute(string origin, string target, PropertyInfo property)
        {
            throw new ArgumentException($"Require '{origin}' for '{target}', property name: {property.Name}, type: {property.DeclaringType}");
        }

        private void GetPropertiesByNamedKey(Type type, MetaAttributes collection, out IReadOnlyList<PropertyInfo> properties, out NameDictionary dictionary)
        {
            if (!collection.Any())
                RequireAttribute(nameof(NamedKeyAttribute), nameof(NamedObjectAttribute), type);
            var map = new SortedDictionary<string, PropertyInfo>();
            foreach (var (property, attribute) in collection)
            {
                var key = ((NamedKeyAttribute)attribute).Key;
                if (string.IsNullOrEmpty(key))
                    throw new ArgumentException($"Named key can not be null or empty, property name: {property.Name}, type: {type}");
                if (map.ContainsKey(key))
                    throw new ArgumentException($"Named key '{key}' already exists, type: {property.DeclaringType}");
                map.Add(key, property);
            }
            properties = map.Values.ToList();
            dictionary = map.ToDictionary(x => x.Value, x => x.Key);
        }

        private void GetPropertiesByTupleKey(Type type, MetaAttributes collection, out IReadOnlyList<PropertyInfo> properties)
        {
            if (!collection.Any())
                RequireAttribute(nameof(TupleKeyAttribute), nameof(TupleObjectAttribute), type);
            var map = new SortedDictionary<int, PropertyInfo>();
            foreach (var (property, attribute) in collection)
            {
                var key = ((TupleKeyAttribute)attribute).Key;
                if (map.ContainsKey(key))
                    throw new ArgumentException($"Tuple key '{key}' already exists, type: {property.DeclaringType}");
                map.Add(key, property);
            }
            var keys = map.Keys.ToList();
            if (keys.First() != 0 || keys.Last() != keys.Count - 1)
                throw new ArgumentException($"Tuple key must be start from zero and must be sequential, type: {type}");
            properties = map.Values.ToList();
        }

        private (PropertyInfo Property, Attribute Key, Attribute Converter) GetPropertyAttributes(PropertyInfo property, Attribute attribute)
        {
            Debug.Assert(attribute == null || attribute is NamedObjectAttribute || attribute is TupleObjectAttribute);
            var array = property.GetCustomAttributes(false).OfType<Attribute>();
            var keys = array.Where(x => keyAttributeTypes.Contains(x.GetType())).ToList();
            var converters = array.Where(x => converterAttributeTypes.Contains(x.GetType())).ToList();

            if (keys.Count > 1 || converters.Count > 1)
                throw new ArgumentException($"Multiple attributes found, property name: {property.Name}, type: {property.DeclaringType}");
            var key = keys.FirstOrDefault();
            var converter = converters.FirstOrDefault();
            if (key == null && converter != null)
                throw new ArgumentException($"Require '{nameof(NamedKeyAttribute)}' or '{nameof(TupleKeyAttribute)}' for '{converter.GetType().Name}', property name: {property.Name}, type: {property.DeclaringType}");

            if (attribute == null && key is NamedKeyAttribute)
                RequireAttribute(nameof(NamedObjectAttribute), nameof(NamedKeyAttribute), property);
            if (attribute == null && key is TupleKeyAttribute)
                RequireAttribute(nameof(TupleObjectAttribute), nameof(TupleKeyAttribute), property);
            if (attribute is NamedObjectAttribute && key != null && !(key is NamedKeyAttribute))
                RequireAttribute(nameof(NamedKeyAttribute), nameof(NamedObjectAttribute), property);
            if (attribute is TupleObjectAttribute && key != null && !(key is TupleKeyAttribute))
                RequireAttribute(nameof(TupleKeyAttribute), nameof(TupleObjectAttribute), property);

            return attribute == null || key != null
                ? (property, key, converter)
                : default;
        }
    }
}
