namespace Mikodev.Binary.SourceGeneration.Contexts;

using Microsoft.CodeAnalysis;

public partial class AttributeConverterCreatorContext
{
    public static string? Invoke(SourceGeneratorContext context, ITypeSymbol symbol)
    {
        if (Symbols.GetConverterCreatorType(context, symbol) is not { } type)
            return null;
        var closure = new AttributeConverterCreatorContext(context, symbol, type);
        closure.Invoke();
        return closure.ConverterCreatorTypeName;
    }
}
