namespace Mikodev.Binary.SourceGeneration.Contexts;

using Microsoft.CodeAnalysis;
using System.Linq;
using System.Text;

public sealed partial class CollectionConverterContext : SymbolConverterContext
{
    private readonly TypeInfo info;

    private CollectionConverterContext(SourceGeneratorContext context, SourceGeneratorTracker tracker, ITypeSymbol symbol, TypeInfo info) : base(context, tracker, symbol)
    {
        var elements = info.ElementTypes;
        for (var i = 0; i < elements.Length; i++)
            AddType(i, elements[i]);
        this.info = info;
    }

    private void AppendConverterCreatorBody(StringBuilder builder)
    {
        var info = this.info;
        var delegateName = info.SourceType switch
        {
            SourceType.List => $"System.Func<System.Collections.Generic.List<{GetTypeFullName(0)}>, {SymbolTypeFullName}>",
            SourceType.HashSet => $"System.Func<System.Collections.Generic.HashSet<{GetTypeFullName(0)}>, {SymbolTypeFullName}>",
            SourceType.Dictionary => $"System.Func<System.Collections.Generic.Dictionary<{GetTypeFullName(0)}, {GetTypeFullName(1)}>, {SymbolTypeFullName}>",
            SourceType.ListKeyValuePair => $"System.Func<System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<{GetTypeFullName(0)}, {GetTypeFullName(1)}>>, {SymbolTypeFullName}>",
            _ => null,
        };

        var elements = info.ElementTypes;
        for (var i = 0; i < elements.Length; i++)
        {
            var element = elements[i];
            AppendAssignConverterExplicit(builder, element, $"cvt{i}", GetConverterTypeFullName(i), GetTypeFullName(i));
            CancellationToken.ThrowIfCancellationRequested();
        }

        var tail = delegateName is null ? ");" : $", constructor);";
        var types = string.Join(", ", new[] { SymbolTypeFullName }.Concat(elements.Select((_, i) => GetTypeFullName(i))));
        if (delegateName is not null)
        {
            var methodBody = info.MethodBody;
            if (string.IsNullOrEmpty(methodBody))
                methodBody = $"static x => new {SymbolTypeFullName}(x)";
            builder.AppendIndent(3, $"var constructor = new {delegateName}({methodBody});");
        }
        builder.AppendIndent(3, $"var converter = Mikodev.Binary.Generator.GetEnumerableConverter<{types}>(", tail, elements.Length, x => $"cvt{x}");
        builder.AppendIndent(3, $"return ({Constants.IConverterTypeName})converter;");
    }

    protected override void Invoke(StringBuilder builder)
    {
        AppendConverterCreatorHead(builder);
        AppendConverterCreatorBody(builder);
        AppendConverterCreatorTail(builder);
    }
}
