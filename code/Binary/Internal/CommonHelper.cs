using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Mikodev.Binary.Internal
{
    internal static class CommonHelper
    {
        internal static T SelectGenericTypeDefinitionOrDefault<T>(Type type, Func<Type, T> func)
        {
            return type.IsGenericType ? func.Invoke(type.GetGenericTypeDefinition()) : default;
        }

        internal static bool TryGetGenericArguments(Type type, Type definition, out Type[] arguments)
        {
            Debug.Assert(definition.IsGenericTypeDefinition);
            arguments = type.IsGenericType && type.GetGenericTypeDefinition() == definition ? type.GetGenericArguments() : null;
            return arguments is not null;
        }

        internal static bool TryGetInterfaceArguments(Type type, Type definition, out Type[] arguments)
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

        internal static MethodInfo GetMethod(Type type, string methodName, Type[] types)
        {
            var result = type.GetMethod(methodName, types);
            if (result is null)
                throw new MissingMethodException($"Method not found, method name: {methodName}, type: {type}");
            return result;
        }

        internal static MethodInfo GetMethod(Type type, string methodName, BindingFlags flags)
        {
            var result = type.GetMethod(methodName, flags);
            if (result is null)
                throw new MissingMethodException($"Method not found, method name: {methodName}, type: {type}");
            return result;
        }

        internal static FieldInfo GetField(Type type, string fieldName, BindingFlags flags)
        {
            var result = type.GetField(fieldName, flags);
            if (result is null)
                throw new MissingFieldException($"Field not found, field name: {fieldName}, type: {type}");
            return result;
        }

        internal static PropertyInfo GetProperty(Type type, string propertyName, BindingFlags flags)
        {
            var result = type.GetProperty(propertyName, flags);
            if (result is null)
                throw new MissingMemberException($"Property not found, property name: {propertyName}, type: {type}");
            return result;
        }

        internal static ConstructorInfo GetConstructor(Type type, Type[] types)
        {
            var result = type.GetConstructor(types);
            if (result is null)
                throw new MissingMethodException($"Constructor not found, type: {type}");
            return result;
        }

        internal static MethodInfo GetMethod<T, E>(Expression<Func<T, E>> expression)
        {
            return ((MethodCallExpression)expression.Body).Method;
        }

        internal static PropertyInfo GetProperty<T, E>(Expression<Func<T, E>> expression)
        {
            return (PropertyInfo)((MemberExpression)expression.Body).Member;
        }

        internal static IConverter GetConverter(IConverter converter, Type type)
        {
            Debug.Assert(converter is not null);
            var expectedType = typeof(Converter<>).MakeGenericType(type);
            var instanceType = converter.GetType();
            if (expectedType.IsAssignableFrom(instanceType) is false)
                throw new ArgumentException($"Can not convert '{instanceType}' to '{expectedType}'");
            return converter;
        }

        internal static IConverter GetConverter(IConverter converter, Type type, Type creatorType)
        {
            var expectedType = typeof(Converter<>).MakeGenericType(type);
            if (converter is null)
                throw new ArgumentException($"Can not convert null to '{expectedType}', converter creator type: {creatorType}");
            var instanceType = converter.GetType();
            if (expectedType.IsAssignableFrom(instanceType) is false)
                throw new ArgumentException($"Can not convert '{instanceType}' to '{expectedType}', converter creator type: {creatorType}");
            return converter;
        }

        internal static IConverter GetConverter(IGeneratorContext context, Type type, Type typeDefinition, Type converterDefinition, Func<ImmutableArray<IConverter>, ImmutableArray<object>> argumentsHandler)
        {
            Debug.Assert(converterDefinition.IsGenericTypeDefinition);
            Debug.Assert(converterDefinition.GetGenericArguments().Length == typeDefinition.GetGenericArguments().Length);
            if (TryGetGenericArguments(type, typeDefinition, out var arguments) is false)
                return null;
            var converters = arguments.Select(context.GetConverter).ToImmutableArray();
            var converterArguments = argumentsHandler is null ? converters.Cast<object>().ToArray() : argumentsHandler.Invoke(converters).ToArray();
            var converterType = converterDefinition.MakeGenericType(arguments);
            var converter = Activator.CreateInstance(converterType, converterArguments);
            return (IConverter)converter;
        }
    }
}
