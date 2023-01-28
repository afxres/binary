namespace Mikodev.Binary.Tests.Internal;

using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Xunit;

[DebuggerStepThrough]
internal static class ReflectionExtensions
{
    internal static TDelegate CreateDelegate<TDelegate>(Func<Type, bool> typeFilter, Func<MethodInfo, bool> methodFilter) where TDelegate : Delegate
    {
        var type = typeof(IConverter).Assembly.GetTypes().Single(typeFilter);
        var method = type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).Single(methodFilter);
        var functor = Delegate.CreateDelegate(typeof(TDelegate), method);
        return (TDelegate)functor;
    }

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

    internal static object GetFieldValueNotNull(this object instance, string name, BindingFlags flags)
    {
        var type = instance.GetType();
        var result = type.GetField(name, flags);
        if (result is null)
            throw new MissingFieldException();
        var target = result.GetValue(instance);
        Assert.NotNull(target);
        return target;
    }

    internal static T CreateInstance<T>(string typeName)
    {
        var type = typeof(IConverter).Assembly.GetTypes().Single(x => x.Name == typeName);
        var instance = Activator.CreateInstance(type);
        return Assert.IsAssignableFrom<T>(instance);
    }
}
