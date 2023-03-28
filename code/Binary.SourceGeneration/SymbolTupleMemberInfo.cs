namespace Mikodev.Binary.SourceGeneration;

using Microsoft.CodeAnalysis;

public class SymbolTupleMemberInfo : SymbolMemberInfo
{
    public SymbolTupleMemberInfo(IFieldSymbol field) : base(field) { }

    public SymbolTupleMemberInfo(IPropertySymbol property) : base(property) { }
}
