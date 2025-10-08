namespace Mikodev.Binary.SourceGeneration.Tests.DiagnosticTests;

using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using Xunit;

public class NamedObjectTests
{
    [Fact(DisplayName = "Named Key Duplicated")]
    public void KeyDuplicated()
    {
        var source =
            """
            namespace Mikodev.Binary.SourceGeneration.Tests;

            using Mikodev.Binary.Attributes;

            [SourceGeneratorContext]
            [SourceGeneratorInclude<Person>]
            public partial class TestSourceGeneratorContext { }

            [NamedObject]
            public class Person
            {
                [NamedKey("entry")]
                public int Id { get; set; }

                [NamedKey("entry")]
                public string Name { get; set; }
            }
            """;
        var compilation = CompilationModule.CreateCompilation(source);
        var generator = new SourceGenerator();
        _ = CompilationModule.RunGenerators(compilation, out var diagnostics, generator);
        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.EndsWith("Named key duplicated, key: entry", diagnostic.ToString());
        Assert.Contains("""NamedKey("entry")""", diagnostic.Location.GetSourceText());
    }

    public static IEnumerable<object[]> NullOrEmptyKeyData()
    {
        var alpha =
            """
            namespace Tests;

            using Mikodev.Binary.Attributes;

            [SourceGeneratorContext]
            [SourceGeneratorInclude<Alpha>]
            public partial class AlphaSourceGeneratorContext { }

            [NamedObject]
            public class Alpha
            {
                [NamedKey(null)]
                public string? Name { get; set; }
            }
            """;
        var bravo =
            """
            namespace Tests;

            using Mikodev.Binary.Attributes;

            [SourceGeneratorContext]
            [SourceGeneratorInclude<Bravo>]
            public partial class BravoSourceGeneratorContext { }

            [NamedObject]
            public class Bravo
            {
                [NamedKey("")]
                public string? Name { get; set; }
            }
            """;
        yield return [alpha, "NamedKey(null)"];
        yield return [bravo, """NamedKey("")"""];
    }

    [Theory(DisplayName = "Key Null Or Empty")]
    [MemberData(nameof(NullOrEmptyKeyData))]
    public void KeyNullOrEmpty(string source, string location)
    {
        var compilation = CompilationModule.CreateCompilation(source);
        var generator = new SourceGenerator();
        _ = CompilationModule.RunGenerators(compilation, out var diagnostics, generator);
        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.EndsWith("Named key can not be null or empty.", diagnostic.ToString());
        Assert.Contains(location, diagnostic.Location.GetSourceText());
    }
}
