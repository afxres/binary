namespace Mikodev.Binary.SourceGeneration;

using Microsoft.CodeAnalysis;

public class SymbolObjectMemberInfo(ISymbol symbol) : SymbolMemberInfo(symbol)
{
    public bool IsReadOnly { get; } = Symbols.IsReadOnlyFieldOrProperty(symbol);

    public string EscapedName { get; } = Symbols.EscapeIdentifier(symbol.Name);
}
