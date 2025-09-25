namespace Mikodev.Binary.SourceGeneration;

using Microsoft.CodeAnalysis;

public class SymbolNamedObjectMemberInfo(ISymbol symbol, string namedKeyLiteral, bool optional) : SymbolObjectMemberInfo(symbol)
{
    public bool IsOptional { get; } = optional;

    public string NamedKeyLiteral { get; } = namedKeyLiteral;
}
