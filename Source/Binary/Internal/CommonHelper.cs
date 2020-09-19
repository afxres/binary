using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Mikodev.Binary.Internal
{
    internal static class CommonHelper
    {
        internal static T[] Concat<T>(T item, T[] values)
        {
            var result = new List<T> { item };
            result.AddRange(values);
            return result.ToArray();
        }

        internal static bool IsByRefLike(Type type)
        {
            if (type.IsValueType is false)
                return false;
            var attributes = type.GetCustomAttributes();
            return attributes.Select(x => x.GetType()).Any(x => x.FullName == "System.Runtime.CompilerServices.IsByRefLikeAttribute");
        }

        internal static bool IsImplementationOf(Type type, Type definition)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == definition;
        }

        internal static bool TryGetGenericArguments(Type type, Type definition, out Type[] arguments)
        {
            Debug.Assert(definition.IsGenericTypeDefinition);
            arguments = IsImplementationOf(type, definition) ? type.GetGenericArguments() : null;
            return arguments != null;
        }

        internal static bool TryGetInterfaceArguments(Type type, Type definition, out Type[] arguments)
        {
            Debug.Assert(definition.IsInterface);
            Debug.Assert(definition.IsGenericTypeDefinition);
            var interfaces = type.IsInterface ? Concat(type, type.GetInterfaces()) : type.GetInterfaces();
            var types = interfaces.Where(x => IsImplementationOf(x, definition)).ToList();
            var count = types.Count;
            if (count > 1)
                throw new ArgumentException($"Multiple interface implementations detected, type: {type}, interface type: {definition}");
            arguments = count == 0 ? null : types.Single().GetGenericArguments();
            return arguments != null;
        }
    }
}
