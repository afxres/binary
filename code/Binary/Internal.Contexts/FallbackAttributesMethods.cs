namespace Mikodev.Binary.Internal.Contexts;

using Mikodev.Binary.Attributes;
using Mikodev.Binary.External;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

internal static class FallbackAttributesMethods
{
    [RequiresUnreferencedCode(CommonModule.RequiresUnreferencedCodeMessage)]
    internal static IConverter GetConverter(IGeneratorContext context, Type type)
    {
        var attributes = GetAttributes(type, a => a is NamedObjectAttribute or TupleObjectAttribute or ConverterAttribute or ConverterCreatorAttribute);
        if (attributes.Length > 1)
            throw new ArgumentException($"Multiple attributes found, type: {type}");

        var attribute = attributes.FirstOrDefault();
        SetMemberVariables(type, attribute, out var keys, out var conversions, out var annotations);
        Debug.Assert(keys.Count == annotations.Count);
        Debug.Assert(keys.Count == conversions.Count);

        if (attribute is ConverterAttribute or ConverterCreatorAttribute)
            return GetConverter(context, type, null, attribute);
        if (keys.Count is 0)
            throw new ArgumentException($"No available property found, type: {type}");

        var properties = GetSortedMembers(type, attribute, keys, out var names);
        var converters = properties.Select(x => GetConverter(context, type, x, conversions[x])).ToImmutableArray();
        var constructor = GetConstructor(type, properties);

        if (attribute is TupleObjectAttribute)
            return ContextMethodsOfTupleObject.GetConverterAsTupleObject(type, constructor, converters, ContextMethods.GetMemberInitializers(properties));

        var required = properties.Select(x => annotations[x]).ToImmutableArray();
        var instance = (Converter<string>)context.GetConverter(typeof(string));
        var memories = names.Select(x => new ReadOnlyMemory<byte>(instance.Encode(x))).ToImmutableArray();
        var dictionary = BinaryObject.Create(memories);
        if (dictionary is null)
            throw new ArgumentException($"Named object error, duplicate binary string keys detected, type: {type}, string converter type: {instance.GetType()}");
        return ContextMethodsOfNamedObject.GetConverterAsNamedObject(type, constructor, converters, properties, names, memories, required, dictionary);
    }

