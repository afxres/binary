namespace Mikodev.Binary.SourceGeneration;

using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

public class SymbolTypeInfo(ITypeSymbol symbol, ImmutableArray<ISymbol> originalMembers, ImmutableArray<ISymbol> filteredMembers, ImmutableHashSet<ISymbol> requiredMembers)
{
    public ITypeSymbol Symbol { get; } = symbol;

    public ImmutableArray<ISymbol> OriginalFieldsAndProperties { get; } = originalMembers;

    public ImmutableArray<ISymbol> FilteredFieldsAndProperties { get; } = filteredMembers;

    public ImmutableHashSet<ISymbol> RequiredFieldsAndProperties { get; } = requiredMembers;

    public static SymbolTypeInfo Create(ITypeSymbol symbol, CancellationToken cancellation)
    {
        var originalMembers = Symbols.GetAllFieldsAndProperties(symbol, cancellation);
        var filteredMembers = Symbols.FilterFieldsAndProperties(originalMembers, cancellation);
        var requiredMembers = originalMembers.Where(Symbols.IsRequired).ToImmutableHashSet(SymbolEqualityComparer.Default);
        return new SymbolTypeInfo(symbol, originalMembers, filteredMembers, requiredMembers);
    }
}
