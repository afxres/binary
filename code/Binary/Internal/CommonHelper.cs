using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Mikodev.Binary.Internal
{
    internal static class CommonHelper
    {
        internal static T[] Concat<T>(T item, IReadOnlyList<T> values)
        {
            var result = new List<T> { item };
            result.AddRange(values);
            return result.ToArray();
        }

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
            var interfaces = type.IsInterface ? Concat(type, type.GetInterfaces()) : type.GetInterfaces();
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
    }
}
