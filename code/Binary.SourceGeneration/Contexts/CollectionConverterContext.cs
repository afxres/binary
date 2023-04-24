namespace Mikodev.Binary.SourceGeneration.Contexts;

using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

public sealed partial class CollectionConverterContext : SymbolConverterContext
{
    private readonly SourceType sourceType;

    private readonly string methodBody;

    private readonly ImmutableArray<ITypeSymbol> elements;

    private CollectionConverterContext(SourceGeneratorContext context, ITypeSymbol symbol, SourceType sourceType, string methodBody, ImmutableArray<ITypeSymbol> elements) : base(context, symbol)
    {
        for (var i = 0; i < elements.Length; i++)
            AddType(i, elements[i]);
        this.elements = elements;
        this.sourceType = sourceType;
        this.methodBody = methodBody;
    }

    private void AppendConverterCreatorBody(StringBuilder builder)
    {
        var sourceType = this.sourceType;
        var methodBody = this.methodBody;
        var elements = this.elements;

        var delegateName = sourceType switch
        {
            SourceType.List => $"System.Func<System.Collections.Generic.List<{GetTypeFullName(0)}>, {SymbolTypeFullName}>",
            SourceType.HashSet => $"System.Func<System.Collections.Generic.HashSet<{GetTypeFullName(0)}>, {SymbolTypeFullName}>",
            SourceType.Dictionary => $"System.Func<System.Collections.Generic.Dictionary<{GetTypeFullName(0)}, {GetTypeFullName(1)}>, {SymbolTypeFullName}>",
            SourceType.ListKeyValuePair => $"System.Func<System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<{GetTypeFullName(0)}, {GetTypeFullName(1)}>>, {SymbolTypeFullName}>",
            _ => null,
        };

        for (var i = 0; i < elements.Length; i++)
        {
            var element = elements[i];
            AppendAssignConverterExplicit(builder, element, $"cvt{i}", $"{GetConverterTypeFullName(i)}", GetTypeFullName(i));
            CancellationToken.ThrowIfCancellationRequested();
        }

        var tail = delegateName is null ? ");" : $", constructor);";
        var types = string.Join(", ", new[] { SymbolTypeFullName }.Concat(elements.Select((_, i) => GetTypeFullName(i))));
        if (delegateName is not null)
        {
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
