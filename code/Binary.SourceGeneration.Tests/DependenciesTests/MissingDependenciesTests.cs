namespace Mikodev.Binary.SourceGeneration.Tests.DependenciesTests;

using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
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
        var builder = ImmutableArray.CreateBuilder<MetadataReference>();
        builder.Add(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));
        builder.Add(MetadataReference.CreateFromFile(Assembly.Load(new AssemblyName("System.Runtime")).Location));
        var compilation = CompilationModule.CreateCompilation(source, builder.ToImmutable());
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
        var builder = ImmutableArray.CreateBuilder<MetadataReference>();
        builder.Add(MetadataReference.CreateFromFile(typeof(IConverter).Assembly.Location));
        builder.Add(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));
        builder.Add(MetadataReference.CreateFromFile(Assembly.Load(new AssemblyName("System.Runtime")).Location));
        var compilation = CompilationModule.CreateCompilation(source, builder.ToImmutable());
        var generator = new SourceGenerator();
        _ = CompilationModule.RunGenerators(compilation, out var diagnostics, generator);
        Assert.Empty(diagnostics);
    }
}
