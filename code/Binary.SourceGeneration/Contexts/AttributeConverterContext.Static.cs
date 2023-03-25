namespace Mikodev.Binary.SourceGeneration.Contexts;

using Microsoft.CodeAnalysis;

public partial class AttributeConverterContext
{
    public static string? Invoke(SourceGeneratorContext context, ITypeSymbol symbol)
    {
        if (Symbols.GetConverterType(context, symbol) is not { } type)
            return null;
        var closure = new AttributeConverterContext(context, symbol, type);
        closure.Invoke();
        return closure.ConverterCreatorTypeName;
    }
}
