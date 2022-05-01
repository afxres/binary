namespace Mikodev.Binary.Features.Tests;

using System;
using System.Linq;
using System.Runtime.Versioning;
using Xunit;

public class CodeContractsTests
{
    [Fact(DisplayName = "Preview Features Attributes")]
    public void FeaturesAttributes()
    {
        var types = typeof(IConverter).Assembly.GetTypes().Where(x => x.IsNested is false && x.Namespace is { } n && n.Contains(".Features")).ToList();
        types.Add(typeof(GeneratorBuilderPreviewFeaturesExtensions));
        foreach (var i in types)
        {
            var attributes = i.GetCustomAttributes(false);
            var query = attributes.Cast<Attribute>().Where(x => (Type)x.TypeId == typeof(RequiresPreviewFeaturesAttribute));
            _ = Assert.Single(query);
        }

        Assert.Contains(types, x => x.IsClass);
        Assert.Contains(types, x => x.IsInterface);
        Assert.Contains(types, x => x.IsValueType);
    }

    [Fact(DisplayName = "Raw Converter Type Name")]
    public void RawConverterTypeName()
    {
        var types = typeof(IConverter).Assembly.GetTypes().Where(x => x.GetInterfaces().Any(x => x.Name is "IRawConverter`1") && x.IsGenericType is false).ToList();
        Assert.NotEmpty(types);
        Assert.All(types, x => Assert.True(x.IsValueType));
        foreach (var i in types)
        {
            var genericArgument = i.GetInterfaces().Single(x => x.Name is "IRawConverter`1").GetGenericArguments().Single();
            var expectedTypeName = $"{genericArgument.Name}RawConverter";
            Assert.Equal(expectedTypeName, i.Name);
        }
    }
}
