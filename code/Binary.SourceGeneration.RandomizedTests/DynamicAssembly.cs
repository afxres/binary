namespace Mikodev.Binary.SourceGeneration.RandomizedTests;

using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using Xunit;

/// <summary>
/// Emits the compiled source into a collectible load context. The metadata references come from the
/// assemblies loaded in this process, so the emitted types share <see cref="IConverterCreator"/> and
/// <see cref="IConverter"/> with the test code and can be handed to a generator directly.
/// </summary>
internal sealed class DynamicAssembly : IDisposable
{
    private readonly AssemblyLoadContext context;

    public Assembly Assembly { get; }

    private DynamicAssembly(AssemblyLoadContext context, Assembly assembly)
    {
        this.context = context;
        this.Assembly = assembly;
    }

    public static DynamicAssembly Create(string source, string assemblyName)
    {
        var output = CompilationModule.RunGenerators(source, out var diagnostics);
        Assert.DoesNotContain(diagnostics, x => x.Severity is DiagnosticSeverity.Error);
        using var stream = new MemoryStream();
        var result = output.Emit(stream);
        Assert.True(result.Success, string.Join(Environment.NewLine, result.Diagnostics));
        stream.Position = 0;
        var context = new AssemblyLoadContext(assemblyName, isCollectible: true);
        var assembly = context.LoadFromStream(stream);
        return new DynamicAssembly(context, assembly);
    }

    public Type GetType(string name) => this.Assembly.GetType($"Randomized.{name}", throwOnError: true)!;

    public IReadOnlyDictionary<Type, IConverterCreator> GetConverterCreators(string typeName)
    {
        var type = this.Assembly.GetType(typeName, throwOnError: true)!;
        var property = type.GetProperty("ConverterCreators", BindingFlags.Public | BindingFlags.Static)!;
        return Assert.IsAssignableFrom<IReadOnlyDictionary<Type, IConverterCreator>>(property.GetValue(null));
    }

    public void Dispose() => this.context.Unload();
}
