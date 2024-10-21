#if NET9_0_OR_GREATER
namespace Mikodev.Binary.Tests.Features;

using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Xunit;

public class AllowsReferenceStructureTests
{
    [Fact(DisplayName = "Allocator Method Allows Reference Structure Test")]
    public void AllocatorMethodAllowsReferenceStructureTest()
    {
        var methods = typeof(Allocator).GetMethods(BindingFlags.Public | BindingFlags.Static).Where(x => x.IsGenericMethod).ToImmutableArray();
        foreach (var i in methods)
        {
            var genericArgument = Assert.Single(i.GetGenericArguments());
            var genericArgumentAttributes = genericArgument.GenericParameterAttributes;
            Assert.True((genericArgumentAttributes | GenericParameterAttributes.AllowByRefLike) is not 0);
        }
    }
}
#endif
