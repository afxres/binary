namespace Mikodev.Binary.SourceGeneration.Tests.DirectContextTests;

using Microsoft.CodeAnalysis;
using Mikodev.Binary.SourceGeneration.Contexts;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading;
using Xunit;

public class CollectionContextTests
{
    [Fact(DisplayName = "Unsupported Type Test")]
    public void UnsupportedTypeTest()
    {
        var unsupportedTypes = new[] { typeof(string), typeof(Stack<>), typeof(ConcurrentStack<>), typeof(ImmutableStack<>), typeof(IImmutableStack<>) }.ToHashSet();
        var unsupportedTypeAssemblyLocations = unsupportedTypes.Select(x => x.Assembly.Location);
        var basicAssemblyLocations = new[] { typeof(object).Assembly.Location, Assembly.Load(new AssemblyName("System.Runtime")).Location };
        var locations = basicAssemblyLocations.Concat(unsupportedTypeAssemblyLocations).ToHashSet();
        var references = locations.Select(x => (MetadataReference)MetadataReference.CreateFromFile(x)).ToImmutableArray();
        var compilation = CompilationModule.CreateCompilation(string.Empty, references);

        var unsupportedTypeSymbols = unsupportedTypes
            .Select(x => Assert.IsAssignableFrom<INamedTypeSymbol>(compilation.GetTypeByMetadataName(Assert.IsType<string>(x.FullName))))
            .ToList();
        var referenced = new Queue<ITypeSymbol>();
        var context = new SourceGeneratorContext(compilation, _ => Assert.Fail("Invalid Call!"), CancellationToken.None);
        var tracker = new SourceGeneratorTracker(referenced);
        var results = unsupportedTypeSymbols.Select(x => CollectionConverterContext.Invoke(context, tracker, x)).ToList();
        Assert.All(results, Assert.Null);

        var arraySymbol = compilation.CreateArrayTypeSymbol(compilation.GetSpecialType(SpecialType.System_String), 1);
        Assert.Null(CollectionConverterContext.Invoke(context, tracker, arraySymbol));
    }

    [Fact(DisplayName = "Unsupported Type Compilation Test")]
    public void UnsupportedTypeCompilationTest()
    {
        var source =
            """
            namespace Tests;

            using Mikodev.Binary.Attributes;
            using System.Collections.Concurrent;
            using System.Collections.Generic;
            using System.Collections.Immutable;

            [SourceGeneratorContext]
            [SourceGeneratorInclude<Stack<int>>]
            [SourceGeneratorInclude<Stack<string>>]
            [SourceGeneratorInclude<ConcurrentStack<int>>]
            [SourceGeneratorInclude<ConcurrentStack<string>>]
            [SourceGeneratorInclude<ImmutableStack<int>>]
            [SourceGeneratorInclude<ImmutableStack<string>>]
            [SourceGeneratorInclude<IImmutableStack<int>>]
            [SourceGeneratorInclude<IImmutableStack<string>>]
            partial class TestGeneratorContext { }
            """;
        var unsupportedTypes = new[] { typeof(Stack<>), typeof(ConcurrentStack<>), typeof(ImmutableStack<>), typeof(IImmutableStack<>) }.ToHashSet();
        var unsupportedTypeAssemblyLocations = unsupportedTypes.Select(x => x.Assembly.Location);
        var basicAssemblyLocations = new[] { typeof(object).Assembly.Location, Assembly.Load(new AssemblyName("System.Runtime")).Location, typeof(IConverter).Assembly.Location };
        var locations = basicAssemblyLocations.Concat(unsupportedTypeAssemblyLocations).ToHashSet();
        var references = locations.Select(x => (MetadataReference)MetadataReference.CreateFromFile(x)).ToImmutableArray();
        var compilation = CompilationModule.CreateCompilation(source, references);
        var generator = new SourceGenerator();
        _ = CompilationModule.RunGenerators(compilation, out var diagnostics, generator);
        Assert.All(diagnostics, x => Assert.Equal(DiagnosticSeverity.Warning, x.Severity));
        Assert.All(diagnostics, x => Assert.Contains("No converter generated", x.ToString()));
        var locationTexts = diagnostics.Select(x => x.Location.GetSourceText()).ToHashSet();
        Assert.Equal(8, locationTexts.Count);
        Assert.All(locationTexts, x => Assert.Contains(x, source));
    }
}