    private static void SetMemberVariables([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] Type type, Attribute? attribute, out ImmutableDictionary<PropertyInfo, Attribute?> keys, out ImmutableDictionary<PropertyInfo, Attribute?> conversions, out ImmutableDictionary<PropertyInfo, bool> annotations)
    {
        var requiredType = GetAttributes(type, x => x is RequiredMemberAttribute).Any();
        bool IsRequiredOrDefault(PropertyInfo property, Attribute? key)
        {
            if (requiredType is false)
                return true;
            var requiredProperty = GetAttributes(property, x => x is RequiredMemberAttribute).Any();
            if (requiredProperty is false)
                return false;
            if (attribute is NamedObjectAttribute && key is not NamedKeyAttribute)
                throw new ArgumentException($"Require '{nameof(NamedKeyAttribute)}' for required member, property name: {property.Name}, type: {type}");
            if (attribute is TupleObjectAttribute && key is not TupleKeyAttribute)
                throw new ArgumentException($"Require '{nameof(TupleKeyAttribute)}' for required member, property name: {property.Name}, type: {type}");
            return true;
        }

        var builderOfKeys = ImmutableDictionary.CreateBuilder<PropertyInfo, Attribute?>();
        var builderOfConversions = ImmutableDictionary.CreateBuilder<PropertyInfo, Attribute?>();
        var builderOfAnnotations = ImmutableDictionary.CreateBuilder<PropertyInfo, bool>();
        foreach (var property in type.GetProperties(CommonModule.PublicInstanceBindingFlags))
        {
            var keyAttributes = GetAttributes(property, a => a is NamedKeyAttribute or TupleKeyAttribute);
            var key = keyAttributes.FirstOrDefault();
            var conversionAttributes = GetAttributes(property, a => a is ConverterAttribute or ConverterCreatorAttribute);
            var conversion = conversionAttributes.FirstOrDefault();
            var indexer = property.GetIndexParameters().Any();
            if (indexer && (key ?? conversion) is { } result)
                throw new ArgumentException($"Can not apply '{result.GetType().Name}' to an indexer, type: {type}");
            if (indexer)
                continue;
            if (property.GetGetMethod() is null)
                throw new ArgumentException($"No available getter found, property name: {property.Name}, type: {type}");
            if (keyAttributes.Length > 1 || conversionAttributes.Length > 1)
                throw new ArgumentException($"Multiple attributes found, property name: {property.Name}, type: {type}");
            if (key is null && conversion is not null)
                throw new ArgumentException($"Require '{nameof(NamedKeyAttribute)}' or '{nameof(TupleKeyAttribute)}' for '{conversion.GetType().Name}', property name: {property.Name}, type: {type}");
            if (key is NamedKeyAttribute && attribute is not NamedObjectAttribute)
                throw new ArgumentException($"Require '{nameof(NamedObjectAttribute)}' for '{nameof(NamedKeyAttribute)}', property name: {property.Name}, type: {type}");
            if (key is TupleKeyAttribute && attribute is not TupleObjectAttribute)
                throw new ArgumentException($"Require '{nameof(TupleObjectAttribute)}' for '{nameof(TupleKeyAttribute)}', property name: {property.Name}, type: {type}");
            var required = IsRequiredOrDefault(property, key);
            builderOfKeys.Add(property, key);
            builderOfConversions.Add(property, conversion);
            builderOfAnnotations.Add(property, required);
        }
        keys = builderOfKeys.ToImmutable();
        conversions = builderOfConversions.ToImmutable();
        annotations = builderOfAnnotations.ToImmutable();
    }

    private static ImmutableArray<Attribute> GetAttributes(MemberInfo member, Func<Attribute, bool> filter)
    {
        Debug.Assert(member is Type or PropertyInfo);
        var attributes = member.GetCustomAttributes(false).OfType<Attribute>().Where(filter).ToImmutableArray();
        return attributes;
    }

