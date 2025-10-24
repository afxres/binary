namespace Mikodev.Binary.Tests.Features;

using System.Linq;
using System.Reflection;
using Xunit;

public class CodeContractsTests
{
    [Fact(DisplayName = "Converter Type Name")]
    public void ConverterTypeName()
    {
        var types = typeof(IConverter).Assembly.GetTypes()
            .Where(x => x.IsAbstract is false && x.IsGenericTypeDefinition is false && x.IsAssignableTo(typeof(IConverter)))
            .ToList();
        Assert.NotEmpty(types);
        foreach (var i in types)
        {
            var baseType = i.BaseType;
            Assert.NotNull(baseType);
            var parameter = baseType.GetGenericArguments().First();
            if (parameter == typeof(object))
                continue;
            var expectedName = parameter.Name + "Converter";
            Assert.Equal(expectedName, i.Name);
        }
    }

    [Fact(DisplayName = "Converter Functions Type Name")]
    public void ConverterFunctionsTypeName()
    {
        var types = typeof(IConverter).Assembly.GetTypes()
            .Where(x => x.IsInterface is false && x.GetInterfaces().Any(i => i.Name.Contains("Functions`")))
            .ToList();
        Assert.NotEmpty(types);
        Assert.All(types, x => Assert.True(x.IsValueType));
        Assert.All(types, x => Assert.Equal("Functions", x.Name));
        Assert.All(types, x => Assert.Contains("IsReadOnlyAttribute", x.GetCustomAttributes().Select(x => x.GetType().Name)));
    }
}
