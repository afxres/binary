namespace Mikodev.Binary.SourceGeneration.Tests;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Xunit;

internal class CompilationModule
{
    public static readonly CSharpParseOptions ParseOptions = new CSharpParseOptions(
        kind: SourceCodeKind.Regular,
        languageVersion: LanguageVersion.Preview,
        documentationMode: DocumentationMode.Parse);

    public static Compilation CreateCompilation(string source)
    {
        return CreateCompilation(source,
        [
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(IConverter).Assembly.Location),
            MetadataReference.CreateFromFile(Assembly.Load(new AssemblyName("System.Runtime")).Location),
            MetadataReference.CreateFromFile(typeof(ImmutableArray<object>).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(LinkedList<object>).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ReadOnlySequence<object>).Assembly.Location),
        ]);
    }

    public static Compilation CreateCompilation(string source, ImmutableArray<MetadataReference> references)
    {
        const string AssemblyName = "TestAssembly";
        var compilation = CSharpCompilation.Create(
            AssemblyName,
            syntaxTrees: [CSharpSyntaxTree.ParseText(source, ParseOptions)],
            references: [.. references],
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));
        var diagnostics = compilation.GetDiagnostics();
        Assert.Empty(diagnostics.Where(x => x.Severity is DiagnosticSeverity.Error));
        return compilation;
    }

    public static Compilation RunGenerators(Compilation compilation, out ImmutableArray<Diagnostic> outputDiagnostics, params IIncrementalGenerator[] generators)
    {
        var driver = CSharpGeneratorDriver.Create(generators: generators.Select(g => g.AsSourceGenerator()), parseOptions: ParseOptions);
        _ = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out outputDiagnostics);
        var diagnostics = compilation.GetDiagnostics();
        var outputCompilationDiagnostics = outputCompilation.GetDiagnostics();
        Assert.Empty(diagnostics.Where(x => x.Severity is DiagnosticSeverity.Error));
        Assert.Empty(outputCompilationDiagnostics.Where(x => x.Severity is DiagnosticSeverity.Error));
        return outputCompilation;
    }
}
