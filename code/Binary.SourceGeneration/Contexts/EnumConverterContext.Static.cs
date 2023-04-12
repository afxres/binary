namespace Mikodev.Binary.SourceGeneration.Contexts;

using Microsoft.CodeAnalysis;

public partial class EnumConverterContext
{
    public static SymbolConverterContent? Invoke(SourceGeneratorContext context, ITypeSymbol symbol)
    {
        if (symbol.TypeKind is not TypeKind.Enum)
            return null;
        return new EnumConverterContext(context, symbol).Invoke();
    }
}
