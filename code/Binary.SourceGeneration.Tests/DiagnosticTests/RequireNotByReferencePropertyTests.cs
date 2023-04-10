namespace Mikodev.Binary.SourceGeneration.Tests.DiagnosticTests;

using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using Xunit;

public class RequireNotByReferencePropertyTests
{
    public static IEnumerable<object[]> ByReferencePropertyObjectData()
    {
        var a =
            """
            namespace Tests;

            using Mikodev.Binary.Attributes;
            using System;

            [SourceGeneratorContext]
            [SourceGeneratorInclude<Alpha>]
            public partial class TestGeneratorContext { }

            [NamedObject]
            public class Alpha
            {
                [NamedKey("address")]
                public ref int Location => throw new NotSupportedException();
            }
            """;
        var b =
            """
            namespace Tests;

            using Mikodev.Binary.Attributes;
            using System;

            [SourceGeneratorContext]
            [SourceGeneratorInclude<Bravo>]
            public partial class TestGeneratorContext { }

            [TupleObject]
            public class Bravo
            {
                [TupleKey(0)]
                public ref readonly string ReadOnlyLocation => throw new NotSupportedException();
            }
            """;
        yield return new object[] { a, "Location", "Alpha" };
        yield return new object[] { b, "ReadOnlyLocation", "Bravo" };
    }

    [Theory(DisplayName = "Require Not By Reference Property Test")]
    [MemberData(nameof(ByReferencePropertyObjectData))]
    public void RequireNotByReferencePropertyTest(string source, string memberName, string typeName)
    {
        var compilation = CompilationModule.CreateCompilation(source);
        var generator = new SourceGenerator();
        _ = CompilationModule.RunGenerators(compilation, out var diagnostics, generator);
        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.EndsWith($"Require not by reference property, member name: {memberName}, containing type: {typeName}", diagnostic.ToString());
        Assert.Matches(memberName, diagnostic.Location.GetSourceText());
    }

    public static IEnumerable<object[]> ByReferencePropertyPlainObjectData()
    {
        var a =
            """
            namespace Tests;

            using Mikodev.Binary.Attributes;
            using System;

            [SourceGeneratorContext]
            [SourceGeneratorInclude<Alpha>]
            public partial class TestGeneratorContext { }

            public class Alpha
            {
                public ref int Location => throw new NotSupportedException();
            }
            """;
        var b =
            """
            namespace Tests;

            using Mikodev.Binary.Attributes;
            using System;

            [SourceGeneratorContext]
            [SourceGeneratorInclude<Bravo>]
            public partial class TestGeneratorContext { }

            public class Bravo
            {
                public ref readonly long ReadOnlyLocation => throw new NotSupportedException();
            }
            """;
        yield return new object[] { a, "Alpha" };
        yield return new object[] { b, "Bravo" };
    }

    [Theory(DisplayName = "Require By Reference Property Plain Object Test")]
    [MemberData(nameof(ByReferencePropertyPlainObjectData))]
    public void RequireNotByReferencePropertyPlainObjectTest(string source, string typeName)
    {
        // no converter generated because by reference properties are ignored
        var compilation = CompilationModule.CreateCompilation(source);
        var generator = new SourceGenerator();
        _ = CompilationModule.RunGenerators(compilation, out var diagnostics, generator);
        var diagnostic = Assert.Single(diagnostics);
        Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
        Assert.EndsWith($"No converter generated, type: {typeName}", diagnostic.ToString());
        Assert.Contains($"SourceGeneratorInclude<{typeName}>", diagnostic.Location.GetSourceText());
    }
}
