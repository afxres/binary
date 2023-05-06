namespace Mikodev.Binary.SourceGeneration.Contexts;

using Microsoft.CodeAnalysis;
using System.Text;

public sealed partial class EnumConverterContext : SymbolConverterContext
{
    private EnumConverterContext(SourceGeneratorContext context, ITypeSymbol symbol) : base(context, symbol) { }

    private void AppendConverterCreatorBody(StringBuilder builder)
    {
        builder.AppendIndent(3, $"var converter = Mikodev.Binary.Generator.GetEnumConverter<{SymbolTypeFullName}>();");
        builder.AppendIndent(3, $"return ({Constants.IConverterTypeName})converter;");
    }

    protected override void Invoke(StringBuilder builder)
    {
        AppendConverterCreatorHead(builder);
        AppendConverterCreatorBody(builder);
        AppendConverterCreatorTail(builder);
    }
}
