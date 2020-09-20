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
            var propertyWithAttributes = new List<(PropertyInfo Property, Attribute Key, Attribute ConverterOrCreator)>();
            foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(x => x.GetGetMethod()?.GetParameters().Length == 0).OrderBy(x => x.Name))
            {
                var key = GetAttribute(property, a => a is NamedKeyAttribute || a is TupleKeyAttribute);
                var any = GetAttribute(property, a => a is ConverterAttribute || a is ConverterCreatorAttribute);
                if (key is null && any != null)
                    throw new ArgumentException($"Require '{nameof(NamedKeyAttribute)}' or '{nameof(TupleKeyAttribute)}' for '{any.GetType().Name}', property name: {property.Name}, type: {type}");
                propertyWithAttributes.Add((property, key, any));
            }

            var attribute = GetAttribute(type, a => a is NamedObjectAttribute || a is TupleObjectAttribute || a is ConverterAttribute || a is ConverterCreatorAttribute);
            if (propertyWithAttributes.Count == 0 && (attribute is ConverterAttribute || attribute is ConverterCreatorAttribute) is false)
                throw new ArgumentException($"No available property found, type: {type}");

            var propertyWithNamedKeyAttributes = propertyWithAttributes.Where(x => x.Key is NamedKeyAttribute).Select(x => (x.Property, Key: (NamedKeyAttribute)x.Key, x.ConverterOrCreator)).ToList();
            var propertyWithTupleKeyAttributes = propertyWithAttributes.Where(x => x.Key is TupleKeyAttribute).Select(x => (x.Property, Key: (TupleKeyAttribute)x.Key, x.ConverterOrCreator)).ToList();
            if (propertyWithNamedKeyAttributes.Count == 0 && attribute is NamedObjectAttribute)
                throw new ArgumentException($"Require '{nameof(NamedKeyAttribute)}' for '{nameof(NamedObjectAttribute)}', type: {type}");
            if (propertyWithTupleKeyAttributes.Count == 0 && attribute is TupleObjectAttribute)
                throw new ArgumentException($"Require '{nameof(TupleKeyAttribute)}' for '{nameof(TupleObjectAttribute)}', type: {type}");
            if (propertyWithNamedKeyAttributes.Count != 0 && attribute is NamedObjectAttribute is false)
                throw new ArgumentException($"Require '{nameof(NamedObjectAttribute)}' for '{nameof(NamedKeyAttribute)}', property name: {propertyWithNamedKeyAttributes.First().Property.Name}, type: {type}");
            if (propertyWithTupleKeyAttributes.Count != 0 && attribute is TupleObjectAttribute is false)
                throw new ArgumentException($"Require '{nameof(TupleObjectAttribute)}' for '{nameof(TupleKeyAttribute)}', property name: {propertyWithTupleKeyAttributes.First().Property.Name}, type: {type}");

            if (attribute is ConverterAttribute || attribute is ConverterCreatorAttribute)
                return GetConverter(context, type, attribute);

            var names = default(IReadOnlyList<string>);
            var properties = default(IReadOnlyList<PropertyInfo>);
            if (attribute is NamedObjectAttribute)
                properties = GetSortedProperties(type, propertyWithNamedKeyAttributes.Select(x => (x.Property, x.Key)).ToList(), out names);
            else if (attribute is TupleObjectAttribute)
                properties = GetSortedProperties(type, propertyWithTupleKeyAttributes.Select(x => (x.Property, x.Key)).ToList());
            else
                properties = propertyWithAttributes.Select(x => x.Property).ToList();

            var constructor = GetConstructor(type, properties);
            var propertyWithConverters = propertyWithAttributes.ToDictionary(x => x.Property, x => GetConverter(context, x.Property.PropertyType, x.ConverterOrCreator));
            var converters = properties.Select(x => propertyWithConverters[x]).ToList();

            if (attribute is TupleObjectAttribute)
                return ContextMethodsOfTupleObject.GetConverterAsTupleObject(type, constructor, converters, properties.Select(x => x.PropertyType).ToList(), ContextMethods.GetMemberInitializers(properties));

            var encoder = (Converter<string>)context.GetConverter(typeof(string));
            if (names is null)
                names = properties.Select(x => x.Name).ToList();
            return ContextMethodsOfNamedObject.GetConverterAsNamedObject(type, constructor, converters, properties, names, encoder);
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

        private static IReadOnlyList<PropertyInfo> GetSortedProperties(Type type, IReadOnlyCollection<(PropertyInfo, NamedKeyAttribute)> collection, out IReadOnlyList<string> names)
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
            names = map.Keys.ToList();
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

        private static ContextObjectConstructor GetConstructor(Type type, IReadOnlyList<PropertyInfo> properties)
        {
            Debug.Assert(properties.Any());
            if (type.IsAbstract || type.IsInterface)
                return null;
            if ((type.IsValueType || type.GetConstructor(Type.EmptyTypes) != null) && properties.All(x => x.GetSetMethod() != null))
                return (delegateType, initializer) => ContextMethods.GetDecodeDelegate(delegateType, initializer, ContextMethods.GetMemberInitializers(properties));

            var selector = new Func<PropertyInfo, string>(x => x.Name.ToUpperInvariant());
            if (properties.Select(selector).Distinct().Count() != properties.Count)
                return null;

            var dictionary = properties.ToDictionary(selector);
            var collection = new List<(ConstructorInfo, IReadOnlyList<PropertyInfo>, IReadOnlyList<PropertyInfo> Properties)>();
            foreach (var i in type.GetConstructors())
            {
                var parameters = (IReadOnlyList<ParameterInfo>)i.GetParameters();
                var result = parameters
                    .Select(x => dictionary.TryGetValue(x.Name.ToUpperInvariant(), out var property) && property.PropertyType == x.ParameterType ? property : null)
                    .Where(x => x != null)
                    .ToList();
                if (result.Count == 0 || result.Count != parameters.Count)
                    continue;
                var except = properties.Except(result).ToList();
                if (except.Any(x => x.GetSetMethod() is null))
                    continue;
                collection.Add((i, result, except));
            }

            if (collection.Count == 0)
                return null;
            var constructorOnlyResults = collection.Where(x => x.Properties.Count == 0).ToList();
            var constructorWithMembers = collection.Where(x => x.Properties.Count != 0).ToList();
            if (constructorOnlyResults.Count > 1 || (constructorOnlyResults.Count == 0 && constructorWithMembers.Count > 1))
                throw new ArgumentException($"Multiple suitable constructors found, type: {type}");
            var (constructor, objectProperties, memberProperties) = constructorOnlyResults.Any()
                ? constructorOnlyResults.Single()
                : constructorWithMembers.Single();
            var content = properties.Select((x, i) => (Key: x, Value: i)).ToDictionary(x => x.Key, x => x.Value);
            var objectIndexes = objectProperties.Select(x => content[x]).ToList();
            var memberIndexes = memberProperties.Select(x => content[x]).ToList();
            Debug.Assert(properties.Count == objectIndexes.Count + memberIndexes.Count);
            return (delegateType, initializer) => ContextMethods.GetDecodeDelegate(delegateType, initializer, constructor, objectIndexes, ContextMethods.GetMemberInitializers(memberProperties), memberIndexes);
        }
    }
}
