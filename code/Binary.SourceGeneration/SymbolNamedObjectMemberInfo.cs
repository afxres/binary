namespace Mikodev.Binary.SourceGeneration;

using Microsoft.CodeAnalysis;

public class SymbolNamedObjectMemberInfo(ISymbol symbol, bool optional, string namedKey, string namedKeyLiteral) : SymbolObjectMemberInfo(symbol)
{
    public bool IsOptional { get; } = optional;

    public string NamedKey { get; } = namedKey;

    public string NamedKeyLiteral { get; } = namedKeyLiteral;
}
