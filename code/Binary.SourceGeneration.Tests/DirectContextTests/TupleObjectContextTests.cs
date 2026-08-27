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
        var source =
            """
            // no member
            namespace Tests;

            using Mikodev.Binary.Attributes;

            [SourceGeneratorContext]
            [SourceGeneratorInclude<TupleObjectWithoutMember>]
            public partial class TestSourceGeneratorContext { }

            [TupleObject]
            struct TupleObjectWithoutMember { }
            """;
        var compilation = CompilationModule.CreateCompilation(source);
        var valueTupleSymbol = compilation.GetTypeByMetadataName("Tests.TupleObjectWithoutMember");
        Assert.NotNull(valueTupleSymbol);
        var context = new SourceGeneratorContext(compilation, _ => Assert.Fail("Invalid Call!"), CancellationToken.None);
        var tracker = new SourceGeneratorTracker(_ => Assert.Fail("Invalid Call!"));
        var result = Assert.IsType<SourceResult>(TupleObjectConverterContext.Invoke(context, tracker, valueTupleSymbol));
        Assert.NotNull(result);
        Assert.Equal(SourceStatus.Error, result.Status);

        var typeName = "TupleObjectWithoutMember";
        var generator = new SourceGenerator();
        _ = CompilationModule.RunGenerators(compilation, out var diagnostics, generator);
        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.Equal("TupleObject", diagnostic.Location.GetSourceText());
        Assert.EndsWith($"No available member was found, type: {typeName}", diagnostic.ToString());
    }
}
