namespace Mikodev.Binary.SourceGeneration;

using Microsoft.CodeAnalysis;

public class SymbolConverterContent
{
    public ITypeSymbol Symbol { get; }

    public string ConverterCreatorTypeName { get; }

    public string Code { get; }

    public SymbolConverterContent(ITypeSymbol symbol, string converterCreatorTypeName, string code)
    {
        Symbol = symbol;
        ConverterCreatorTypeName = converterCreatorTypeName;
        Code = code;
    }
}
