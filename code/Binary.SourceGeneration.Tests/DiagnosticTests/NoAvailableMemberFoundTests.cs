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
        yield return new object[] { namedObject, "TestNamedClass" };
        yield return new object[] { tupleObject, "AnotherTupleClass" };
    }

    public static IEnumerable<object[]> NoAvailableMemberReferencedTypeData()
    {
        var a =
            """
            namespace TestNamespace;

            using Mikodev.Binary.Attributes;

            [SourceGeneratorContext]
            [SourceGeneratorInclude<L1Object>]
            public partial class TestGeneratorContext { }

            [TupleObject]
            public class L1Object
            {
                [TupleKey(0)]
                public L2Object? L2 { get; set; }
            }

            [NamedObject]
            public class L2Object { }
            """;
        var b =
            """
            namespace TestNamespace;

            using Mikodev.Binary.Attributes;

            [SourceGeneratorContext]
            [SourceGeneratorInclude<R1Object>]
            public partial class TestGeneratorContext { }

            public class R1Object
            {
                public R2Object? R2 { get; set; }
            }

            [TupleObject]
            public class R2Object { }
            """;
        yield return new object[] { a, "L2Object" };
        yield return new object[] { b, "R2Object" };
    }

    [Theory(DisplayName = "No Available Member Found")]
    [MemberData(nameof(NoAvailableMemberData))]
    [MemberData(nameof(NoAvailableMemberReferencedTypeData))]
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

    public static IEnumerable<object[]> NoAvailableMemberPlainObjectData()
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
        yield return new object[] { plainObject, "PlainClass" };
    }

    [Theory(DisplayName = "No Available Member Found On Plain Object")]
    [MemberData(nameof(NoAvailableMemberPlainObjectData))]
    public void NoAvailableMemberPlainObjectTest(string source, string typeName)
    {
        var compilation = CompilationModule.CreateCompilation(source);
        var generator = new SourceGenerator();
        _ = CompilationModule.RunGenerators(compilation, out var diagnostics, generator);
        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
        Assert.EndsWith($"No converter generated, type: {typeName}", diagnostic.ToString());
        Assert.Contains(typeName, diagnostic.Location.GetSourceText());
    }

    public static IEnumerable<object[]> NoAvailableMemberReferencedTypePlainObjectData()
    {
        var a =
            """
            namespace TestNamespace;

            using Mikodev.Binary.Attributes;

            [SourceGeneratorContext]
            [SourceGeneratorInclude<N1Object>]
            public partial class TestGeneratorContext { }

            public class N1Object
            {
                public N2Object? N2 { get; set; }
            }

            public class N2Object { }
            """;
        yield return new object[] { a };
    }

    [Theory(DisplayName = "No Available Member Found On Referenced Plain Object")]
    [MemberData(nameof(NoAvailableMemberReferencedTypePlainObjectData))]
    public void NoAvailableMemberReferencedTypePlainObjectTest(string source)
    {
        var compilation = CompilationModule.CreateCompilation(source);
        var generator = new SourceGenerator();
        _ = CompilationModule.RunGenerators(compilation, out var diagnostics, generator);
        Assert.Empty(diagnostics);
    }
}
