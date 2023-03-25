namespace Mikodev.Binary.SourceGeneration;

using Microsoft.CodeAnalysis;

public class SymbolTupleMemberInfo : SymbolMemberInfo
{
    public SymbolTupleMemberInfo(ISymbol symbol, ITypeSymbol type, string name, bool @readonly) : base(symbol, type, name, @readonly) { }
}
