namespace Mikodev.Binary.SourceGeneration;

using Microsoft.CodeAnalysis;

public class SymbolObjectMemberInfo(ISymbol symbol) : SymbolMemberInfo(symbol)
{
    public bool IsReadOnly { get; } = Symbols.IsReadOnlyFieldOrProperty(symbol);

    public string NameInSourceCode { get; } = Symbols.GetNameInSourceCode(symbol.Name);
}
