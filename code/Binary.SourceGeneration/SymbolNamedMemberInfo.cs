namespace Mikodev.Binary.SourceGeneration;

using Microsoft.CodeAnalysis;

public class SymbolNamedMemberInfo(ISymbol symbol, string namedKeyLiteral, bool optional) : SymbolMemberInfo(symbol)
{
    public bool IsOptional { get; } = optional;

    public string NamedKeyLiteral { get; } = namedKeyLiteral;
}
