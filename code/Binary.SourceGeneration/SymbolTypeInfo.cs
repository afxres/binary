namespace Mikodev.Binary.SourceGeneration;

using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

public class SymbolTypeInfo(ITypeSymbol symbol, ImmutableSortedSet<string> conflict, ImmutableArray<ISymbol> originalMembers, ImmutableArray<ISymbol> filteredMembers, ImmutableHashSet<ISymbol> requiredMembers)
{
    public ITypeSymbol Symbol { get; } = symbol;

    public ImmutableSortedSet<string> Conflict { get; } = conflict;

    public ImmutableArray<ISymbol> OriginalFieldsAndProperties { get; } = originalMembers;

    public ImmutableArray<ISymbol> FilteredFieldsAndProperties { get; } = filteredMembers;

    public ImmutableHashSet<ISymbol> RequiredFieldsAndProperties { get; } = requiredMembers;

    public static SymbolTypeInfo Create(Compilation compilation, ITypeSymbol symbol, CancellationToken cancellation)
    {

        var originalMembers = Symbols.GetAllFieldsAndProperties(compilation, symbol, out var conflict, cancellation);
        if (conflict.Count is not 0)
            return new SymbolTypeInfo(symbol, conflict, ImmutableArray.Create<ISymbol>(), ImmutableArray.Create<ISymbol>(), ImmutableHashSet.Create<ISymbol>(SymbolEqualityComparer.Default));
        var filteredMembers = Symbols.FilterFieldsAndProperties(originalMembers, cancellation);
        var requiredMembers = originalMembers.Where(Symbols.IsRequired).ToImmutableHashSet(SymbolEqualityComparer.Default);
        return new SymbolTypeInfo(symbol, ImmutableSortedSet.Create<string>(), originalMembers, filteredMembers, requiredMembers);
    }
}
