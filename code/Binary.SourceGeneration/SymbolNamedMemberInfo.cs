namespace Mikodev.Binary.SourceGeneration;

using Microsoft.CodeAnalysis;

public class SymbolNamedMemberInfo : SymbolMemberInfo
{
    public bool IsOptional { get; }

    public string NamedKeyLiteral { get; }

    public SymbolNamedMemberInfo(ISymbol symbol, ITypeSymbol type, string name, bool @readonly, string namedKeyLiteral, bool optional) : base(symbol, type, name, @readonly)
    {
        IsOptional = optional;
        NamedKeyLiteral = namedKeyLiteral;
    }
}
