﻿namespace Mikodev.Binary.SourceGeneration.Contexts;

using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Text;

public partial class CollectionConverterContext : SymbolConverterContext
{
    private readonly SourceType sourceType;

    private readonly string methodBody;

    private readonly ImmutableArray<ITypeSymbol> elements;

    private CollectionConverterContext(SourceGeneratorContext context, ITypeSymbol symbol, SourceType sourceType, string methodBody, ImmutableArray<ITypeSymbol> elements) : base(context, symbol)
    {
        for (var i = 0; i < elements.Length; i++)
            TypeAliases.Add(elements[i], i.ToString());
        this.elements = elements;
        this.sourceType = sourceType;
        this.methodBody = methodBody;
    }

    private void AppendConverterCreatorBody(StringBuilder builder)
    {
        var sourceType = this.sourceType;
        var methodBody = this.methodBody;
        var elements = this.elements;
        var delegateName = DelegateTypeNames[sourceType];

        for (var i = 0; i < elements.Length; i++)
        {
            var element = elements[i];
            AppendAssignConverterExplicit(builder, element, $"cvt{i}", $"_C{i}", $"_T{i}");
            CancellationToken.ThrowIfCancellationRequested();
        }

        builder.AppendIndent(3, $"var constructor = new {delegateName}({methodBody});");
        builder.AppendIndent(3, $"var converter = Mikodev.Binary.Generator.GetEnumerableConverter(", ", constructor);", elements.Length, x => $"cvt{x}");
        builder.AppendIndent(3, $"return ({Constants.IConverterTypeName})converter;");
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
