namespace Mikodev.Binary.Tests.Contexts;

using Mikodev.Binary.Tests.Internal;
using System;
using System.Reflection;
using Xunit;

public class GeneratorTests
{
    [Theory(DisplayName = "Internal Create Instance Error")]
    [InlineData("Mikodev.Binary.Not.Exist")]
    [InlineData("System.Nullable`1[System.Int32]")]
    public void InternalCreateInstanceError(string typeName)
    {
        var method = ReflectionExtensions.GetMethodNotNull(typeof(Generator), "Create", BindingFlags.Static | BindingFlags.NonPublic);
        var create = (Func<string, string, IConverterCreator>)Delegate.CreateDelegate(typeof(Func<string, string, IConverterCreator>), method);
        var error = Assert.Throws<ArgumentException>(() => create.Invoke("VirtualStaticsInInterfaces", typeName));
        Assert.Null(error.ParamName);
        Assert.Equal($"Create instance error, type: {typeName}", error.Message);
    }

    [Theory(DisplayName = "Internal Create Instance Default")]
    [InlineData("NotExistFeature")]
    [InlineData("BadFeatureString")]
    public void InternalCreateInstanceDefault(string feature)
    {
        var method = ReflectionExtensions.GetMethodNotNull(typeof(Generator), "Create", BindingFlags.Static | BindingFlags.NonPublic);
        var create = (Func<string, string?, IConverterCreator>)Delegate.CreateDelegate(typeof(Func<string, string, IConverterCreator>), method);
        var result = create.Invoke(feature, null);
        Assert.NotNull(result);
        Assert.Equal("OldConverterCreator", result.GetType().Name);
    }
}
