﻿namespace Mikodev.Binary.Internal.Contexts;

using Mikodev.Binary.Attributes;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

internal static class FallbackAttributesMethods
{
    internal static IConverter GetConverter(IGeneratorContext context, Type type)
    {
        var attributes = GetAttributes(type, a => a is NamedObjectAttribute or TupleObjectAttribute or ConverterAttribute or ConverterCreatorAttribute);
        if (attributes.Length > 1)
            throw new ArgumentException($"Multiple attributes found, type: {type}");

        var propertyWithAttributes = new List<(PropertyInfo Property, Attribute? Key, Attribute? ConverterOrCreator)>();
        foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public).OrderBy(x => x.Name))
        {
            var keys = GetAttributes(property, a => a is NamedKeyAttribute or TupleKeyAttribute);
            var list = GetAttributes(property, a => a is ConverterAttribute or ConverterCreatorAttribute);
            var head = keys.FirstOrDefault();
            var tail = list.FirstOrDefault();
            var indexer = property.GetIndexParameters().Any();
            if (indexer && (head ?? tail) is { } result)
                throw new ArgumentException($"Can not apply '{result.GetType().Name}' to an indexer, type: {type}");
            if (indexer)
                continue;
            if (property.GetGetMethod() is null)
                throw new ArgumentException($"No available getter found, property name: {property.Name}, type: {type}");
            if (keys.Length > 1 || list.Length > 1)
                throw new ArgumentException($"Multiple attributes found, property name: {property.Name}, type: {type}");
            if (head is null && tail is not null)
                throw new ArgumentException($"Require '{nameof(NamedKeyAttribute)}' or '{nameof(TupleKeyAttribute)}' for '{tail.GetType().Name}', property name: {property.Name}, type: {type}");
            propertyWithAttributes.Add((property, head, tail));
        }

        var attribute = attributes.FirstOrDefault();
        if (propertyWithAttributes.Count is 0 && attribute is not ConverterAttribute && attribute is not ConverterCreatorAttribute)
            throw new ArgumentException($"No available property found, type: {type}");

        static IEnumerable<R> Choose<T, U, R>(IEnumerable<T> source, Func<T, U?> filter, Func<T, U, R> result) =>
            from i in source
            let x = filter.Invoke(i)
            where x is not null
            select result.Invoke(i, x);

        var propertyWithNamedKeyAttributes = Choose(propertyWithAttributes, x => x.Key as NamedKeyAttribute, (a, b) => (a.Property, Key: b)).ToList();
        var propertyWithTupleKeyAttributes = Choose(propertyWithAttributes, x => x.Key as TupleKeyAttribute, (a, b) => (a.Property, Key: b)).ToList();
        if (propertyWithNamedKeyAttributes.Count is 0 && attribute is NamedObjectAttribute)
            throw new ArgumentException($"Require '{nameof(NamedKeyAttribute)}' for '{nameof(NamedObjectAttribute)}', type: {type}");
        if (propertyWithTupleKeyAttributes.Count is 0 && attribute is TupleObjectAttribute)
            throw new ArgumentException($"Require '{nameof(TupleKeyAttribute)}' for '{nameof(TupleObjectAttribute)}', type: {type}");
        if (propertyWithNamedKeyAttributes.FirstOrDefault().Property is { } alpha && attribute is NamedObjectAttribute is false)
            throw new ArgumentException($"Require '{nameof(NamedObjectAttribute)}' for '{nameof(NamedKeyAttribute)}', property name: {alpha.Name}, type: {type}");
        if (propertyWithTupleKeyAttributes.FirstOrDefault().Property is { } bravo && attribute is TupleObjectAttribute is false)
            throw new ArgumentException($"Require '{nameof(TupleObjectAttribute)}' for '{nameof(TupleKeyAttribute)}', property name: {bravo.Name}, type: {type}");

        if (attribute is ConverterAttribute or ConverterCreatorAttribute)
            return GetConverter(context, type, attribute);

        var names = default(ImmutableArray<string>);
        var properties = attribute switch
        {
            NamedObjectAttribute => GetSortedProperties(type, propertyWithNamedKeyAttributes.Select(x => (x.Property, x.Key)).ToImmutableArray(), out names),
            TupleObjectAttribute => GetSortedProperties(type, propertyWithTupleKeyAttributes.Select(x => (x.Property, x.Key)).ToImmutableArray()),
            _ => propertyWithAttributes.Select(x => x.Property).ToImmutableArray(),
        };

        var constructor = GetConstructor(type, properties);
        var propertyWithConverters = propertyWithAttributes.ToDictionary(x => x.Property, x => GetConverter(context, x.Property.PropertyType, x.ConverterOrCreator));
        var converters = properties.Select(x => propertyWithConverters[x]).ToImmutableArray();

        if (attribute is TupleObjectAttribute)
            return ContextMethodsOfTupleObject.GetConverterAsTupleObject(type, constructor, converters, ContextMethods.GetMemberInitializers(properties));

        var encoder = (Converter<string>)context.GetConverter(typeof(string));
        if (names.IsDefault)
            names = properties.Select(x => x.Name).ToImmutableArray();
        return ContextMethodsOfNamedObject.GetConverterAsNamedObject(type, constructor, converters, properties, names, encoder);
    }

    private static ImmutableArray<Attribute> GetAttributes(MemberInfo member, Func<Attribute, bool> filter)
    {
        Debug.Assert(member is Type or PropertyInfo);
        var attributes = member.GetCustomAttributes(false).OfType<Attribute>().Where(filter).ToImmutableArray();
        return attributes;
    }

    private static T GetInstance<T>(Type instance, Type item, Func<Type, string> message)
    {
        try
        {
            return (T)CommonHelper.CreateInstance(instance, null);
        }
        catch (Exception e)
        {
            throw new ArgumentException(message.Invoke(typeof(Converter<>).MakeGenericType(item)), e);
        }
    }

    private static IConverter GetConverter(IGeneratorContext context, Type type, Attribute? attribute)
    {
        var x = (Type t) => $"Can not get custom converter via attribute, expected converter type: {t}";
        var y = (Type t) => $"Can not get custom converter creator via attribute, expected converter type: {t}";

        Debug.Assert(attribute is null or ConverterAttribute or ConverterCreatorAttribute);
        return attribute switch
        {
            ConverterAttribute alpha => CommonHelper.GetConverter(GetInstance<IConverter>(alpha.Type, type, x), type),
            ConverterCreatorAttribute bravo => CommonHelper.GetConverter(GetInstance<IConverterCreator>(bravo.Type, type, y).GetConverter(context, type), type, bravo.Type),
            _ => context.GetConverter(type),
        };
    }

    private static ImmutableArray<PropertyInfo> GetSortedProperties(Type type, ImmutableArray<(PropertyInfo, NamedKeyAttribute)> collection, out ImmutableArray<string> names)
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
        names = map.Keys.ToImmutableArray();
        return map.Values.ToImmutableArray();
    }

    private static ImmutableArray<PropertyInfo> GetSortedProperties(Type type, ImmutableArray<(PropertyInfo, TupleKeyAttribute)> collection)
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
        if (keys.First() is not 0 || keys.Last() != keys.Count - 1)
            throw new ArgumentException($"Tuple key must be start at zero and must be sequential, type: {type}");
        return map.Values.ToImmutableArray();
    }

    private static ContextObjectConstructor? GetConstructor(Type type, ImmutableArray<PropertyInfo> properties)
    {
        static string MakeUpperCaseInvariant(string? text) => string.IsNullOrEmpty(text) ? string.Empty : text.ToUpperInvariant();

        Debug.Assert(properties.Any());
        if (type.IsAbstract || type.IsInterface)
            return null;
        if ((type.IsValueType || type.GetConstructor(Type.EmptyTypes) is not null) && properties.All(x => x.GetSetMethod() is not null))
            return (delegateType, initializer) => ContextMethods.GetDecodeDelegate(delegateType, initializer, ContextMethods.GetMemberInitializers(properties));

        var selector = new Func<PropertyInfo, string>(x => MakeUpperCaseInvariant(x.Name));
        if (properties.Select(selector).Distinct().Count() != properties.Length)
            return null;

        var dictionary = properties.ToDictionary(selector);
        var collection = new List<(ConstructorInfo, ImmutableArray<PropertyInfo>, ImmutableArray<PropertyInfo> Properties)>();
        foreach (var i in type.GetConstructors())
        {
            var parameters = i.GetParameters();
            var result = parameters
                .Select(x => dictionary.TryGetValue(MakeUpperCaseInvariant(x.Name), out var property) && property.PropertyType == x.ParameterType ? property : null)
                .OfType<PropertyInfo>()
                .ToImmutableArray();
            if (result.Length is 0 || result.Length != parameters.Length)
                continue;
            var except = properties.Except(result).ToImmutableArray();
            if (except.Any(x => x.GetSetMethod() is null))
                continue;
            collection.Add((i, result, except));
        }

        if (collection.Count is 0)
            return null;
        var constructorOnlyResults = collection.Where(x => x.Properties.Length is 0).ToImmutableArray();
        var constructorWithMembers = collection.Where(x => x.Properties.Length is not 0).ToImmutableArray();
        if (constructorOnlyResults.Length > 1 || (constructorOnlyResults.Length is 0 && constructorWithMembers.Length > 1))
            throw new ArgumentException($"Multiple suitable constructors found, type: {type}");
        var (constructor, objectProperties, memberProperties) = constructorOnlyResults.Any()
            ? constructorOnlyResults.Single()
            : constructorWithMembers.Single();
        var content = properties.Select((x, i) => (Key: x, Value: i)).ToDictionary(x => x.Key, x => x.Value);
        var objectIndexes = objectProperties.Select(x => content[x]).ToImmutableArray();
        var memberIndexes = memberProperties.Select(x => content[x]).ToImmutableArray();
        Debug.Assert(properties.Length == objectIndexes.Length + memberIndexes.Length);
        return (delegateType, initializer) => ContextMethods.GetDecodeDelegate(delegateType, initializer, constructor, objectIndexes, ContextMethods.GetMemberInitializers(memberProperties), memberIndexes);
    }
}
