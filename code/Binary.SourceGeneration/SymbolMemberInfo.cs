namespace Mikodev.Binary.SourceGeneration;

using Microsoft.CodeAnalysis;

public class SymbolMemberInfo(ISymbol symbol)
{
    public ISymbol Symbol { get; } = symbol;

    public ITypeSymbol Type { get; } = symbol is IFieldSymbol field ? field.Type : ((IPropertySymbol)symbol).Type;
}
