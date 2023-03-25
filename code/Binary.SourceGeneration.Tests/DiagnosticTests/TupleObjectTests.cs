namespace Mikodev.Binary.SourceGeneration.Tests.DiagnosticTests;

using Microsoft.CodeAnalysis;
using Xunit;

public class TupleObjectTests
{
    [Fact(DisplayName = "Tuple Key Duplicated")]
    public void KeyDuplicated()
    {
        var source =
            """
            namespace Mikodev.Binary.SourceGeneration.Tests;

            using Mikodev.Binary.Attributes;

            [SourceGeneratorContext]
            [SourceGeneratorInclude<Equipment>]
            public partial class TestSourceGeneratorContext { }

            [TupleObject]
            public class Equipment
            {
                [TupleKey(2)]
                public long Tag { get; set; }

                [TupleKey(2)]
                public string Category { get; set; }
            }
            """;
        var compilation = CompilationModule.CreateCompilation(source);
        var generator = new SourceGenerator();
        _ = CompilationModule.RunGenerators(compilation, out var diagnostics, generator);
        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.Contains("Tuple key duplicated, key: 2", diagnostic.ToString());
        Assert.Contains("""TupleKey(2)""", diagnostic.Location.GetSourceText());
    }
}
