namespace Mikodev.Binary.SourceGeneration.Contexts;

using Microsoft.CodeAnalysis;

public partial class EnumConverterContext
{
    public static string? Invoke(SourceGeneratorContext context, ITypeSymbol symbol)
    {
        if (symbol.TypeKind is not TypeKind.Enum)
            return null;
        var closure = new EnumConverterContext(context, symbol);
        closure.Invoke();
        return closure.ConverterCreatorTypeName;
    }
}
