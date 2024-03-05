namespace Mikodev.Binary.SourceGeneration.Tests.DependenciesTests;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Reflection;
using Xunit;

public class MissingDependenciesTests
{
    [Fact(DisplayName = "No Binary Assembly")]
    public void NoBinaryAssemblyTest()
    {
        var source =
            """
            namespace Tests;

            public class Empty { }
            """;
        var compilation = CompilationModule.CreateCompilation(source,
        [
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(Assembly.Load(new AssemblyName("System.Runtime")).Location),
        ]);
        var generator = new SourceGenerator();
        _ = CompilationModule.RunGenerators(compilation, out var diagnostics, generator);
        Assert.Empty(diagnostics);
    }

    [Fact(DisplayName = "No Collections Assembly")]
    public void NoCollectionsAssemblyTest()
    {
        var source =
            """
            namespace TestNamespace;

            using Mikodev.Binary.Attributes;

            [SourceGeneratorContext]
            [SourceGeneratorInclude<Plain<int>>]
            [SourceGeneratorInclude<Plain<string>>]
            public partial class TestGeneratorContext { }

            public class Plain<T>
            {
                public T Data { get; set; }
            }
            """;
        var compilation = CompilationModule.CreateCompilation(source,
        [
            MetadataReference.CreateFromFile(typeof(IConverter).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(Assembly.Load(new AssemblyName("System.Runtime")).Location),
        ]);
        var generator = new SourceGenerator();
        var driver = CSharpGeneratorDriver.Create(generators: [generator.AsSourceGenerator()], parseOptions: CompilationModule.ParseOptions);
        _ = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out var outputDiagnostics);
        var diagnostics = compilation.GetDiagnostics();
        var outputCompilationDiagnostics = outputCompilation.GetDiagnostics();
        Assert.Empty(diagnostics);
        Assert.Empty(outputDiagnostics);
        Assert.NotEmpty(outputCompilationDiagnostics);
        Assert.All(outputCompilationDiagnostics, x => Assert.Contains("System.Collections.Frozen", x.Location.GetSourceText()));
    }
}
