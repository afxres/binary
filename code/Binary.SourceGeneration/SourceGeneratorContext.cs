namespace Mikodev.Binary.SourceGeneration;

using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Threading;

public class SourceGeneratorContext
{
    private readonly Queue<ITypeSymbol> referencedTypes;

    private readonly Dictionary<string, ITypeSymbol?> types;

    public Dictionary<string, object> Resources { get; }

    public Compilation Compilation { get; }

    public CancellationToken CancellationToken { get; }

    public SourceGeneratorContext(Compilation compilation, Queue<ITypeSymbol> referencedTypes, CancellationToken cancellation)
    {
        Resources = new Dictionary<string, object>();
        Compilation = compilation;
        CancellationToken = cancellation;
        this.types = new Dictionary<string, ITypeSymbol?>();
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
