namespace Mikodev.Binary.SourceGeneration;

using Microsoft.CodeAnalysis;

public class SymbolTupleMemberInfo(ISymbol symbol, string path) : SymbolMemberInfo(symbol)
{
    public string Path => path;
}
