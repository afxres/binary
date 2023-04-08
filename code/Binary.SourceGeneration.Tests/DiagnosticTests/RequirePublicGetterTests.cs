namespace Mikodev.Binary.SourceGeneration.Tests.DiagnosticTests;

using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using Xunit;

public class RequirePublicGetterTests
{
    public static IEnumerable<object[]> NonPublicGetterData()
    {
        var a =
            """
            namespace Tests;

            using Mikodev.Binary.Attributes;

            [SourceGeneratorContext]
            [SourceGeneratorInclude<Alpha>]
            partial class AlphaSourceGeneratorContext { }

            [TupleObject]
            public class Alpha
            {
                [TupleKey(0)]
                public int Id { private get; set; }
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
                private string name;

                [TupleKey(0)]
                public string Name { set => name = value; }
            }
            """;
        var c =
            """
            namespace Tests;

            using Mikodev.Binary.Attributes;

            [SourceGeneratorContext]
            [SourceGeneratorInclude<Delta>]
            partial class DeltaSourceGeneratorContext { }

            [NamedObject]
            public class Delta
            {
                [NamedKey("internal")]
                public string Tags { internal get; set; }
            }
            """;
        yield return new object[] { a, "Id", "Alpha" };
        yield return new object[] { b, "Name", "Bravo" };
        yield return new object[] { c, "Tags", "Delta" };
    }

    [Theory(DisplayName = "Require Public Getter")]
    [MemberData(nameof(NonPublicGetterData))]
    public void RequirePublicGetter(string source, string memberName, string typeName)
    {
        var compilation = CompilationModule.CreateCompilation(source);
        var generator = new SourceGenerator();
        _ = CompilationModule.RunGenerators(compilation, out var diagnostics, generator);
        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.EndsWith($"Require a public getter, member name: {memberName}, containing type: {typeName}", diagnostic.ToString());
        Assert.Contains(memberName, diagnostic.Location.GetSourceText());
    }
}
