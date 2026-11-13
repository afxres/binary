namespace Mikodev.Binary.SourceGeneration.Contexts;

using Microsoft.CodeAnalysis;

public sealed partial class CollectionConverterContext
{
    public static SourceResult? Invoke(SourceGeneratorContext context, SourceGeneratorTracker tracker, ITypeSymbol symbol)
    {
        if (symbol is not INamedTypeSymbol namedType)
            return null;
        var types = context.GetOrCreateResource(CollectionSupportedTypes.Create);
        if (types.IsUnsupported(namedType))
            return new SourceResult.Ignored();
        if (types.GetInfo(namedType) is not { } info)
            return null;
        return new CollectionConverterContext(context, tracker, symbol, info).Build();
    }
}
