namespace Mikodev.Binary.SourceGeneration;

using Microsoft.CodeAnalysis;

public class SymbolNamedMemberInfo : SymbolMemberInfo
{
    public bool IsOptional { get; }

    public string NamedKeyLiteral { get; }

    public SymbolNamedMemberInfo(IFieldSymbol field, string namedKeyLiteral, bool optional) : base(field)
    {
        IsOptional = optional;
        NamedKeyLiteral = namedKeyLiteral;
    }

    public SymbolNamedMemberInfo(IPropertySymbol property, string namedKeyLiteral, bool optional) : base(property)
    {
        IsOptional = optional;
        NamedKeyLiteral = namedKeyLiteral;
    }
}
