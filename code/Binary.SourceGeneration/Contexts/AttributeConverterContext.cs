namespace Mikodev.Binary.SourceGeneration.Contexts;

using Microsoft.CodeAnalysis;
using Mikodev.Binary.SourceGeneration;
using System.Text;

public partial class AttributeConverterContext : SymbolConverterContext
{
    private readonly ITypeSymbol converter;

    private AttributeConverterContext(SourceGeneratorContext context, ITypeSymbol symbol, ITypeSymbol converter) : base(context, symbol)
    {
        this.converter = converter;
    }

    private void AppendConverterCreatorBody(StringBuilder builder)
    {
        AppendAssignConverterExplicitConverter(builder, this.converter, "converter", SymbolConverterTypeFullName);
        builder.AppendIndent(3, $"return ({Constants.IConverterTypeName})converter;");
        CancellationToken.ThrowIfCancellationRequested();
    }

    protected override void Invoke(StringBuilder builder)
    {
        AppendConverterCreatorHead(builder);
        AppendConverterCreatorBody(builder);
        AppendConverterCreatorTail(builder);
    }
}
