﻿namespace Mikodev.Binary.SourceGeneration.Contexts;

using Microsoft.CodeAnalysis;
using Mikodev.Binary.SourceGeneration;
using System.Text;

public partial class AttributeConverterCreatorContext : SymbolConverterContext
{
    private readonly ITypeSymbol creator;

    private AttributeConverterCreatorContext(SourceGeneratorContext context, ITypeSymbol symbol, ITypeSymbol creator) : base(context, symbol)
    {
        this.creator = creator;
    }

    private void AppendConverterCreatorBody(StringBuilder builder)
    {
        AppendAssignConverterExplicitConverterCreator(builder, this.creator, "converter", "_CSelf", "_TSelf");
        builder.AppendIndent(3, $"return ({Constants.IConverterTypeName})converter;");
        CancellationToken.ThrowIfCancellationRequested();
    }

    private void Invoke()
    {
        var builder = new StringBuilder();
        AppendFileHead(builder);

        AppendConverterCreatorHead(builder);
        AppendConverterCreatorBody(builder);
        AppendConverterCreatorTail(builder);

        AppendFileTail(builder);
        Finish(builder);
    }
}
