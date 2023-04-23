namespace Mikodev.Binary.SourceGeneration.Tests.DiagnosticTests;

using Microsoft.CodeAnalysis;
using System.Collections.Generic;
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
                [TupleKey(0)]
                public int Id;

                [TupleKey(1)]
                public string Content;

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

    public static IEnumerable<object[]> TupleKeyNotSequentialData()
    {
        var a =
            """
            namespace Tests;

            using Mikodev.Binary.Attributes;

            [SourceGeneratorContext]
            [SourceGeneratorInclude<Alpha>]
            partial class TestGeneratorContext { }

            [TupleObject]
            class Alpha
            {
                [TupleKey(-1)]
                public int A;

                [TupleKey(0)]
                public int B;

                [TupleKey(1)]
                public int C;
            }
            """;
        var b =
            """
            namespace Tests;

            using Mikodev.Binary.Attributes;

            [SourceGeneratorContext]
            [SourceGeneratorInclude<Bravo>]
            partial class TestGeneratorContext { }

            [TupleObject]
            class Bravo
            {
                [TupleKey(3)]
                public byte C;

                [TupleKey(0)]
                public byte B;

                [TupleKey(1)]
                public ushort D;
            }
            """;
        yield return new object[] { a, "Alpha" };
        yield return new object[] { b, "Bravo" };
    }

    [Theory(DisplayName = "Tuple Key Not Sequential")]
    [MemberData(nameof(TupleKeyNotSequentialData))]
    public void TupleKeyNotSequentialTest(string source, string typeName)
    {
        var compilation = CompilationModule.CreateCompilation(source);
        var generator = new SourceGenerator();
        _ = CompilationModule.RunGenerators(compilation, out var diagnostics, generator);
        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.EndsWith($"Tuple key must start at zero and must be sequential, type: {typeName}", diagnostic.ToString());
        Assert.Equal(typeName, diagnostic.Location.GetSourceText());
    }
}
