namespace Mikodev.Binary.SourceGeneration;

using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public class SourceGeneratorContext(Compilation compilation, Action<Diagnostic> diagnosticCollector, CancellationToken cancellation)
{
    private readonly Action<Diagnostic> diagnosticCollector = diagnosticCollector;

    private readonly Dictionary<string, object> resources = new Dictionary<string, object>();

    private readonly Dictionary<ITypeSymbol, string> typeFullNameCache = new Dictionary<ITypeSymbol, string>(SymbolEqualityComparer.Default);

    private readonly Dictionary<ITypeSymbol, string> converterTypeFullNameCache = new Dictionary<ITypeSymbol, string>(SymbolEqualityComparer.Default);

    private readonly Dictionary<ITypeSymbol, SymbolTypeInfo> typeInfoCache = new Dictionary<ITypeSymbol, SymbolTypeInfo>(SymbolEqualityComparer.Default);

    private readonly Dictionary<ITypeSymbol, SymbolTypeKind> validationCache = new Dictionary<ITypeSymbol, SymbolTypeKind>(SymbolEqualityComparer.Default);

    public Compilation Compilation { get; } = compilation;

    public CancellationToken CancellationToken { get; } = cancellation;

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
            dictionary.Add(symbol, result = SymbolTypeInfo.Create(Compilation, symbol, CancellationToken));
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

    public SymbolTypeKind ValidateType(ITypeSymbol symbol)
    {
        var dictionary = this.validationCache;
        if (dictionary.TryGetValue(symbol, out var result) is false)
            dictionary.Add(symbol, result = Symbols.ValidateType(this, symbol));
        return result;
    }

    public bool Equals(ISymbol? symbol, string typeName)
    {
        return SymbolEqualityComparer.Default.Equals(symbol, Compilation.GetTypeByMetadataName(typeName));
    }

    public void Collect(Diagnostic diagnostic)
    {
        this.diagnosticCollector.Invoke(diagnostic);
    }
}
