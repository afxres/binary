namespace Mikodev.Binary.SourceGeneration;

using Microsoft.CodeAnalysis;
using System.Diagnostics;

public class SymbolMemberInfo
{
    public ISymbol Symbol { get; }

    public ITypeSymbol TypeSymbol { get; }

    public string Name { get; }

    public bool IsReadOnly { get; }

    public SymbolMemberInfo(ISymbol symbol, ITypeSymbol type, string name, bool @readonly)
    {
        Debug.Assert(string.IsNullOrEmpty(name) is false);
        Symbol = symbol;
        TypeSymbol = type;
        Name = name;
        IsReadOnly = @readonly;
    }
}
