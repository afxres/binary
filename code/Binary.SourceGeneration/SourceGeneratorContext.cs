namespace Mikodev.Binary.SourceGeneration;

using Microsoft.CodeAnalysis;
using System.Collections.Generic;

public class SourceGeneratorContext
{
    private readonly Queue<ITypeSymbol> referencedTypes;

    private readonly Dictionary<string, ITypeSymbol?> types = new Dictionary<string, ITypeSymbol?>();

    public string HintNameUnit { get; }

    public string Name { get; }

    public string Namespace { get; }

    public Compilation Compilation { get; }

    public SourceProductionContext SourceProductionContext { get; }

    public Dictionary<string, object> Resources { get; } = new Dictionary<string, object>();

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

    public INamedTypeSymbol? GetNamedTypeSymbol(string typeName)
    {
        var types = this.types;
        if (types.TryGetValue(typeName, out var type) is false)
            types.Add(typeName, type = Compilation.GetTypeByMetadataName(typeName));
        return (INamedTypeSymbol?)type;
    }

    public bool Equals(ISymbol? symbol, string typeName)
    {
        return SymbolEqualityComparer.Default.Equals(symbol, GetNamedTypeSymbol(typeName));
    }
}
