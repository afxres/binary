namespace Mikodev.Binary.SourceGeneration.ObjectCrossTests;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using System.Reflection;
using Xunit;

public static class CompilationModule
{
    public static Compilation CreateCompilationFromThisAssembly()
    {
        var references = new List<MetadataReference>();
        references.Add(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));
        references.Add(MetadataReference.CreateFromFile(Assembly.Load(new AssemblyName("System.Runtime")).Location));
        references.Add(MetadataReference.CreateFromFile(typeof(CompilationModule).Assembly.Location));

        const string AssemblyName = "TestAssembly";
        var compilation = CSharpCompilation.Create(
            AssemblyName,
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));
        var diagnostics = compilation.GetDiagnostics();
        Assert.Empty(diagnostics);
        return compilation;
    }
}
