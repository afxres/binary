namespace Mikodev.Binary.SourceGeneration;

using Microsoft.CodeAnalysis;

public class SymbolMemberInfo
{
    public ISymbol Symbol { get; }

    public ITypeSymbol TypeSymbol { get; }

    public string Name { get; }

    public string NameInSourceCode { get; }

    public bool IsReadOnly { get; }

    public SymbolMemberInfo(ISymbol symbol, ITypeSymbol typeSymbol, bool @readonly)
    {
        var name = symbol.Name;
        Name = name;
        NameInSourceCode = Symbols.GetNameInSourceCode(name);
        Symbol = symbol;
        TypeSymbol = typeSymbol;
        IsReadOnly = @readonly;
    }

    public SymbolMemberInfo(IFieldSymbol field) : this(field, field.Type, field.IsReadOnly) { }

    public SymbolMemberInfo(IPropertySymbol property) : this(property, property.Type, Symbols.HasPublicSetter(property) is false) { }
}
