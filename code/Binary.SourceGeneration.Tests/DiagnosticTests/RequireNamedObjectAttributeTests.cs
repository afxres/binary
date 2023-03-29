namespace Mikodev.Binary.SourceGeneration.Tests.DiagnosticTests;

using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using Xunit;

public class RequireNamedObjectAttributeTests
{
    public static IEnumerable<object[]> ExplicitData()
    {
        var a =
            """
            namespace TestNamespace;

            using Mikodev.Binary.Attributes;

            [SourceGeneratorContext]
            [SourceGeneratorInclude<Alpha>]
            public partial class TestGeneratorContext { }

            [TupleObject]
            public class Alpha
            {
                [NamedKey("1")]
                public int Id { get; set; }

                [TupleKey(0)]
                public int Key { get; set; }
            }
            """;
        var b =
            """
            namespace TestNamespace;

            using Mikodev.Binary.Attributes;

            [SourceGeneratorContext]
            [SourceGeneratorInclude<Bravo>]
            public partial class TestGeneratorContext { }

            public class Bravo
            {
                [NamedKey("string")]
                public string Name { get; set; }
            }
            """;
        yield return new object[] { a, "Id", "Alpha" };
        yield return new object[] { b, "Name", "Bravo" };
    }

    public static IEnumerable<object[]> ImplicitData()
    {
        var a =
            """
            namespace TestNamespace;

            using Mikodev.Binary.Attributes;

            [SourceGeneratorContext]
            [SourceGeneratorInclude<Main>]
            public partial class TestGeneratorContext { }

            public class Back
            {
                [NamedKey("key")]
                public long Key;
            }

            public class Main
            {
                public Back Back { get; set; }
            }
            """;
        yield return new object[] { a, "Key", "Back" };
    }

    [Theory(DisplayName = "Require 'NamedObjectAttribute' Test")]
    [MemberData(nameof(ExplicitData))]
    [MemberData(nameof(ImplicitData))]
    public void RequireNamedObjectAttributeTest(string source, string memberName, string typeName)
    {
        var compilation = CompilationModule.CreateCompilation(source);
        var generator = new SourceGenerator();
        _ = CompilationModule.RunGenerators(compilation, out var diagnostics, generator);
        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
        Assert.EndsWith($"Require 'NamedObjectAttribute' for 'NamedKeyAttribute', this attribute will be ignored, member name: {memberName}, type: {typeName}", diagnostic.ToString());
        Assert.Matches(@"NamedKey\(.*\)", diagnostic.Location.GetSourceText());
    }
}
