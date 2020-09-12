using Mikodev.Binary.Attributes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Mikodev.Binary.Internal.Contexts
{
    internal static class FallbackAttributesMethods
    {
        internal static IConverter GetConverter(IGeneratorContext context, Type type)
        {
            var properties = new List<(PropertyInfo Property, Attribute Key, Attribute ConverterOrCreator)>();
            foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(x => x.GetGetMethod()?.GetParameters().Length == 0).OrderBy(x => x.Name))
            {
                var key = GetAttribute(property, a => a is NamedKeyAttribute || a is TupleKeyAttribute);
                var any = GetAttribute(property, a => a is ConverterAttribute || a is ConverterCreatorAttribute);
                if (key is null && any != null)
                    throw new ArgumentException($"Require '{nameof(NamedKeyAttribute)}' or '{nameof(TupleKeyAttribute)}' for '{any.GetType().Name}', property name: {property.Name}, type: {type}");
                properties.Add((property, key, any));
            }

            var attribute = GetAttribute(type, a => a is NamedObjectAttribute || a is TupleObjectAttribute || a is ConverterAttribute || a is ConverterCreatorAttribute);
            if (properties.Any() is false && (attribute is ConverterAttribute || attribute is ConverterCreatorAttribute) is false)
                throw new ArgumentException($"No available property found, type: {type}");

            var propertyWithNamedKeyAttributes = properties.Where(x => x.Key is NamedKeyAttribute).Select(x => (x.Property, Key: (NamedKeyAttribute)x.Key, x.ConverterOrCreator)).ToList();
            var propertyWithTupleKeyAttributes = properties.Where(x => x.Key is TupleKeyAttribute).Select(x => (x.Property, Key: (TupleKeyAttribute)x.Key, x.ConverterOrCreator)).ToList();
            if (propertyWithNamedKeyAttributes.Count == 0 && attribute is NamedObjectAttribute)
                throw new ArgumentException($"Require '{nameof(NamedKeyAttribute)}' for '{nameof(NamedObjectAttribute)}', type: {type}");
            if (propertyWithTupleKeyAttributes.Count == 0 && attribute is TupleObjectAttribute)
                throw new ArgumentException($"Require '{nameof(TupleKeyAttribute)}' for '{nameof(TupleObjectAttribute)}', type: {type}");
            if (propertyWithNamedKeyAttributes.Count != 0 && !(attribute is NamedObjectAttribute))
                throw new ArgumentException($"Require '{nameof(NamedObjectAttribute)}' for '{nameof(NamedKeyAttribute)}', property name: {propertyWithNamedKeyAttributes.First().Property.Name}, type: {type}");
            if (propertyWithTupleKeyAttributes.Count != 0 && !(attribute is TupleObjectAttribute))
                throw new ArgumentException($"Require '{nameof(TupleObjectAttribute)}' for '{nameof(TupleKeyAttribute)}', property name: {propertyWithTupleKeyAttributes.First().Property.Name}, type: {type}");

            if (attribute is ConverterAttribute || attribute is ConverterCreatorAttribute)
                return GetConverter(context, type, attribute);

            var names = default(IReadOnlyDictionary<PropertyInfo, string>);
            var sortedProperties = default(IReadOnlyList<PropertyInfo>);
            if (attribute is NamedObjectAttribute)
                sortedProperties = GetSortedProperties(type, propertyWithNamedKeyAttributes.Select(x => (x.Property, x.Key)).ToList(), out names);
            else if (attribute is TupleObjectAttribute)
                sortedProperties = GetSortedProperties(type, propertyWithTupleKeyAttributes.Select(x => (x.Property, x.Key)).ToList());
            else
                sortedProperties = properties.Select(x => x.Property).ToList();

            var constructor = GetConstructor(type, sortedProperties, out var indexes);
            var propertyWithConverters = properties.ToDictionary(x => x.Property, x => GetConverter(context, x.Property.PropertyType, x.ConverterOrCreator));
            var sortedConverters = sortedProperties.Select(x => propertyWithConverters[x]).ToList();

            if (attribute is TupleObjectAttribute)
                return ContextMethodsOfTupleObject.GetConverterAsTupleObject(type, constructor, indexes, sortedConverters, sortedProperties, null, null);

            var encoder = (Converter<string>)context.GetConverter(typeof(string));
            if (names is null)
                names = sortedProperties.ToDictionary(x => x, x => x.Name);
            return ContextMethodsOfNamedObject.GetConverterAsNamedObject(type, constructor, indexes, sortedConverters, sortedProperties, names, encoder);
        }

        private static Exception GetInstance<T>(Type type, out T result) where T : class
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

        private static Attribute GetAttribute(MemberInfo member, Func<Attribute, bool> filter)
        {
            Debug.Assert(member is Type || member is PropertyInfo);
            var attributes = member.GetCustomAttributes(false).OfType<Attribute>().Where(filter).ToList();
            if (attributes.Count <= 1)
                return attributes.SingleOrDefault();
            var message = member is PropertyInfo property
                ? $"Multiple attributes found, property name: {property.Name}, type: {property.ReflectedType}"
                : $"Multiple attributes found, type: {(Type)member}";
            throw new ArgumentException(message);
        }

        private static IConverter GetConverter(IGeneratorContext context, Type type, Attribute attribute)
        {
            Debug.Assert(attribute is null || attribute is ConverterAttribute || attribute is ConverterCreatorAttribute);
            if (attribute is ConverterAttribute alpha)
                if (GetInstance<IConverter>(alpha.Type, out var converter) is { } exception)
                    throw new ArgumentException($"Can not get custom converter via attribute, expected converter type: {typeof(Converter<>).MakeGenericType(type)}", exception);
                else
                    return ContextMethods.EnsureConverter(converter, type);
            if (attribute is ConverterCreatorAttribute bravo)
                if (GetInstance<IConverterCreator>(bravo.Type, out var creator) is { } exception)
                    throw new ArgumentException($"Can not get custom converter creator via attribute, expected converter type: {typeof(Converter<>).MakeGenericType(type)}", exception);
                else
                    return ContextMethods.EnsureConverter(creator.GetConverter(context, type), type, bravo.Type);
            return context.GetConverter(type);
        }

        private static IReadOnlyList<PropertyInfo> GetSortedProperties(Type type, IReadOnlyCollection<(PropertyInfo, NamedKeyAttribute)> collection, out IReadOnlyDictionary<PropertyInfo, string> dictionary)
        {
            Debug.Assert(collection.Any());
            var map = new SortedDictionary<string, PropertyInfo>();
            foreach (var (property, attribute) in collection)
            {
                var key = attribute.Key;
                if (string.IsNullOrEmpty(key))
                    throw new ArgumentException($"Named key can not be null or empty, property name: {property.Name}, type: {type}");
                if (map.ContainsKey(key))
                    throw new ArgumentException($"Named key '{key}' already exists, property name: {property.Name}, type: {type}");
                map.Add(key, property);
            }
            dictionary = map.ToDictionary(x => x.Value, x => x.Key);
            return map.Values.ToList();
        }

        private static IReadOnlyList<PropertyInfo> GetSortedProperties(Type type, IReadOnlyCollection<(PropertyInfo, TupleKeyAttribute)> collection)
        {
            Debug.Assert(collection.Any());
            var map = new SortedDictionary<int, PropertyInfo>();
            foreach (var (property, attribute) in collection)
            {
                var key = attribute.Key;
                if (map.ContainsKey(key))
                    throw new ArgumentException($"Tuple key '{key}' already exists, property name: {property.Name}, type: {type}");
                map.Add(key, property);
            }
            var keys = map.Keys.ToList();
            if (keys.First() != 0 || keys.Last() != keys.Count - 1)
                throw new ArgumentException($"Tuple key must be start at zero and must be sequential, type: {type}");
            return map.Values.ToList();
        }

        private static ConstructorInfo GetConstructor(Type type, IReadOnlyCollection<PropertyInfo> properties, out IReadOnlyList<int> indexes)
        {
            Debug.Assert(properties.Any());
            indexes = null;
            if (type.IsAbstract || type.IsInterface)
                return null;
            var selector = new Func<PropertyInfo, string>(x => x.Name.ToUpperInvariant());
            if (properties.Select(selector).Distinct().Count() != properties.Count)
                return null;

            var dictionary = properties.ToDictionary(selector);
            var collection = new List<(ConstructorInfo, IReadOnlyList<PropertyInfo>)>();
            foreach (var i in type.GetConstructors())
            {
                var parameters = (IReadOnlyList<ParameterInfo>)i.GetParameters();
                if (parameters.Count != dictionary.Count)
                    continue;
                var result = parameters
                    .Select(x => dictionary.TryGetValue(x.Name.ToUpperInvariant(), out var property) && property.PropertyType == x.ParameterType ? property : null)
                    .Where(x => x != null)
                    .ToList();
                if (parameters.Count != result.Count)
                    continue;
                collection.Add((i, result));
            }

            if (collection.Count == 0)
                return null;
            if (collection.Count != 1)
                throw new ArgumentException($"Multiple suitable constructors found, type: {type}");
            var (constructor, second) = collection.Single();
            var orders = properties.Select((x, i) => (Key: x, Value: i)).ToDictionary(x => x.Key, x => x.Value);
            indexes = second.Select(x => orders[x]).ToArray();
            Debug.Assert(properties.Count == indexes.Count);
            return constructor;
        }
    }
}
