namespace Mikodev.Binary.SourceGeneration;

using Microsoft.CodeAnalysis;

public class SymbolMemberInfo(ISymbol symbol, ITypeSymbol typeSymbol, bool @readonly)
{
    public ISymbol Symbol { get; } = symbol;

    public ITypeSymbol Type { get; } = typeSymbol;

    public string NameInSourceCode { get; } = Symbols.GetNameInSourceCode(symbol.Name);

    public bool IsReadOnly { get; } = @readonly;

    public SymbolMemberInfo(IFieldSymbol field) : this(field, field.Type, field.IsReadOnly) { }

    public SymbolMemberInfo(IPropertySymbol property) : this(property, property.Type, Symbols.HasPublicSetter(property) is false) { }
}
