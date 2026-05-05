namespace Mikodev.Binary.SourceGeneration.Tests.DirectContextTests;

using Microsoft.CodeAnalysis;
using Mikodev.Binary.SourceGeneration.Contexts;
using System.Threading;
using Xunit;

public class TupleContextTests
{
    [Fact(DisplayName = "No Available Member Found")]
    public void NoAvailableMemberTest()
    {
        var source =
            """
            // no member
            namespace Tests;

            using Mikodev.Binary.Attributes;

            [SourceGeneratorContext]
            [SourceGeneratorInclude<System.ValueTuple>]
            public partial class TestSourceGeneratorContext { }
            """;
        var compilation = CompilationModule.CreateCompilation(source);
        var valueTupleSymbol = compilation.GetTypeByMetadataName("System.ValueTuple");
        Assert.NotNull(valueTupleSymbol);
        var context = new SourceGeneratorContext(compilation, _ => Assert.Fail("Invalid Call!"), CancellationToken.None);
        var tracker = new SourceGeneratorTracker(_ => Assert.Fail("Invalid Call!"));
        var result = TupleConverterContext.Invoke(context, tracker, valueTupleSymbol);
        Assert.NotNull(result);
        Assert.Equal(SourceStatus.Error, result.Status);

        var generator = new SourceGenerator();
        _ = CompilationModule.RunGenerators(compilation, out var diagnostics, generator);
        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
        Assert.Equal("SourceGeneratorInclude<System.ValueTuple>", diagnostic.Location.GetSourceText());
        Assert.EndsWith($"No converter generated, type: ValueTuple", diagnostic.ToString());
    }
}
