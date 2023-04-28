namespace Mikodev.Binary.SourceGeneration.Contexts;

using Microsoft.CodeAnalysis;
using Mikodev.Binary.SourceGeneration;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

public sealed partial class GenericConverterContext : SymbolConverterContext
{
    private readonly string name;

    private readonly SelfType selfType;

    private readonly ImmutableArray<ITypeSymbol> elements;

    private GenericConverterContext(SourceGeneratorContext context, ITypeSymbol symbol, string name, ImmutableArray<ITypeSymbol> elements, SelfType selfType) : base(context, symbol)
    {
        for (var i = 0; i < elements.Length; i++)
            AddType(i, elements[i]);
        this.name = name;
        this.elements = elements;
        this.selfType = selfType;
    }

    private void AppendConverterCreatorBody(StringBuilder builder)
    {
        var name = this.name;
        var elements = this.elements;
        for (var i = 0; i < elements.Length; i++)
        {
            var element = elements[i];
            AppendAssignConverterExplicit(builder, element, $"cvt{i}", GetConverterTypeFullName(i), GetTypeFullName(i));
            CancellationToken.ThrowIfCancellationRequested();
        }

        var types = string.Join(", ", (this.selfType is SelfType.Include ? new[] { SymbolTypeFullName } : Array.Empty<string>()).Concat(elements.Select((_, i) => GetTypeFullName(i))));
        builder.AppendIndent(3, $"var converter = Mikodev.Binary.Generator.Get{name}Converter<{types}>(", ");", elements.Length, x => $"cvt{x}");
        builder.AppendIndent(3, $"return ({Constants.IConverterTypeName})converter;");
    }

    protected override void Invoke(StringBuilder builder)
    {
        AppendConverterCreatorHead(builder);
        AppendConverterCreatorBody(builder);
        AppendConverterCreatorTail(builder);
    }
}
