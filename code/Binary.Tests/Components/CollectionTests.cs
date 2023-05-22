namespace Mikodev.Binary.Tests.Components;

using Mikodev.Binary.Components;
using System.Linq;
using Xunit;

public class CollectionTests
{
    [Fact(DisplayName = "Argument Null Test")]
    public void ArgumentNullTest()
    {
        var methods = typeof(Collection).GetMethods().Where(x => x.Name.Contains("Decoder")).ToList();
        var group1 = methods.Where(x => x.GetGenericArguments().Length is 1).ToList();
        var group2 = methods.Where(x => x.GetGenericArguments().Length is 2).ToList();
        var group1Methods = group1.Select(x => x.MakeGenericMethod(typeof(object))).ToList();
        var group2Methods = group2.Select(x => x.MakeGenericMethod(typeof(object), typeof(object))).ToList();
        var result = group1Methods.Concat(group2Methods).ToList();
        Assert.NotEmpty(group1Methods);
        Assert.NotEmpty(group2Methods);
        Assert.Equal(methods.Count, result.Count);
        Assert.All(result, ArgumentTests.ArgumentNullExceptionTest);
    }
}
