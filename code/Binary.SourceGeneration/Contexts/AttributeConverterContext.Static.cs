namespace Mikodev.Binary.SourceGeneration.Contexts;

using Microsoft.CodeAnalysis;

public sealed partial class AttributeConverterContext
{
    public static SymbolConverterContent? Invoke(SourceGeneratorContext context, ITypeSymbol symbol)
    {
        if (Symbols.GetConverterType(context, symbol) is not { } type)
            return null;
        return new AttributeConverterContext(context, symbol, type).Invoke();
    }
}
