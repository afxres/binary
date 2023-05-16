namespace Mikodev.Binary.SourceGeneration.Contexts;

using Microsoft.CodeAnalysis;

public sealed partial class AttributeConverterCreatorContext
{
    public static SourceResult? Invoke(SourceGeneratorContext context, SourceGeneratorTracker tracker, ITypeSymbol symbol)
    {
        if (Symbols.GetConverterCreatorType(context, symbol) is not { } type)
            return null;
        return new AttributeConverterCreatorContext(context, tracker, symbol, type).Invoke();
    }
}
