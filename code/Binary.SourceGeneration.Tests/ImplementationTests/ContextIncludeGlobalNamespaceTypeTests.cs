namespace Mikodev.Binary.SourceGeneration.Tests.ImplementationTests;

using System.Collections.Generic;
using Xunit;

public class ContextIncludeGlobalNamespaceTypeTests
{
    public static IEnumerable<object[]> ContextIncludeGlobalNamespaceTypeData()
    {
        var a =
            """
            using Mikodev.Binary.Attributes;

            class Alpha
            {
                public int Data { get; set; }
            }

            namespace Tests
            {
                [SourceGeneratorContext]
                [SourceGeneratorInclude<Alpha>]
                public partial class TestContext { }
            }
            """;
        var b =
            """
            using Mikodev.Binary.Attributes;

            class Bravo<T>
            {
                public T Data;
            }

            namespace Tests
            {
                [SourceGeneratorContext]
                [SourceGeneratorInclude<Bravo<int>>]
                [SourceGeneratorInclude<Bravo<string>>]
                public partial class TestContext { }
            }
            """;
        yield return new object[] { a };
        yield return new object[] { b };
    }

    [Theory(DisplayName = "Context Including Global Namespace Type Test")]
    [MemberData(nameof(ContextIncludeGlobalNamespaceTypeData))]
    public void ContextIncludeGlobalNamespaceTypeTest(string source)
    {
        var compilation = CompilationModule.CreateCompilation(source);
        var generator = new SourceGenerator();
        _ = CompilationModule.RunGenerators(compilation, out var diagnostics, generator);
        Assert.Empty(diagnostics);
    }
}
