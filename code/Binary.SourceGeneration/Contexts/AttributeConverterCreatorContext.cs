namespace Mikodev.Binary.SourceGeneration.Contexts;

using Microsoft.CodeAnalysis;
using Mikodev.Binary.SourceGeneration;
using System.Text;

public sealed partial class AttributeConverterCreatorContext : SymbolConverterContext
{
    private readonly ITypeSymbol creator;

    private AttributeConverterCreatorContext(SourceGeneratorContext context, SourceGeneratorTracker tracker, ITypeSymbol symbol, ITypeSymbol creator) : base(context, tracker, symbol)
    {
        this.creator = creator;
    }

    private void AppendConverterCreatorBody(StringBuilder builder)
    {
        AppendAssignConverterExplicitConverterCreator(builder, this.creator, "converter", SymbolConverterTypeFullName, SymbolTypeFullName);
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
