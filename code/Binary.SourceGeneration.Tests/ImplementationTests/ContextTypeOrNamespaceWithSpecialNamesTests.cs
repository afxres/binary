namespace Mikodev.Binary.SourceGeneration.Tests.ImplementationTests;

using System.Linq;
using Xunit;

public class ContextTypeOrNamespaceWithSpecialNamesTests
{
    [Fact(DisplayName = "Source Generator Context Name Or Namespace With Special Names Test")]
    public void ContextTypeOrNamespaceWithSpecialNamesTest()
    {
        var source =
            """
            using Mikodev.Binary.Attributes;

            namespace @class.yield.@return
            {
                [SourceGeneratorContext]
                partial class @public { }
            }

            namespace @internal.let
            {
                [SourceGeneratorContext]
                partial class @managed { }
            }

            namespace @true.region.@bool
            {
                [SourceGeneratorContext]
                partial class @false { }
            }
            """;
        var compilation = CompilationModule.CreateCompilation(source);
        var generator = new SourceGenerator();
        var compilationGenerated = CompilationModule.RunGenerators(compilation, out var diagnostics, generator);
        var syntaxTrees = compilationGenerated.SyntaxTrees;
        var filePaths = syntaxTrees.Select(x => x.FilePath).ToList();
        _ = Assert.Single(filePaths, x => x.EndsWith("public.0.g.cs"));
        _ = Assert.Single(filePaths, x => x.EndsWith("managed.0.g.cs"));
        _ = Assert.Single(filePaths, x => x.EndsWith("false.0.g.cs"));
        Assert.Empty(diagnostics);
    }
}
