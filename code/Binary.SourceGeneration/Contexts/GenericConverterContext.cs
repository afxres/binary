namespace Mikodev.Binary.SourceGeneration.Contexts;

using Microsoft.CodeAnalysis;
using Mikodev.Binary.SourceGeneration;
using System;
using System.Linq;
using System.Text;

public sealed partial class GenericConverterContext : SymbolConverterContext
{
    private readonly TypeInfo info;

    private GenericConverterContext(SourceGeneratorContext context, ITypeSymbol symbol, TypeInfo info) : base(context, symbol)
    {
        var elements = info.Elements;
        for (var i = 0; i < elements.Length; i++)
            AddType(i, elements[i]);
        this.info = info;
    }

    private void AppendConverterCreatorBody(StringBuilder builder)
    {
        var info = this.info;
        var elements = info.Elements;
        for (var i = 0; i < elements.Length; i++)
        {
            var element = elements[i];
            AppendAssignConverterExplicit(builder, element, $"cvt{i}", GetConverterTypeFullName(i), GetTypeFullName(i));
            CancellationToken.ThrowIfCancellationRequested();
        }

        var types = string.Join(", ", (info.SelfType is SelfType.Include ? new[] { SymbolTypeFullName } : Array.Empty<string>()).Concat(elements.Select((_, i) => GetTypeFullName(i))));
        builder.AppendIndent(3, $"var converter = Mikodev.Binary.Generator.Get{info.Name}Converter<{types}>(", ");", elements.Length, x => $"cvt{x}");
        builder.AppendIndent(3, $"return ({Constants.IConverterTypeName})converter;");
    }

    protected override void Invoke(StringBuilder builder)
    {
        AppendConverterCreatorHead(builder);
        AppendConverterCreatorBody(builder);
        AppendConverterCreatorTail(builder);
    }
}
