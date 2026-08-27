namespace Mikodev.Binary.SourceGeneration.Tests.DirectContextTests;

using Microsoft.CodeAnalysis;
using Mikodev.Binary.SourceGeneration.Contexts;
using System.Threading;
using Xunit;

public class TupleContextTests
{
    [Fact(DisplayName = "Type Not Recognized")]
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
        var result = Assert.IsType<SourceResultWithDiagnostic>(TupleConverterContext.Invoke(context, tracker, valueTupleSymbol));
        var (descriptor, messageArguments) = Assert.Single(result.DiagnosticArguments);
        Assert.NotNull(messageArguments);
        Assert.Equal(2, messageArguments.Length);
        Assert.NotNull(result);
        Assert.Equal(SourceStatus.Diagnostic, result.Status);
        Assert.Equal(Constants.TypeNotRecognized, descriptor);
        Assert.Equal("Tuple", messageArguments[0]);
        Assert.Equal(Symbols.GetSymbolDiagnosticDisplayString(valueTupleSymbol), messageArguments[1]);

        var generator = new SourceGenerator();
        _ = CompilationModule.RunGenerators(compilation, out var diagnostics, generator);
        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
        Assert.Equal("SourceGeneratorInclude<System.ValueTuple>", diagnostic.Location.GetSourceText());
        Assert.EndsWith("The converter could not be generated because the type could not be identified, pattern: Tuple, type: ValueTuple", diagnostic.ToString());
    }
}
