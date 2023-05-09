namespace Mikodev.Binary.SourceGeneration;

using Microsoft.CodeAnalysis;

public class SymbolMemberInfo
{
    public ISymbol Symbol { get; }

    public ITypeSymbol Type { get; }

    public string Name { get; }

    public string NameInSourceCode { get; }

    public bool IsReadOnly { get; }

    public SymbolMemberInfo(ISymbol symbol, ITypeSymbol typeSymbol, bool @readonly)
    {
        Name = symbol.Name;
        Type = typeSymbol;
        Symbol = symbol;
        IsReadOnly = @readonly;
        NameInSourceCode = Symbols.GetNameInSourceCode(symbol.Name);
    }

    public SymbolMemberInfo(IFieldSymbol field) : this(field, field.Type, field.IsReadOnly) { }

    public SymbolMemberInfo(IPropertySymbol property) : this(property, property.Type, Symbols.HasPublicSetter(property) is false) { }
}
