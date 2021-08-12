namespace Mikodev.Binary.Tests.Internal;

using System;
using System.Reflection;

internal static class ReflectionExtensions
{
    internal static MethodInfo GetMethodNotNull(this Type type, string name, BindingFlags flags)
    {
        var result = type.GetMethod(name, flags);
        if (result is null)
            throw new MissingMethodException();
        return result;
    }

    internal static MethodInfo GetMethodNotNull(this Type type, string name, params Type[] types)
    {
        var result = type.GetMethod(name, types);
        if (result is null)
            throw new MissingMethodException();
        return result;
    }

    internal static FieldInfo GetFieldNotNull(this Type type, string name, BindingFlags flags)
    {
        var result = type.GetField(name, flags);
        if (result is null)
            throw new MissingFieldException();
        return result;
    }
}
