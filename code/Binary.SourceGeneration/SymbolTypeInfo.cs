namespace Mikodev.Binary.SourceGeneration;

using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

public class SymbolTypeInfo
{
    private readonly ITypeSymbol symbol;

    private readonly Lazy<ImmutableArray<ISymbol>> originalMembers;

    private readonly Lazy<ImmutableArray<ISymbol>> filteredMembers;

    private readonly Lazy<ImmutableHashSet<ISymbol>> requiredMembers;

    public ITypeSymbol Symbol => this.symbol;

    public ImmutableArray<ISymbol> OriginalMembers => this.originalMembers.Value;

    public ImmutableArray<ISymbol> FilteredMembers => this.filteredMembers.Value;

    public ImmutableHashSet<ISymbol> RequiredMembers => this.requiredMembers.Value;

    public SymbolTypeInfo(ITypeSymbol symbol)
    {
        const LazyThreadSafetyMode LazyMode = LazyThreadSafetyMode.ExecutionAndPublication;
        this.symbol = symbol;
        this.originalMembers = new Lazy<ImmutableArray<ISymbol>>(() => Symbols.GetAllFieldsAndProperties(this.symbol), LazyMode);
        this.filteredMembers = new Lazy<ImmutableArray<ISymbol>>(() => Symbols.FilterFieldsAndProperties(this.originalMembers.Value), LazyMode);
        this.requiredMembers = new Lazy<ImmutableHashSet<ISymbol>>(() => this.originalMembers.Value.Where(Symbols.IsRequired).ToImmutableHashSet(SymbolEqualityComparer.Default), LazyMode);
    }
}