    private static T GetConverterOrCreator<T>([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type instance, Type reflected, PropertyInfo? property)
    {
        try
        {
            return (T)CommonModule.CreateInstance(instance, null);
        }
        catch (Exception e)
        {
            var prefix = typeof(T) == typeof(IConverter)
                ? "converter"
                : "converter creator";
            var suffix = property is null
                ? $"type: {reflected}"
                : $"property name: {property.Name}, type: {reflected}";
            throw new ArgumentException($"Can not get custom {prefix} via attribute, {suffix}", e);
        }
    }

    [RequiresUnreferencedCode(CommonModule.RequiresUnreferencedCodeMessage)]
    private static IConverter GetConverter(IGeneratorContext context, Type reflected, PropertyInfo? property, Attribute? attribute)
    {
        var type = property is null ? reflected : property.PropertyType;
        Debug.Assert(attribute is null or ConverterAttribute or ConverterCreatorAttribute);
        if (attribute is ConverterAttribute alpha)
            return CommonModule.GetConverter(GetConverterOrCreator<IConverter>(alpha.Type, reflected, property), type, null);
        if (attribute is ConverterCreatorAttribute bravo)
            return CommonModule.GetConverter(GetConverterOrCreator<IConverterCreator>(bravo.Type, reflected, property).GetConverter(context, type), type, bravo.Type);
        Debug.Assert(attribute is null);
        return context.GetConverter(type);
    }

    private static ImmutableArray<PropertyInfo> GetSortedMembers(Type type, Attribute? attribute, ImmutableDictionary<PropertyInfo, Attribute?> source, out ImmutableArray<string> list)
    {
        ImmutableDictionary<PropertyInfo, T> Choose<T>() where T : class
        {
            var builder = ImmutableDictionary.CreateBuilder<PropertyInfo, T>();
            foreach (var (property, attribute) in source)
                if (attribute is T result)
                    builder.Add(property, result);
            return builder.ToImmutable();
        }

        var named = Choose<NamedKeyAttribute>();
        var tuple = Choose<TupleKeyAttribute>();
        if (named.Count is 0 && attribute is NamedObjectAttribute)
            throw new ArgumentException($"Require '{nameof(NamedKeyAttribute)}' for '{nameof(NamedObjectAttribute)}', type: {type}");
        if (tuple.Count is 0 && attribute is TupleObjectAttribute)
            throw new ArgumentException($"Require '{nameof(TupleKeyAttribute)}' for '{nameof(TupleObjectAttribute)}', type: {type}");

        list = default;
        if (attribute is TupleObjectAttribute)
            return GetSortedMembers(type, tuple);
        if (attribute is NamedObjectAttribute)
            return GetSortedMembers(type, named, out list);
        var result = source.Select(x => x.Key).OrderBy(x => x.Name).ToImmutableArray();
        list = result.Select(x => x.Name).ToImmutableArray();
        return result;
    }

    private static ImmutableArray<PropertyInfo> GetSortedMembers(Type type, ImmutableDictionary<PropertyInfo, NamedKeyAttribute> collection, out ImmutableArray<string> list)
    {
        Debug.Assert(collection.Any());
        var map = new SortedDictionary<string, PropertyInfo>();
        foreach (var (property, attribute) in collection)
        {
            var key = attribute.Key;
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException($"Named key can not be null or empty, property name: {property.Name}, type: {type}");
            if (map.ContainsKey(key))
                throw new ArgumentException($"Named key '{key}' already exists, type: {type}");
            map.Add(key, property);
        }
        list = map.Keys.ToImmutableArray();
        return map.Values.ToImmutableArray();
    }

    private static ImmutableArray<PropertyInfo> GetSortedMembers(Type type, ImmutableDictionary<PropertyInfo, TupleKeyAttribute> collection)
    {
        Debug.Assert(collection.Any());
        var map = new SortedDictionary<int, PropertyInfo>();
        foreach (var (property, attribute) in collection)
        {
            var key = attribute.Key;
            if (map.ContainsKey(key))
                throw new ArgumentException($"Tuple key '{key}' already exists, type: {type}");
            map.Add(key, property);
        }
        var keys = map.Keys.ToList();
        if (keys.First() is not 0 || keys.Last() != keys.Count - 1)
            throw new ArgumentException($"Tuple key must be start at zero and must be sequential, type: {type}");
        return map.Values.ToImmutableArray();
    }

    [RequiresUnreferencedCode(CommonModule.RequiresUnreferencedCodeMessage)]
    private static ContextObjectConstructor? GetConstructor(Type type, ImmutableArray<PropertyInfo> properties)
    {
        Debug.Assert(properties.Any());
        if (type.IsAbstract || type.IsInterface)
            return null;
        if ((type.IsValueType || type.GetConstructor(Type.EmptyTypes) is not null) && properties.All(x => x.GetSetMethod() is not null))
            return (delegateType, initializer) => ContextMethods.GetDecodeDelegate(delegateType, initializer, ContextMethods.GetMemberInitializers(properties));

        static string Select(string? text) =>
            text?.ToUpperInvariant() ??
            string.Empty;

        var selector = new Func<PropertyInfo, string>(x => Select(x.Name));
        if (properties.Select(selector).Distinct().Count() != properties.Length)
            return null;

        var dictionary = properties.ToDictionary(selector);
        var collection = new List<(ConstructorInfo, ImmutableArray<PropertyInfo>, ImmutableArray<PropertyInfo> Properties)>();
        foreach (var i in type.GetConstructors())
        {
            var parameters = i.GetParameters();
            var result = parameters
                .Select(x => dictionary.TryGetValue(Select(x.Name), out var property) && property.PropertyType == x.ParameterType ? property : null)
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
