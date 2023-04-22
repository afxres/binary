namespace Mikodev.Binary.SourceGeneration.Contexts;

using Microsoft.CodeAnalysis;

public sealed partial class AttributeConverterCreatorContext
{
    public static SymbolConverterContent? Invoke(SourceGeneratorContext context, ITypeSymbol symbol)
    {
        if (Symbols.GetConverterCreatorType(context, symbol) is not { } type)
            return null;
        return new AttributeConverterCreatorContext(context, symbol, type).Invoke();
    }
}
