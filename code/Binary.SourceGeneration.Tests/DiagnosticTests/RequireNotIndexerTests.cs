namespace Mikodev.Binary.SourceGeneration.Tests.DiagnosticTests;

using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using Xunit;

public class RequireNotIndexerTests
{
    public static IEnumerable<object[]> IndexerData()
    {
        var a =
            """
            namespace Tests;

            using Mikodev.Binary.Attributes;

            [SourceGeneratorContext]
            [SourceGeneratorInclude<Alpha>]
            partial class AlphaSourceGeneratorContext { }

            [NamedObject]
            public class Alpha
            {
                [NamedKey("this")]
                public int this[int key] => key;
            }
            """;
        var b =
            """
            namespace Tests;

            using Mikodev.Binary.Attributes;

            [SourceGeneratorContext]
            [SourceGeneratorInclude<Bravo>]
            partial class BravoSourceGeneratorContext { }

            [TupleObject]
            public class Bravo
            {
                [TupleKey(0)]
                public string this[string key] => key;
            }
            """;
        yield return new object[] { a, "Alpha" };
        yield return new object[] { b, "Bravo" };
    }

    [Theory(DisplayName = "Require Not Indexer")]
    [MemberData(nameof(IndexerData))]
    public void RequireNotIndexer(string source, string typeName)
    {
        var compilation = CompilationModule.CreateCompilation(source);
        var generator = new SourceGenerator();
        _ = CompilationModule.RunGenerators(compilation, out var diagnostics, generator);
        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.EndsWith($"Require not an indexer, type: {typeName}", diagnostic.ToString());
        Assert.Equal("this", diagnostic.Location.GetSourceText());
    }
}
