namespace Mikodev.Binary.SourceGeneration;

using Microsoft.CodeAnalysis;

public static partial class Symbols
{
    public static void ValidateTypeAttributes(SourceGeneratorContext context, ITypeSymbol symbol)
    {
        var attributeNames = new[]
        {
            Constants.ConverterAttributeTypeName,
            Constants.ConverterCreatorAttributeTypeName,
            Constants.NamedObjectAttributeTypeName,
            Constants.TupleObjectAttributeTypeName
        };

        var attributes = GetAttributes(context, symbol, attributeNames);
        if (attributes.Length > 1)
            context.Throw(Constants.MultipleTypeAttributesFound, GetLocation(symbol), new object[] { symbol.Name });
        return;
    }

    public static void ValidateMemberAttributes(SourceGeneratorContext context, ISymbol symbol)
    {
        var attributes = GetAttributes(context, symbol, new[] { Constants.ConverterAttributeTypeName, Constants.ConverterCreatorAttributeTypeName });
        if (attributes.Length > 1)
            context.Throw(Constants.MultipleMemberAttributesFound, GetLocation(symbol), new object?[] { symbol.Name, symbol.ContainingType?.Name });
        return;
    }
}
