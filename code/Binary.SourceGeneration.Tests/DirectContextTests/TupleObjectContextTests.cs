namespace Mikodev.Binary.SourceGeneration.Tests.DirectContextTests;

using Microsoft.CodeAnalysis;
using Mikodev.Binary.SourceGeneration.Contexts;
using System.Threading;
using Xunit;

public class TupleObjectContextTests
{
    [Fact(DisplayName = "Invalid Symbol Test")]
    public void InvalidSymbolTest()
    {
        var compilation = CompilationModule.CreateCompilation(string.Empty);
        var int32Symbol = compilation.GetSpecialType(SpecialType.System_Int32);
        var int32PointerSymbol = compilation.CreatePointerTypeSymbol(int32Symbol);
        var context = new SourceGeneratorContext(compilation, _ => Assert.Fail("Invalid Call!"), CancellationToken.None);
        var tracker = new SourceGeneratorTracker(_ => Assert.Fail("Invalid Call!"));
        var a = TupleObjectConverterContext.Invoke(context, tracker, int32Symbol);
        var b = TupleObjectConverterContext.Invoke(context, tracker, int32PointerSymbol);
        Assert.Null(a);
        Assert.Null(b);
    }

    [Fact(DisplayName = "No Available Member Found")]
    public void NoAvailableMemberTest()
    {
        var compilation = CompilationModule.CreateCompilation(string.Empty);
        var valueTupleSymbol = compilation.GetTypeByMetadataName("System.ValueTuple");
        Assert.NotNull(valueTupleSymbol);
        var context = new SourceGeneratorContext(compilation, _ => Assert.Fail("Invalid Call!"), CancellationToken.None);
        var tracker = new SourceGeneratorTracker(_ => Assert.Fail("Invalid Call!"));
        var result = TupleObjectConverterContext.Invoke(context, tracker, valueTupleSymbol);
        Assert.NotNull(result);
        Assert.Equal(SourceStatus.NoAvailableMember, result.Status);
    }
}
