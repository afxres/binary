namespace Mikodev.Binary.SourceGeneration.Tests.DiagnosticTests;

using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using Xunit;

public class NoAvailableMemberFoundTests
{
    public static IEnumerable<object[]> NoAvailableMemberData()
    {
        var plainObject =
            """
            namespace Tests;

            using Mikodev.Binary.Attributes;

            [SourceGeneratorContext]
            [SourceGeneratorInclude<PlainClass>]
            public partial class TestSourceGeneratorContext { }

            public class PlainClass { }
            """;
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
        yield return new object[] { plainObject, "PlainClass" };
        yield return new object[] { namedObject, "TestNamedClass" };
        yield return new object[] { tupleObject, "AnotherTupleClass" };
    }

    [Theory(DisplayName = "No Available Member Found")]
    [MemberData(nameof(NoAvailableMemberData))]
    public void NoAvailableMemberTest(string source, string typeName)
    {
        var compilation = CompilationModule.CreateCompilation(source);
        var generator = new SourceGenerator();
        _ = CompilationModule.RunGenerators(compilation, out var diagnostics, generator);
        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.EndsWith($"No available member found, type: {typeName}", diagnostic.ToString());
        Assert.Contains(typeName, diagnostic.Location.GetSourceText());
    }
}
