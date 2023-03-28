namespace Mikodev.Binary.SourceGeneration;

using Microsoft.CodeAnalysis;

public class SymbolMemberInfo
{
    public ISymbol Symbol { get; }

    public ITypeSymbol TypeSymbol { get; }

    public string Name { get; }

    public bool IsReadOnly { get; }

    public SymbolMemberInfo(IFieldSymbol field)
    {
        Name = field.Name;
        Symbol = field;
        TypeSymbol = field.Type;
        IsReadOnly = field.IsReadOnly;
    }

    public SymbolMemberInfo(IPropertySymbol property)
    {
        Name = property.Name;
        Symbol = property;
        TypeSymbol = property.Type;
        IsReadOnly = property.SetMethod?.DeclaredAccessibility is not Accessibility.Public;
    }
}
