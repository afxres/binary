namespace Mikodev.Binary.SourceGeneration.Contexts;

using Microsoft.CodeAnalysis;
using Mikodev.Binary.SourceGeneration;

public sealed partial class AttributeConverterContext : SymbolConverterContext
{
    private readonly ITypeSymbol converter;

    private AttributeConverterContext(SourceGeneratorContext context, SourceGeneratorTracker tracker, ITypeSymbol symbol, ITypeSymbol converter) : base(context, tracker, symbol)
    {
        this.converter = converter;
    }

    protected override void Handle()
    {
        AppendConverterCreatorHead();
        AppendAssignConverterExplicitConverter(this.converter, "converter", SymbolConverterTypeFullName);
        AppendConverterCreatorTail();
    }
}
