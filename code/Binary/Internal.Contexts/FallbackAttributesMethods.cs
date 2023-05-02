namespace Mikodev.Binary.Internal.Contexts;

using Mikodev.Binary.Attributes;
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
        var required = GetAttributes(type, x => x is RequiredMemberAttribute).Any();
        var typeInfo = new MetaTypeInfo(type, attribute, required);
        var memberInfoArrayUnsorted = GetMemberVariables(typeInfo);

        if (attribute is ConverterAttribute or ConverterCreatorAttribute)
            return GetConverter(context, type, null, attribute);
        if (memberInfoArrayUnsorted.Length is 0)
            throw new ArgumentException($"No available member found, type: {type}");
        if (memberInfoArrayUnsorted.All(x => x.IsOptional))
            throw new ArgumentException($"No available required member found, type: {type}");

        var members = GetSortedMembers(typeInfo, memberInfoArrayUnsorted, out var names);
        var converters = members.Select(x => GetConverter(context, type, x, x.ConverterOrConverterCreatorAttribute)).ToImmutableArray();
        var initializers = members.Select(x => x.Initializer).ToImmutableArray();
        var constructor = GetConstructor(type, members, initializers);

        if (attribute is TupleObjectAttribute)
            return ContextMethodsOfTupleObject.GetConverterAsTupleObject(type, constructor, converters, initializers);

        var optional = members.Select(x => x.IsOptional).ToImmutableArray();
        var encoding = (Converter<string>)context.GetConverter(typeof(string));
        return ContextMethodsOfNamedObject.GetConverterAsNamedObject(type, constructor, converters, initializers, names, optional, encoding);
    }

    [RequiresUnreferencedCode(CommonModule.RequiresUnreferencedCodeMessage)]
    private static ImmutableArray<MetaMemberInfo> GetMemberVariables(MetaTypeInfo typeInfo)
    {
        var type = typeInfo.Type;
        var result = ImmutableArray.CreateBuilder<MetaMemberInfo>();
        foreach (var member in type.GetMembers(CommonModule.PublicInstanceBindingFlags))
        {
            if (member is FieldInfo or PropertyInfo is false)
                continue;
            var property = member as PropertyInfo;
            var keyAttributes = GetAttributes(member, a => a is NamedKeyAttribute or TupleKeyAttribute);
            var key = keyAttributes.FirstOrDefault();
            var conversionAttributes = GetAttributes(member, a => a is ConverterAttribute or ConverterCreatorAttribute);
            var conversion = conversionAttributes.FirstOrDefault();
            var indexer = property is not null && property.GetIndexParameters().Length is not 0;
            if (indexer && (key ?? conversion) is { } instance)
                throw new ArgumentException($"Can not apply '{instance.GetType().Name}' to an indexer, type: {type}");
            if (indexer)
                continue;
            if (property is not null && property.GetGetMethod() is null)
                throw new ArgumentException($"No available getter found, member name: {member.Name}, type: {type}");
            if (keyAttributes.Length > 1 || conversionAttributes.Length > 1)
                throw new ArgumentException($"Multiple attributes found, member name: {member.Name}, type: {type}");
            if (key is null && conversion is not null)
                throw new ArgumentException($"Require '{nameof(NamedKeyAttribute)}' or '{nameof(TupleKeyAttribute)}' for '{conversion.GetType().Name}', member name: {member.Name}, type: {type}");
            if (key is NamedKeyAttribute && typeInfo.IsNamedObject is false)
                throw new ArgumentException($"Require '{nameof(NamedObjectAttribute)}' for '{nameof(NamedKeyAttribute)}', member name: {member.Name}, type: {type}");
            if (key is TupleKeyAttribute && typeInfo.IsTupleObject is false)
                throw new ArgumentException($"Require '{nameof(TupleObjectAttribute)}' for '{nameof(TupleKeyAttribute)}', member name: {member.Name}, type: {type}");
            var optional = GetMemberIsOptional(typeInfo, member, key);
            var memberInfo = new MetaMemberInfo(member, key, conversion, optional);
            result.Add(memberInfo);
        }
        return result.DrainToImmutable();
    }

    private static bool GetMemberIsOptional(MetaTypeInfo typeInfo, MemberInfo member, Attribute? key)
    {
        if (typeInfo.IsRequired is false)
            return false;
        var required = GetAttributes(member, x => x is RequiredMemberAttribute).Any();
        if (required is false)
            return true;
        if (typeInfo.IsNamedObject && key is not NamedKeyAttribute)
            throw new ArgumentException($"Require '{nameof(NamedKeyAttribute)}' for required member, member name: {member.Name}, type: {typeInfo.Type}");
        if (typeInfo.IsTupleObject && key is not TupleKeyAttribute)
            throw new ArgumentException($"Require '{nameof(TupleKeyAttribute)}' for required member, member name: {member.Name}, type: {typeInfo.Type}");
        return false;
    }

    private static ImmutableArray<Attribute> GetAttributes(MemberInfo member, Func<Attribute, bool> filter)
    {
        Debug.Assert(member is Type or FieldInfo or PropertyInfo);
        var attributes = member.GetCustomAttributes(false).OfType<Attribute>().Where(filter).ToImmutableArray();
        return attributes;
    }

    private static T GetConverterOrCreator<T>([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type instance, Type reflected, string? memberName)
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
            var suffix = memberName is null
                ? $"type: {reflected}"
                : $"member name: {memberName}, type: {reflected}";
            throw new ArgumentException($"Can not get custom {prefix} via attribute, {suffix}", e);
        }
    }

    [RequiresUnreferencedCode(CommonModule.RequiresUnreferencedCodeMessage)]
    private static IConverter GetConverter(IGeneratorContext context, Type reflected, MetaMemberInfo? memberInfo, Attribute? attribute)
    {
        var type = memberInfo is null ? reflected : memberInfo.Type;
        Debug.Assert(attribute is null or ConverterAttribute or ConverterCreatorAttribute);
        if (attribute is ConverterAttribute alpha)
            return CommonModule.GetConverter(GetConverterOrCreator<IConverter>(alpha.Type, reflected, memberInfo?.Name), type, null);
        if (attribute is ConverterCreatorAttribute bravo)
            return CommonModule.GetConverter(GetConverterOrCreator<IConverterCreator>(bravo.Type, reflected, memberInfo?.Name).GetConverter(context, type), type, bravo.Type);
        Debug.Assert(attribute is null);
        return context.GetConverter(type);
    }

    private static ImmutableArray<MetaMemberInfo> GetSortedMembers(MetaTypeInfo typeInfo, ImmutableArray<MetaMemberInfo> source, out ImmutableArray<string> list)
    {
        ImmutableDictionary<MetaMemberInfo, T> Choose<T>() where T : class
        {
            var builder = ImmutableDictionary.CreateBuilder<MetaMemberInfo, T>();
            foreach (var member in source)
                if (member.KeyAttribute is T result)
                    builder.Add(member, result);
            return builder.ToImmutable();
        }

        var type = typeInfo.Type;
        var named = Choose<NamedKeyAttribute>();
        var tuple = Choose<TupleKeyAttribute>();
        if (named.Count is 0 && typeInfo.IsNamedObject)
            throw new ArgumentException($"Require '{nameof(NamedKeyAttribute)}' for '{nameof(NamedObjectAttribute)}', type: {type}");
        if (tuple.Count is 0 && typeInfo.IsTupleObject)
            throw new ArgumentException($"Require '{nameof(TupleKeyAttribute)}' for '{nameof(TupleObjectAttribute)}', type: {type}");

        list = default;
        if (typeInfo.IsTupleObject)
            return GetSortedMembers(type, tuple);
        if (typeInfo.IsNamedObject)
            return GetSortedMembers(type, named, out list);
        var result = source.OrderBy(x => x.Name).ToImmutableArray();
        list = result.Select(x => x.Name).ToImmutableArray();
        return result;
    }

    private static ImmutableArray<MetaMemberInfo> GetSortedMembers(Type type, ImmutableDictionary<MetaMemberInfo, NamedKeyAttribute> collection, out ImmutableArray<string> list)
    {
        Debug.Assert(collection.Count is not 0);
        var map = new SortedDictionary<string, MetaMemberInfo>();
        foreach (var (member, attribute) in collection)
        {
            var key = attribute.Key;
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException($"Named key can not be null or empty, member name: {member.Name}, type: {type}");
            if (map.ContainsKey(key))
                throw new ArgumentException($"Named key '{key}' already exists, type: {type}");
            map.Add(key, member);
        }
        list = map.Keys.ToImmutableArray();
        return map.Values.ToImmutableArray();
    }

    private static ImmutableArray<MetaMemberInfo> GetSortedMembers(Type type, ImmutableDictionary<MetaMemberInfo, TupleKeyAttribute> collection)
    {
        Debug.Assert(collection.Count is not 0);
        var map = new SortedDictionary<int, MetaMemberInfo>();
        foreach (var (member, attribute) in collection)
        {
            var key = attribute.Key;
            if (map.ContainsKey(key))
                throw new ArgumentException($"Tuple key '{key}' already exists, type: {type}");
            map.Add(key, member);
        }
        var keys = map.Keys;
        if (keys.First() is not 0 || keys.Last() != keys.Count - 1)
            throw new ArgumentException($"Tuple key must start at zero and must be sequential, type: {type}");
        return map.Values.ToImmutableArray();
    }

    [RequiresUnreferencedCode(CommonModule.RequiresUnreferencedCodeMessage)]
    private static ContextObjectConstructor? GetConstructor(Type type, ImmutableArray<MetaMemberInfo> members, ImmutableArray<ContextMemberInitializer> initializers)
    {
        static string Select(string? text) =>
            text?.ToUpperInvariant() ??
            string.Empty;

        Debug.Assert(members.Length is not 0);
        if (type.IsAbstract || type.IsInterface)
            return null;
        var hasDefaultConstructor = type.IsValueType || type.GetConstructor(Type.EmptyTypes) is not null;
        if (hasDefaultConstructor && members.All(x => x.IsReadOnly is false))
            return (delegateType, initializer) => ContextMethods.GetDecodeDelegate(delegateType, initializer, initializers);
        var selector = new Func<MetaMemberInfo, string>(x => Select(x.Name));
        if (members.DistinctBy(selector).Count() != members.Length)
            return null;

        var constructors = type.GetConstructors()
            .Select(x => (Constructor: x, Parameters: x.GetParameters()))
            .OrderByDescending(x => x.Parameters.Length)
            .ToList();
        // select constructor with most parameters
        var dictionary = members.Select((x, i) => (Key: selector.Invoke(x), Index: i)).ToDictionary(x => x.Key, v => v.Index);
        foreach (var (constructor, parameters) in constructors)
        {
            var objectIndexes = parameters
                .Select(x => dictionary.TryGetValue(Select(x.Name), out var index) && members[index].Type == x.ParameterType ? index : -1)
                .Where(x => x is not -1)
                .ToImmutableArray();
            if (objectIndexes.Length is 0 || objectIndexes.Length != parameters.Length)
                continue;
            var directIndexes = Enumerable.Range(0, members.Length).Except(objectIndexes).ToImmutableArray();
            if (directIndexes.Any(x => members[x].IsReadOnly))
                continue;
            var directInitializers = directIndexes.Select(x => members[x].Initializer).ToImmutableArray();
            Debug.Assert(members.Length == objectIndexes.Length + directIndexes.Length);
            return (delegateType, initializer) => ContextMethods.GetDecodeDelegate(delegateType, initializer, constructor, objectIndexes, directInitializers, directIndexes);
        }
        return null;
    }
}
