namespace Mikodev.Binary.SourceGeneration.RandomizedTests;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Xunit;

/// <summary>
/// Parses and compiles a generated source string, then runs the source generator over it.
/// </summary>
internal static class CompilationModule
{
    private static readonly CSharpParseOptions ParseOptions = new(
        kind: SourceCodeKind.Regular,
        languageVersion: LanguageVersion.Preview,
        documentationMode: DocumentationMode.Parse);

    private static readonly ImmutableArray<MetadataReference> References =
    [
        MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(IConverter).Assembly.Location),
        MetadataReference.CreateFromFile(Assembly.Load(new AssemblyName("System.Runtime")).Location),
        MetadataReference.CreateFromFile(typeof(ImmutableArray<object>).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(LinkedList<object>).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(ReadOnlySequence<object>).Assembly.Location),
    ];

    public static Compilation RunGenerators(string source, out ImmutableArray<Diagnostic> diagnostics)
    {
        const string AssemblyName = "RandomizedAssembly";
        var compilation = CSharpCompilation.Create(
            AssemblyName,
            syntaxTrees: [CSharpSyntaxTree.ParseText(source, ParseOptions)],
            references: [.. References],
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));
        AssertNoErrors(compilation, source);
        var driver = CSharpGeneratorDriver.Create(generators: [new SourceGenerator().AsSourceGenerator()], parseOptions: ParseOptions);
        _ = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out diagnostics);
        AssertNoErrors(outputCompilation, source);
        return outputCompilation;
    }

    /// <summary>Reports the generated source together with the diagnostics, so a failing seed can be inspected.</summary>
    private static void AssertNoErrors(Compilation compilation, string source)
    {
        var errors = compilation.GetDiagnostics().Where(x => x.Severity is DiagnosticSeverity.Error).ToList();
        Assert.True(errors.Count is 0, $"Compilation failed:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}{Environment.NewLine}{source}");
    }
}
