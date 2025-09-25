namespace Mikodev.Binary.SourceGeneration.Tests.DirectContextTests;

using Mikodev.Binary.SourceGeneration.Contexts;
using System.Threading;
using Xunit;

public class TupleContextTests
{
    [Fact(DisplayName = "No Available Member Found")]
    public void NoAvailableMemberTest()
    {
        var compilation = CompilationModule.CreateCompilation(string.Empty);
        var valueTupleSymbol = compilation.GetTypeByMetadataName("System.ValueTuple");
        Assert.NotNull(valueTupleSymbol);
        var context = new SourceGeneratorContext(compilation, _ => Assert.Fail("Invalid Call!"), CancellationToken.None);
        var tracker = new SourceGeneratorTracker(_ => Assert.Fail("Invalid Call!"));
        var result = TupleConverterContext.Invoke(context, tracker, valueTupleSymbol);
        Assert.NotNull(result);
        Assert.Equal(SourceStatus.NoAvailableMember, result.Status);
    }
}
