#if NET11_0_OR_GREATER

namespace Mikodev.Binary.SourceGeneration.Tests.DirectContextTests;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Mikodev.Binary.SourceGeneration.Contexts;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading;
using Xunit;

public class UnionObjectContextTests
{
    public static IEnumerable<object[]> InvalidUnionObjectData()
    {
        var a =
            """
            // union empty body
            namespace UnionTests;

            using Mikodev.Binary.Attributes;
            using System.Runtime.CompilerServices;

            [SourceGeneratorContext]
            [SourceGeneratorInclude<FakeUnion>]
            partial class TestGeneratorContext { }

            [Union]
            readonly struct FakeUnion { }
            """;
        yield return [a];
    }

    [Theory(DisplayName = "Invalid Union Object Test")]
    [MemberData(nameof(InvalidUnionObjectData))]
    public void InvalidUnionObjectTest(string source)
    {
        var references = new List<MetadataReference>
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(IConverter).Assembly.Location),
            MetadataReference.CreateFromFile(Assembly.Load(new AssemblyName("System.Runtime")).Location),
            MetadataReference.CreateFromFile(typeof(ImmutableArray<object>).Assembly.Location)
        };
        var compilation = CSharpCompilation.Create(
            "TestAssembly",
            syntaxTrees: [CSharpSyntaxTree.ParseText(source, CompilationModule.ParseOptions)],
            references: [.. references],
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));
        var driver = CSharpGeneratorDriver.Create(generators: [new SourceGenerator().AsSourceGenerator()], parseOptions: CompilationModule.ParseOptions);
        _ = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var outputDiagnostics);
        var diagnostic = Assert.Single(outputDiagnostics);
        var outputCompilationDiagnostics = outputCompilation.GetDiagnostics();
        Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
        Assert.DoesNotContain(outputCompilationDiagnostics, x => x.Id is "CS0234");

        var tree = compilation.SyntaxTrees.First();
        var model = compilation.GetSemanticModel(tree);
        var nodes = tree.GetRoot().DescendantNodes();
        var declaration = nodes.OfType<StructDeclarationSyntax>().Last();
        var symbol = Assert.IsType<ITypeSymbol>(model.GetDeclaredSymbol(declaration), exactMatch: false);
        var context = new SourceGeneratorContext(compilation, _ => Assert.Fail("Invalid Call!"), CancellationToken.None);
        var tracker = new SourceGeneratorTracker(_ => Assert.Fail("Invalid Call!"));
        var result = Assert.IsType<SourceResult>(UnionObjectConverterContext.Invoke(context, tracker, symbol));
        Assert.NotNull(result);
        Assert.Equal(SourceStatus.Error, result.Status);
    }
}

#endif
