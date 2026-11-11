namespace Mikodev.Binary.SourceGeneration;

using Microsoft.CodeAnalysis;

public class SymbolNamedObjectMemberInfo(ISymbol symbol, string namedKey, string namedKeyLiteral, bool optional) : SymbolObjectMemberInfo(symbol)
{
    public string NamedKey { get; } = namedKey;

    public bool IsOptional { get; } = optional;

    public string NamedKeyLiteral { get; } = namedKeyLiteral;
}
