namespace Mikodev.Binary.SourceGeneration;

using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public class SourceGeneratorContext
{
    private readonly Action<Diagnostic> diagnosticCollector;

    private readonly Dictionary<string, object> resources;

    private readonly Dictionary<string, ITypeSymbol?> types;

    private readonly Dictionary<ITypeSymbol, string> typeFullNameCache;

    private readonly Dictionary<ITypeSymbol, string> converterTypeFullNameCache;

    private readonly Dictionary<ITypeSymbol, SymbolTypeInfo> typeInfoCache;

    private readonly Dictionary<ITypeSymbol, bool> validationCache;

    public Compilation Compilation { get; }

    public CancellationToken CancellationToken { get; }

    public SourceGeneratorContext(Compilation compilation, Action<Diagnostic> diagnosticCollector, CancellationToken cancellation)
    {
        Compilation = compilation;
        CancellationToken = cancellation;
        this.diagnosticCollector = diagnosticCollector;
        this.resources = new Dictionary<string, object>();
        this.types = new Dictionary<string, ITypeSymbol?>();
        this.typeFullNameCache = new Dictionary<ITypeSymbol, string>(SymbolEqualityComparer.Default);
        this.converterTypeFullNameCache = new Dictionary<ITypeSymbol, string>(SymbolEqualityComparer.Default);
        this.typeInfoCache = new Dictionary<ITypeSymbol, SymbolTypeInfo>(SymbolEqualityComparer.Default);
        this.validationCache = new Dictionary<ITypeSymbol, bool>(SymbolEqualityComparer.Default);
    }

    public object GetOrCreateResource(string key, Func<Compilation, object> creator)
    {
        var dictionary = this.resources;
        if (dictionary.TryGetValue(key, out var result) is false)
            dictionary.Add(key, result = creator.Invoke(Compilation));
        return result;
    }

    public SymbolTypeInfo GetTypeInfo(ITypeSymbol symbol)
    {
        var dictionary = this.typeInfoCache;
        if (dictionary.TryGetValue(symbol, out var result) is false)
            dictionary.Add(symbol, result = new SymbolTypeInfo(symbol));
        return result;
    }

    public string GetTypeFullName(ITypeSymbol symbol)
    {
        var dictionary = this.typeFullNameCache;
        if (dictionary.TryGetValue(symbol, out var result) is false)
            dictionary.Add(symbol, result = Symbols.GetSymbolFullName(symbol));
        return result;
    }

    public string GetConverterTypeFullName(ITypeSymbol symbol)
    {
        var dictionary = this.converterTypeFullNameCache;
        if (dictionary.TryGetValue(symbol, out var result) is false)
            dictionary.Add(symbol, result = $"{Constants.ConverterTypeName}<{GetTypeFullName(symbol)}>");
        return result;
    }

    public AttributeData? GetAttribute(ISymbol symbol, string attributeTypeName)
    {
        return symbol.GetAttributes().FirstOrDefault(x => Equals(x.AttributeClass, attributeTypeName));
    }

    public INamedTypeSymbol? GetNamedTypeSymbol(string typeName)
    {
        var dictionary = this.types;
        if (dictionary.TryGetValue(typeName, out var type) is false)
            dictionary.Add(typeName, type = Compilation.GetTypeByMetadataName(typeName));
        return (INamedTypeSymbol?)type;
    }

    public bool ValidateType(ITypeSymbol symbol)
    {
        var dictionary = this.validationCache;
        if (dictionary.TryGetValue(symbol, out var result) is false)
            dictionary.Add(symbol, result = Symbols.ValidateType(this, symbol));
        return result;
    }

    public bool Equals(ISymbol? symbol, string typeName)
    {
        return SymbolEqualityComparer.Default.Equals(symbol, GetNamedTypeSymbol(typeName));
    }

    public void Collect(Diagnostic diagnostic)
    {
        this.diagnosticCollector.Invoke(diagnostic);
    }
}
