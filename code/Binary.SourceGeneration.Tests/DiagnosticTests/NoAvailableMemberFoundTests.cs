namespace Mikodev.Binary.SourceGeneration.Tests.DiagnosticTests;

using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using Xunit;

public class NoAvailableMemberFoundTests
{
    public static IEnumerable<object[]> NoAvailableMemberData()
    {
        var namedObject =
            """
            namespace Tests;

            using Mikodev.Binary.Attributes;

            [SourceGeneratorContext]
            [SourceGeneratorInclude<TestNamedClass>]
            public partial class TestSourceGeneratorContext { }

            [NamedObject]
            public class TestNamedClass { }
            """;
        var tupleObject =
            """
            namespace Tests;

            using Mikodev.Binary.Attributes;

            [SourceGeneratorContext]
            [SourceGeneratorInclude<AnotherTupleClass>]
            public partial class TestSourceGeneratorContext { }

            [TupleObject]
            public class AnotherTupleClass { }
            """;
        yield return new object[] { namedObject, "NamedObject", "TestNamedClass" };
        yield return new object[] { tupleObject, "TupleObject", "AnotherTupleClass" };
    }

    [Theory(DisplayName = "No Available Member Found")]
    [MemberData(nameof(NoAvailableMemberData))]
    public void NoAvailableMemberTest(string source, string location, string typeName)
    {
        var compilation = CompilationModule.CreateCompilation(source);
        var generator = new SourceGenerator();
        _ = CompilationModule.RunGenerators(compilation, out var diagnostics, generator);
        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.EndsWith($"No available member found, type: {typeName}", diagnostic.ToString());
        Assert.Contains(location, diagnostic.Location.GetSourceText());
    }
}
