namespace Mikodev.Binary.SourceGeneration.Tests.ImplementationTests;

using System.Linq;
using Xunit;

public class ContextTypeNameConflictTests
{
    [Fact(DisplayName = "Source Generator Context Name Conflict Test")]
    public void ContextTypeNameConflictTest()
    {
        var source =
            """
            using Mikodev.Binary.Attributes;

            namespace NS1
            {
                [SourceGeneratorContext]
                public partial class TestContext { }
            }

            namespace NS2
            {
                [SourceGeneratorContext]
                public partial class TestContext { }
            }
            """;
        var compilation = CompilationModule.CreateCompilation(source);
        var generator = new SourceGenerator();
        var compilationGenerated = CompilationModule.RunGenerators(compilation, out var diagnostics, generator);
        var syntaxTrees = compilationGenerated.SyntaxTrees;
        var filePaths = syntaxTrees.Select(x => x.FilePath).ToList();
        _ = Assert.Single(filePaths, x => x.EndsWith("TestContext.0.g.cs"));
        _ = Assert.Single(filePaths, x => x.EndsWith("TestContext.1.g.cs"));
        Assert.Empty(diagnostics);
    }
}
