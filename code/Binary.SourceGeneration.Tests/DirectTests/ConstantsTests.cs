namespace Mikodev.Binary.SourceGeneration.Tests.DirectTests;

using Microsoft.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Xunit;

public class ConstantsTests
{
    [Fact(DisplayName = "Diagnostic Descriptors Test")]
    public void DiagnosticDescriptorsTest()
    {
        var members = typeof(Constants).GetMembers(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        var descriptorProperties = members.OfType<PropertyInfo>().Where(x => x.PropertyType == typeof(DiagnosticDescriptor)).ToList();
        var descriptors = descriptorProperties.Select(x => Assert.IsType<DiagnosticDescriptor>(x.GetValue(null), exactMatch: false)).ToList();
        Assert.NotEmpty(descriptors);
        var ids = descriptors.Select(x => x.Id).ToList();
        Assert.All(ids, x => Assert.Matches(@"^BINSRCGEN\d\d$", x));
        Assert.Distinct(ids);
    }
}
