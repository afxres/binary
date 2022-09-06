namespace Mikodev.Binary.Tests.Features;

using System.Linq;
using Xunit;

public class CodeContractsTests
{
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
