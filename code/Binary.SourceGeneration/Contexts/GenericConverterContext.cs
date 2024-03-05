namespace Mikodev.Binary.SourceGeneration.Contexts;

using Microsoft.CodeAnalysis;
using Mikodev.Binary.SourceGeneration;
using Mikodev.Binary.SourceGeneration.Internal;
using System.Collections.Generic;

public sealed partial class GenericConverterContext : SymbolConverterContext
{
    private readonly TypeInfo info;

    private GenericConverterContext(SourceGeneratorContext context, SourceGeneratorTracker tracker, ITypeSymbol symbol, TypeInfo info) : base(context, tracker, symbol)
    {
        var elements = info.ElementTypes;
        elements.AsSpan().ForEach(AddType);
        this.info = info;
    }

    private void AppendConverterCreatorBody()
    {
        var info = this.info;
        var elements = info.ElementTypes;
        var types = new List<string>();
        if (info.TypeArgumentsOption is TypeArgumentsOption.IncludeReturnType)
            types.Add(SymbolTypeFullName);
        for (var i = 0; i < elements.Length; i++)
        {
            types.Add(GetTypeFullName(i));
            AppendAssignConverterExplicit(elements[i], $"cvt{i}", GetTypeFullName(i));
            CancellationToken.ThrowIfCancellationRequested();
        }
        Output.AppendIndent(3, $"var converter = Mikodev.Binary.Generator.Get{info.TypeName}Converter<{string.Join(", ", types)}>(", ");", elements.Length, x => $"cvt{x}");
    }

    protected override void Handle()
    {
        AppendConverterCreatorHead();
        AppendConverterCreatorBody();
        AppendConverterCreatorTail();
    }
}
