namespace Mikodev.Binary.SourceGeneration.Contexts;

using Microsoft.CodeAnalysis;

public sealed partial class AttributeConverterContext
{
    public static SourceResult? Invoke(SourceGeneratorContext context, SourceGeneratorTracker tracker, ITypeSymbol symbol)
    {
        if (Symbols.GetConverterType(context, symbol) is not { } type)
            return null;
        return new AttributeConverterContext(context, tracker, symbol, type).Invoke();
    }
}
