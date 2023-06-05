namespace Mikodev.Binary.SourceGeneration.Contexts;

using Microsoft.CodeAnalysis;
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

    private void AppendConverterHead(StringBuilder builder)
    {
        var info = this.info;
        var arguments = info.SourceType switch
        {
            SourceType.List => $"System.Collections.Generic.List<{GetTypeFullName(0)}>",
            SourceType.HashSet => $"System.Collections.Generic.HashSet<{GetTypeFullName(0)}>",
            SourceType.Dictionary => $"System.Collections.Generic.Dictionary<{GetTypeFullName(0)}, {GetTypeFullName(1)}>",
            SourceType.ListKeyValuePair => $"System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<{GetTypeFullName(0)}, {GetTypeFullName(1)}>>",
            _ => null,
        };
        var elements = info.ElementTypes;
        var tail = arguments is null ? ")" : $", Mikodev.Binary.Components.CollectionDecoder<{arguments}> decoder)";
        builder.AppendIndent(1, $"private sealed class {OutputConverterTypeName}(", tail, elements.Length, i => $"{GetConverterTypeFullName(i)} cvt{i}");
        builder.AppendIndent(2, $": Mikodev.Binary.Components.CollectionConverter<{SymbolTypeFullName}>");
        builder.AppendIndent(1, $"{{");
        CancellationToken.ThrowIfCancellationRequested();
    }

    private void AppendConverterTail(StringBuilder builder)
    {
        builder.AppendIndent(1, $"}}");
    }

    private void AppendEncodeMethod(StringBuilder builder)
    {
        var info = this.info;
        var elements = info.ElementTypes;
        builder.AppendIndent(2, $"public override void Encode(ref {Constants.AllocatorTypeName} allocator, {SymbolTypeFullName} item)");
        builder.AppendIndent(2, $"{{");
        if (Symbol.IsValueType is false)
        {
            builder.AppendIndent(3, $"if (item is null)");
            builder.AppendIndent(4, "return;");
            CancellationToken.ThrowIfCancellationRequested();
        }
        builder.AppendIndent(3, $"foreach (var i in item)");
        builder.AppendIndent(3, $"{{");
        if (elements.Length is 1)
            builder.AppendIndent(4, $"cvt0.EncodeAuto(ref allocator, i);");
        else
            builder.AppendIndent(4, $"cvt0.EncodeAuto(ref allocator, i.Key);");
        if (elements.Length is not 1)
            builder.AppendIndent(4, $"cvt1.EncodeAuto(ref allocator, i.Value);");
        builder.AppendIndent(3, $"}}");
        builder.AppendIndent(2, $"}}");
    }

    private void AppendDecodeMethod(StringBuilder builder)
    {
        var info = this.info;
        if (info.SourceType is SourceType.Null)
            return;
        builder.AppendIndent();
        builder.AppendIndent(2, $"public override {SymbolTypeFullName} Decode(in System.ReadOnlySpan<byte> span)");
        builder.AppendIndent(2, $"{{");
        var method = info.MethodBody;
        if (string.IsNullOrEmpty(method))
            method = $"new {SymbolTypeFullName}(item)";
        builder.AppendIndent(3, $"var item = decoder.Invoke(span);");
        builder.AppendIndent(3, $"return {method};");
        builder.AppendIndent(2, $"}}");
    }

    private void AppendConverterCreatorBody(StringBuilder builder)
    {
        var info = this.info;
        var method = info.SourceType switch
        {
            SourceType.List => "GetListDecoder",
            SourceType.HashSet => "GetHashSetDecoder",
            SourceType.Dictionary => "GetDictionaryDecoder",
            SourceType.ListKeyValuePair => "GetListDecoder",
            _ => null,
        };
        var elements = info.ElementTypes;
        for (var i = 0; i < elements.Length; i++)
        {
            var element = elements[i];
            AppendAssignConverterExplicit(builder, element, $"cvt{i}", GetConverterTypeFullName(i), GetTypeFullName(i));
            CancellationToken.ThrowIfCancellationRequested();
        }
        var tail = method is null ? ");" : ", decoder);";
        if (method is not null)
            builder.AppendIndent(3, $"var decoder = Mikodev.Binary.Components.Collection.{method}(", ");", elements.Length, x => $"cvt{x}");
        builder.AppendIndent(3, $"var converter = new {OutputConverterTypeName}(", tail, elements.Length, x => $"cvt{x}");
    }

    protected override void Invoke(StringBuilder builder)
    {
        AppendConverterHead(builder);
        AppendEncodeMethod(builder);
        AppendDecodeMethod(builder);
        AppendConverterTail(builder);
        builder.AppendIndent();

        AppendConverterCreatorHead(builder);
        AppendConverterCreatorBody(builder);
        AppendConverterCreatorTail(builder);
    }
}
