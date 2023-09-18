namespace Mikodev.Binary.Internal;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;

internal static class CommonModule
{
    internal const string DebuggerDisplayValue = "{ToString(),nq}";

    internal const string RequiresUnreferencedCodeMessage = "Require public members for binary serialization.";

    internal const BindingFlags PublicInstanceBindingFlags = BindingFlags.Instance | BindingFlags.Public;

    internal static T? SelectGenericTypeDefinitionOrDefault<T>(Type type, Func<Type, T> func)
    {
        return type.IsGenericType ? func.Invoke(type.GetGenericTypeDefinition()) : default;
    }

    internal static T CreateDelegate<T>(object? target, MethodInfo method) where T : MulticastDelegate
    {
        return (T)Delegate.CreateDelegate(typeof(T), target, method);
    }

    internal static object CreateInstance([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type type, object?[]? arguments)
    {
        static object? Invoke(Func<object?> func)
        {
            try
            {
                return func.Invoke();
            }
            catch (TargetInvocationException e)
            {
                if (e.InnerException is { } inner)
                    ExceptionDispatchInfo.Throw(inner);
                throw;
            }
        }

        var result = Invoke(() => Activator.CreateInstance(type, arguments));
        if (result is null)
            throw new InvalidOperationException($"Invalid null instance detected, type: {type}");
        return result;
    }

    internal static bool TryGetInterfaceArguments([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] Type type, Type definition, [MaybeNullWhen(false)] out Type[] arguments)
    {
        Debug.Assert(definition.IsInterface);
        Debug.Assert(definition.IsGenericTypeDefinition);
        var interfaces = type.IsInterface ? (IEnumerable<Type>)ImmutableArray.Create(type).AddRange(type.GetInterfaces()) : type.GetInterfaces();
        var types = interfaces.Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == definition).ToList();
        var count = types.Count;
        if (count > 1)
            throw new ArgumentException($"Multiple interface implementations detected, type: {type}, interface type: {definition}");
        arguments = count is 0 ? null : types.Single().GetGenericArguments();
        return arguments is not null;
    }

    internal static MethodInfo GetPublicInstanceMethod([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] Type type, string methodName)
    {
        var result = type.GetMethod(methodName, PublicInstanceBindingFlags);
        if (result is null)
            throw new MissingMethodException($"Method not found, method name: {methodName}, type: {type}");
        return result;
    }

    internal static FieldInfo GetPublicInstanceField([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] Type type, string fieldName)
    {
        var result = type.GetField(fieldName, PublicInstanceBindingFlags);
        if (result is null)
            throw new MissingFieldException($"Field not found, field name: {fieldName}, type: {type}");
        return result;
    }

    internal static PropertyInfo GetPublicInstanceProperty([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] Type type, string propertyName)
    {
        var result = type.GetProperty(propertyName, PublicInstanceBindingFlags);
        if (result is null)
            throw new MissingMemberException($"Property not found, property name: {propertyName}, type: {type}");
        return result;
    }

    internal static ConstructorInfo GetPublicInstanceConstructor([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type type, Type[] types)
    {
        var result = type.GetConstructor(types);
        if (result is null)
            throw new MissingMethodException($"Constructor not found, type: {type}");
        return result;
    }

    internal static IConverter GetConverter(IConverter? converter, Type type, Type? creator)
    {
        if (converter is not null && Converter.GetGenericArgument(converter) == type)
            return converter;
        var actual = converter is null ? "null" : converter.GetType().ToString();
        var result = $"Invalid converter, expected: converter for '{type}', actual: {actual}";
        if (creator is not null)
            result = $"{result}, converter creator type: {creator}";
        throw new InvalidOperationException(result);
    }

    [RequiresUnreferencedCode(RequiresUnreferencedCodeMessage)]
    internal static ImmutableArray<MemberInfo> GetAllFieldsAndProperties(Type type, BindingFlags flags)
    {
        static ImmutableArray<Type> Expand(Type type)
        {
            var result = ImmutableArray.CreateBuilder<Type>();
            for (var i = type; i != null; i = i.BaseType)
                result.Add(i);
            return result.DrainToImmutable();
        }

        var source = type.IsInterface
            ? ImmutableArray.Create(type).AddRange(type.GetInterfaces())
            : Expand(type);
        var result = new List<MemberInfo>();
        var dictionary = new SortedDictionary<string, MemberInfo>();
        foreach (var target in source)
        {
            var members = target.GetMembers(flags);
            foreach (var member in members)
            {
                if (target != member.DeclaringType)
                    continue;
                var field = member as FieldInfo;
                var property = member as PropertyInfo;
                if (field is null && property is null)
                    continue;

                // ignore overriding or shadowing
                var indexer = property is not null && property.GetIndexParameters().Length is not 0;
                if (indexer)
                    result.Add(member);
                else if (dictionary.TryGetValue(member.Name, out var exists) is false)
                    dictionary.Add(member.Name, member);
                else if (target != exists.DeclaringType && target.IsAssignableTo(exists.DeclaringType))
                    dictionary[member.Name] = member;
                else if (target == exists.DeclaringType || target.IsAssignableFrom(exists.DeclaringType) is false)
                    throw new ArgumentException($"Get members error, duplicate members detected, member name: {member.Name}, type: {target}");
            }
        }
        result.AddRange(dictionary.Values);
        return result.ToImmutableArray();
    }

    [RequiresUnreferencedCode(RequiresUnreferencedCodeMessage)]
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
