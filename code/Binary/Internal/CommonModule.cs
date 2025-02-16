namespace Mikodev.Binary.Internal;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;

[RequiresDynamicCode(CommonDefine.RequiresDynamicCodeMessage)]
[RequiresUnreferencedCode(CommonDefine.RequiresUnreferencedCodeMessage)]
internal static class CommonModule
{
    internal static T? SelectGenericTypeDefinitionOrDefault<T>(Type type, Func<Type, T> func)
    {
        return type.IsGenericType ? func.Invoke(type.GetGenericTypeDefinition()) : default;
    }

    internal static T CreateDelegate<T>(object? target, MethodInfo method) where T : MulticastDelegate
    {
        return (T)Delegate.CreateDelegate(typeof(T), target, method);
    }

    internal static object CreateInstance(Type type, object?[]? arguments)
    {
        try
        {
            if (Activator.CreateInstance(type, arguments) is { } result)
                return result;
            throw new InvalidOperationException($"Invalid null instance detected, type: {type}");
        }
        catch (Exception e)
        {
            if (e is TargetInvocationException { InnerException: { } inner })
                ExceptionDispatchInfo.Throw(inner);
            throw;
        }
    }

    internal static bool TryGetInterfaceArguments(Type type, Type definition, [MaybeNullWhen(false)] out Type[] arguments)
    {
        Debug.Assert(definition.IsInterface);
        Debug.Assert(definition.IsGenericTypeDefinition);
        var interfaces = type.IsInterface ? (IEnumerable<Type>)[type, .. type.GetInterfaces()] : type.GetInterfaces();
        var types = interfaces.Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == definition).ToList();
        var count = types.Count;
        if (count > 1)
            throw new ArgumentException($"Multiple interface implementations detected, type: {type}, interface type: {definition}");
        arguments = count is 0 ? null : types.Single().GetGenericArguments();
        return arguments is not null;
    }

    internal static MethodInfo GetPublicInstanceMethod(Type type, string methodName)
    {
        var result = type.GetMethod(methodName, CommonDefine.PublicInstanceBindingFlags);
        if (result is null)
            throw new MissingMethodException($"Method not found, method name: {methodName}, type: {type}");
        return result;
    }

    internal static FieldInfo GetPublicInstanceField(Type type, string fieldName)
    {
        var result = type.GetField(fieldName, CommonDefine.PublicInstanceBindingFlags);
        if (result is null)
            throw new MissingFieldException($"Field not found, field name: {fieldName}, type: {type}");
        return result;
    }

    internal static PropertyInfo GetPublicInstanceProperty(Type type, string propertyName)
    {
        var result = type.GetProperty(propertyName, CommonDefine.PublicInstanceBindingFlags);
        if (result is null)
            throw new MissingMemberException($"Property not found, property name: {propertyName}, type: {type}");
        return result;
    }

    internal static ConstructorInfo GetPublicInstanceConstructor(Type type, Type[] types)
    {
        var result = type.GetConstructor(types);
        if (result is null)
            throw new MissingMethodException($"Constructor not found, type: {type}");
        return result;
    }

    internal static ImmutableArray<Attribute> GetAttributes(MemberInfo member, Func<Attribute, bool> filter)
    {
        Debug.Assert(member is Type or FieldInfo or PropertyInfo);
        var attributes = member.GetCustomAttributes(false).OfType<Attribute>().Where(filter).ToImmutableArray();
        return attributes;
    }

    internal static bool IsIndexer(PropertyInfo property)
    {
        return property.GetIndexParameters().Length is not 0;
    }

    internal static int CompareInheritance(Type? x, Type? y)
    {
        ArgumentNullException.ThrowIfNull(x);
        ArgumentNullException.ThrowIfNull(y);
        if (x.IsValueType || y.IsValueType)
            throw new ArgumentException("Require reference type.");
        if (x == y)
            throw new ArgumentException("Identical types detected.");
        if (x.IsAssignableTo(y))
            return -1;
        if (y.IsAssignableTo(x))
            return 1;
        return 0;
    }

    internal static ImmutableArray<MemberInfo> GetAllPropertiesForInterfaceType(Type type, BindingFlags flags)
    {
        if (type.IsInterface is false)
            throw new ArgumentException("Require interface type.");

        var source = ImmutableArray.CreateRange(type.GetInterfaces()).Add(type);
        var result = ImmutableArray.CreateBuilder<MemberInfo>();
        var dictionary = new SortedDictionary<string, List<PropertyInfo>>();

        void Insert(PropertyInfo member)
        {
            var same = new List<PropertyInfo>();
            var less = new List<PropertyInfo>();
            if (dictionary.TryGetValue(member.Name, out var values))
            {
                foreach (var i in values)
                {
                    var signal = CompareInheritance(i.DeclaringType, member.DeclaringType);
                    if (signal is 0)
                        same.Add(i);
                    else if (signal is -1)
                        less.Add(i);
                }
            }

            if (less.Count is 0)
                less.Add(member);
            less.AddRange(same);
            dictionary[member.Name] = less;
        }

        foreach (var target in source)
        {
            var members = target.GetProperties(flags);
            foreach (var member in members)
            {
                // ignore overriding or shadowing
                if (IsIndexer(member))
                    result.Add(member);
                else
                    Insert(member);
            }
        }

        foreach (var i in dictionary)
        {
            var values = i.Value;
            if (values.Count is 1)
                result.Add(values.First());
            else
                ThrowHelper.ThrowAmbiguousMembers(i.Key, type);
        }

        return result.DrainToImmutable();
    }

    internal static ImmutableArray<MemberInfo> GetAllFieldsAndPropertiesForNonInterfaceType(Type type, BindingFlags flags)
    {
        if (type.IsInterface)
            throw new ArgumentException("Require not interface type.");

        var result = ImmutableArray.CreateBuilder<MemberInfo>();
        var dictionary = new SortedDictionary<string, MemberInfo>();
        for (var target = type; target is not null; target = target.BaseType)
        {
            var members = target.GetMembers(flags);
            foreach (var member in members)
            {
                if (target != member.DeclaringType)
                    continue;
                if (member is not FieldInfo and not PropertyInfo)
                    continue;
                // ignore overriding or shadowing
                if (member is PropertyInfo property && IsIndexer(property))
                    result.Add(member);
                else if (dictionary.TryGetValue(member.Name, out var exists) is false)
                    dictionary.Add(member.Name, member);
                else if (target == exists.DeclaringType)
                    ThrowHelper.ThrowAmbiguousMembers(member.Name, type);
            }
        }
        result.AddRange(dictionary.Values);
        return result.DrainToImmutable();
    }

    internal static ImmutableArray<MemberInfo> GetAllFieldsAndProperties(Type type, BindingFlags flags)
    {
        if (type.IsInterface)
            return GetAllPropertiesForInterfaceType(type, flags);
        return GetAllFieldsAndPropertiesForNonInterfaceType(type, flags);
    }

    internal static IConverter? GetConverter(IGeneratorContext context, Type type, Type typeDefinition, Type converterDefinition)
    {
        Debug.Assert(converterDefinition.IsGenericTypeDefinition);
        Debug.Assert(converterDefinition.GetGenericArguments().Length == typeDefinition.GetGenericArguments().Length);
        if (type.IsGenericType is false || type.GetGenericTypeDefinition() != typeDefinition)
            return null;
        var arguments = type.GetGenericArguments();
        var converterArguments = arguments.Select(context.GetConverter).Cast<object>().ToArray();
        var converterType = converterDefinition.MakeGenericType(arguments);
        var converter = CreateInstance(converterType, converterArguments);
        return (IConverter)converter;
    }
}
