﻿namespace Mikodev.Binary.SourceGeneration.Contexts;

using Microsoft.CodeAnalysis;
using Mikodev.Binary.SourceGeneration;
using Mikodev.Binary.SourceGeneration.Internal;
using System.Collections.Generic;
using System.Linq;

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
        for (var i = 0; i < elements.Length; i++)
        {
            var element = elements[i];
            AppendAssignConverterExplicit(element, $"cvt{i}", GetConverterTypeFullName(i), GetTypeFullName(i));
            CancellationToken.ThrowIfCancellationRequested();
        }
        var types = new List<string>();
        if (info.SelfType is SelfType.Include)
            types.Add(SymbolTypeFullName);
        types.AddRange(elements.Select((_, i) => GetTypeFullName(i)));
        var arguments = string.Join(", ", types);
        Output.AppendIndent(3, $"var converter = Mikodev.Binary.Generator.Get{info.Name}Converter<{arguments}>(", ");", elements.Length, x => $"cvt{x}");
    }

    protected override void Handle()
    {
        AppendConverterCreatorHead();
        AppendConverterCreatorBody();
        AppendConverterCreatorTail();
    }
}
