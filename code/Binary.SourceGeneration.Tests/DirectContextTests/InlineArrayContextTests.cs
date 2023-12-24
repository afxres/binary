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

public class InlineArrayContextTests
{
    public static IEnumerable<object[]> InvalidInlineArrayData()
    {
        var a =
            """
            // no field
            namespace Tests;

            using Mikodev.Binary.Attributes;
            using System.Runtime.CompilerServices;

            [SourceGeneratorContext]
            [SourceGeneratorInclude<InlineArrayWithoutField>]
            partial class TestGeneratorContext { }

            [InlineArray(1)]
            struct InlineArrayWithoutField { }
            """;
        var b =
            """
            // multiple fields
            namespace Tests;

            using Mikodev.Binary.Attributes;
            using System.Runtime.CompilerServices;

            [SourceGeneratorContext]
            [SourceGeneratorInclude<InlineArrayMultipleFields>]
            partial class TestGeneratorContext { }

            [InlineArray(1)]
            struct InlineArrayMultipleFields
            {
                public int A;

                public int B;
            }
            """;
        var c =
            """
            // no attribute parameter
            namespace Tests;

            using Mikodev.Binary.Attributes;
            using System.Runtime.CompilerServices;

            [SourceGeneratorContext]
            [SourceGeneratorInclude<InlineArrayNoAttributeParameter>]
            partial class TestGeneratorContext { }

            [InlineArray()]
            struct InlineArrayNoAttributeParameter
            {
                public int A;
            }
            """;
        yield return new object[] { a };
        yield return new object[] { b };
        yield return new object[] { c };
    }

    [Theory(DisplayName = "Invalid Inline Array Test")]
    [MemberData(nameof(InvalidInlineArrayData))]
    public void InvalidInlineArrayTest(string source)
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
            syntaxTrees: new[] { CSharpSyntaxTree.ParseText(source, CompilationModule.ParseOptions) },
            references: references.ToArray(),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));
        var driver = CSharpGeneratorDriver.Create(generators: new ISourceGenerator[] { new SourceGenerator().AsSourceGenerator() }, parseOptions: CompilationModule.ParseOptions);
        _ = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var outputDiagnostics);
        var diagnostic = Assert.Single(outputDiagnostics);
        var outputCompilationDiagnostics = outputCompilation.GetDiagnostics();
        Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
        Assert.DoesNotContain(outputCompilationDiagnostics, x => x.Id is "CS0234");

        var tree = compilation.SyntaxTrees.First();
        var model = compilation.GetSemanticModel(tree);
        var nodes = tree.GetRoot().DescendantNodes();
        var declaration = nodes.OfType<StructDeclarationSyntax>().Last();
        var symbol = Assert.IsAssignableFrom<ITypeSymbol>(model.GetDeclaredSymbol(declaration));
        var context = new SourceGeneratorContext(compilation, _ => Assert.Fail("Invalid Call!"), CancellationToken.None);
        var tracker = new SourceGeneratorTracker(_ => Assert.Fail("Invalid Call!"));
        var result = InlineArrayConverterContext.Invoke(context, tracker, symbol);
        Assert.NotNull(result);
        Assert.Empty(result.SourceCode);
        Assert.Empty(result.ConverterCreatorTypeName);
        Assert.Equal(SourceStatus.Skip, result.Status);
    }
}
