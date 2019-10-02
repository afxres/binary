using Mikodev.Binary.Converters.Unsafe.Generic;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Mikodev.Binary.Internal
{
    internal static class Extensions
    {
        internal static bool IsImplementationOf(this Type type, Type definition)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == definition;
        }

        internal static bool IsUnsafeNativeConverter(this Converter converter)
        {
            return converter.GetType().IsImplementationOf(typeof(UnsafeNativeConverter<>));
        }

        internal static bool IsByRefLike(this Type type)
        {
            Debug.Assert(type != null);
            if (!type.IsValueType)
                return false;
            var attributes = type.GetCustomAttributes();
            return attributes.Select(x => x.GetType()).Any(x => x.FullName == "System.Runtime.CompilerServices.IsByRefLikeAttribute");
        }

        internal static bool TryGetGenericArguments(this Type type, Type definition, out Type[] arguments)
        {
            Debug.Assert(definition.IsGenericTypeDefinition);
            arguments = type.IsImplementationOf(definition) ? type.GetGenericArguments() : null;
            return arguments != null;
        }

        internal static bool TryGetInterfaceArguments(this Type type, Type definition, out Type[] arguments)
        {
            Debug.Assert(definition.IsInterface);
            Debug.Assert(definition.IsGenericTypeDefinition);
            var interfaces = type.IsInterface ? type.GetInterfaces().Concat(new[] { type }).ToArray() : type.GetInterfaces();
            var types = interfaces.Where(r => r.IsImplementationOf(definition)).ToList();
            var count = types.Count;
            if (count > 1)
                throw new ArgumentException($"Multiple interface implementations detected, type: {type}, interface type: {definition}");
            arguments = count == 0 ? null : types.Single().GetGenericArguments();
            return arguments != null;
        }
    }
}
