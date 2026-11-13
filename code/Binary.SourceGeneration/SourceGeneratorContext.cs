namespace Mikodev.Binary.SourceGeneration;

using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public class SourceGeneratorContext(Compilation compilation, Action<Diagnostic> collector, CancellationToken cancellation)
{
    private readonly Action<Diagnostic> collector = collector;

    private readonly Dictionary<Type, object> resources = [];

    private readonly Dictionary<ITypeSymbol, string> typeFullNameCache = new Dictionary<ITypeSymbol, string>(SymbolEqualityComparer.Default);

    private readonly Dictionary<ITypeSymbol, string> converterTypeFullNameCache = new Dictionary<ITypeSymbol, string>(SymbolEqualityComparer.Default);

    private readonly Dictionary<ITypeSymbol, SymbolTypeInfo> typeInfoCache = new Dictionary<ITypeSymbol, SymbolTypeInfo>(SymbolEqualityComparer.Default);

    private readonly Dictionary<ITypeSymbol, SymbolTypeKind> validationCache = new Dictionary<ITypeSymbol, SymbolTypeKind>(SymbolEqualityComparer.Default);

    public Compilation Compilation { get; } = compilation;

    public CancellationToken CancellationToken { get; } = cancellation;

    // the cache slot is identified by the type of the cached value,
    // so each slot must own a distinct wrapper type instead of sharing one
    public T GetOrCreateResource<T>(Func<Compilation, T> creator) where T : class
    {
        var dictionary = this.resources;
        var key = typeof(T);
        if (dictionary.TryGetValue(key, out var result) is false)
            dictionary.Add(key, result = creator.Invoke(Compilation));
        return (T)result;
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
            dictionary.Add(symbol, result = $"Mikodev.Binary.Converter<{GetTypeFullName(symbol)}>");
        return result;
    }

    public AttributeData? GetAttribute(ISymbol symbol, string attributeTypeName)
    {
        return symbol.GetAttributes().FirstOrDefault(x => IsType(x.AttributeClass, attributeTypeName));
    }

    public SymbolTypeKind ValidateType(ITypeSymbol symbol)
    {
        var dictionary = this.validationCache;
        if (dictionary.TryGetValue(symbol, out var result) is false)
            dictionary.Add(symbol, result = Symbols.ValidateType(this, symbol));
        return result;
    }

    public bool IsType(ISymbol? symbol, string typeName)
    {
        return SymbolEqualityComparer.Default.Equals(symbol, Compilation.GetTypeByMetadataName(typeName));
    }

    public void Collect(Diagnostic diagnostic)
    {
        this.collector.Invoke(diagnostic);
    }
}
