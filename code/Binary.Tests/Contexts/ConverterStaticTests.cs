namespace Mikodev.Binary.Tests.Contexts;

using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit;

public class ConverterStaticTests
{
    [Fact(DisplayName = "Argument Null Test")]
    public void ArgumentNullTest()
    {
        var methods = new List<MethodInfo>();
        methods.Add(new Func<IConverter, Type>(Converter.GetGenericArgument).Method);
        methods.Add(new Func<IConverter, string, MethodInfo>(Converter.GetMethod).Method);
        Assert.All(methods, ArgumentTests.ArgumentNullExceptionTest);
    }
}
