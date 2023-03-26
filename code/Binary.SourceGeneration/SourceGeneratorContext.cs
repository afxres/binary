﻿namespace Mikodev.Binary.SourceGeneration;

using Microsoft.CodeAnalysis;
using System.Collections.Generic;

public class SourceGeneratorContext
{
    private readonly Queue<ITypeSymbol> referencedTypes;

    public string HintNameUnit { get; }

    public string Name { get; }

    public string Namespace { get; }

    public Compilation Compilation { get; }

    public SourceProductionContext SourceProductionContext { get; }

    public Dictionary<object, object?> Resources { get; } = new Dictionary<object, object?>();

    public SourceGeneratorContext(INamedTypeSymbol type, Compilation compilation, SourceProductionContext sourceProductionContext, Queue<ITypeSymbol> referencedTypes)
    {
        Name = type.Name;
        Namespace = type.ContainingNamespace.ToDisplayString();
        Compilation = compilation;
        SourceProductionContext = sourceProductionContext;
        HintNameUnit = Symbols.GetOutputFullName(type);
        this.referencedTypes = referencedTypes;
    }

    public void AddReferencedType(ITypeSymbol type)
    {
        this.referencedTypes.Enqueue(type);
    }

    public bool Equals(ISymbol? symbol, string typeName)
    {
        if (Resources.TryGetValue(typeName, out var type) is false)
            Resources.Add(typeName, type = Compilation.GetTypeByMetadataName(typeName));
        return SymbolEqualityComparer.Default.Equals(symbol, (INamedTypeSymbol?)type);
    }

    public void Throw(DiagnosticDescriptor descriptor, Location? location, object?[]? arguments)
    {
        var diagnostic = Diagnostic.Create(descriptor, location, arguments);
        SourceProductionContext.ReportDiagnostic(diagnostic);
        throw new SourceGeneratorException();
    }
}