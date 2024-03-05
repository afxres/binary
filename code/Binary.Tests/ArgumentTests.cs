namespace Mikodev.Binary.Tests;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Xunit;

public static class ArgumentTests
{
    private static readonly ImmutableArray<MethodInfo> ArgumentNullTestMethods;

    static ArgumentTests()
    {
        var builder = ImmutableArray.CreateBuilder<MethodInfo>();
        void Add(Func<MethodInfo, IEnumerable<ArgumentNullException>> func) => builder.Add(func.Method.GetGenericMethodDefinition());
        Add(ArgumentNullUnsafeTest<object, object>);
        Add(ArgumentNullUnsafeTest<object, object, object>);
        Add(ArgumentNullUnsafeTest<object, object, object, object>);
        ArgumentNullTestMethods = builder.DrainToImmutable();
    }

    private static IEnumerable<ArgumentNullException> ArgumentNullUnsafeTest<T, R>(MethodInfo method) where T : class
    {
        var func = (Func<T, R>)Delegate.CreateDelegate(typeof(Func<T, R>), method);
        var a = Assert.Throws<ArgumentNullException>(() => func.Invoke(null!));
        return [a];
    }

    private static IEnumerable<ArgumentNullException> ArgumentNullUnsafeTest<T, U, R>(MethodInfo method) where T : class where U : class
    {
        var func = (Func<T, U, R>)Delegate.CreateDelegate(typeof(Func<T, U, R>), method);
        var instance = new object();
        // invalid object reference, do not dereference!!!
        var t = Unsafe.As<object, T>(ref instance);
        // invalid object reference, do not dereference!!!
        var u = Unsafe.As<object, U>(ref instance);
        var a = Assert.Throws<ArgumentNullException>(() => func.Invoke(null!, u));
        var b = Assert.Throws<ArgumentNullException>(() => func.Invoke(t, null!));
        return [a, b];
    }

    private static IEnumerable<ArgumentNullException> ArgumentNullUnsafeTest<T, U, S, R>(MethodInfo method) where T : class where U : class where S : class
    {
        var func = (Func<T, U, S, R>)Delegate.CreateDelegate(typeof(Func<T, U, S, R>), method);
        var instance = new object();
        // invalid object reference, do not dereference!!!
        var t = Unsafe.As<object, T>(ref instance);
        // invalid object reference, do not dereference!!!
        var u = Unsafe.As<object, U>(ref instance);
        // invalid object reference, do not dereference!!!
        var s = Unsafe.As<object, S>(ref instance);
        var a = Assert.Throws<ArgumentNullException>(() => func.Invoke(null!, u, s));
        var b = Assert.Throws<ArgumentNullException>(() => func.Invoke(t, null!, s));
        var c = Assert.Throws<ArgumentNullException>(() => func.Invoke(t, u, null!));
        return [a, b, c];
    }

    public static void ArgumentNullExceptionTest(MethodInfo method)
    {
        var parameters = method.GetParameters();
        var types = parameters.Select(x => x.ParameterType).Concat([method.ReturnType]).ToArray();
        var test = ArgumentNullTestMethods[parameters.Length - 1].MakeGenericMethod(types);
        var errors = Assert.IsAssignableFrom<IEnumerable<ArgumentNullException>>(test.Invoke(null, [method]));
        var expectedParameterNames = parameters.Select(x => x.Name).ToList();
        var actualParameterNames = errors.Select(x => x.ParamName).ToList();
        Assert.Equal(expectedParameterNames, actualParameterNames);
    }
}
