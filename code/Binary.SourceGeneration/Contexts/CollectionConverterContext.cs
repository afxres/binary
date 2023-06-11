namespace Mikodev.Binary.SourceGeneration.Contexts;

using Microsoft.CodeAnalysis;
using Mikodev.Binary.SourceGeneration.Internal;

public sealed partial class CollectionConverterContext : SymbolConverterContext
{
    private readonly TypeInfo info;

    private CollectionConverterContext(SourceGeneratorContext context, SourceGeneratorTracker tracker, ITypeSymbol symbol, TypeInfo info) : base(context, tracker, symbol)
    {
        var elements = info.ElementTypes;
        elements.AsSpan().ForEach(AddType);
        this.info = info;
    }

    private void AppendConverterHead()
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
        Output.AppendIndent(1, $"private sealed class {OutputConverterTypeName}(", tail, elements.Length, i => $"{GetConverterTypeFullName(i)} cvt{i}");
        Output.AppendIndent(2, $": Mikodev.Binary.Components.CollectionConverter<{SymbolTypeFullName}>");
        Output.AppendIndent(1, $"{{");
        CancellationToken.ThrowIfCancellationRequested();
    }

    private void AppendConverterTail()
    {
        Output.AppendIndent(1, $"}}");
        Output.AppendIndent();
    }

    private void AppendEnsureContext()
    {
        if (Symbol.IsValueType)
            return;
        Output.AppendIndent(3, $"if (item is null)");
        Output.AppendIndent(4, "return;");
        CancellationToken.ThrowIfCancellationRequested();
    }

    private void AppendEncodeMethod()
    {
        var info = this.info;
        var elements = info.ElementTypes;
        Output.AppendIndent(2, $"public override void Encode(ref {Constants.AllocatorTypeName} allocator, {SymbolTypeFullName} item)");
        Output.AppendIndent(2, $"{{");
        AppendEnsureContext();
        Output.AppendIndent(3, $"foreach (var i in item)");
        if (elements.Length is 1)
        {
            Output.AppendIndent(4, $"cvt0.EncodeAuto(ref allocator, i);");
        }
        else
        {
            Output.AppendIndent(3, $"{{");
            Output.AppendIndent(4, $"cvt0.EncodeAuto(ref allocator, i.Key);");
            Output.AppendIndent(4, $"cvt1.EncodeAuto(ref allocator, i.Value);");
            Output.AppendIndent(3, $"}}");
        }
        Output.AppendIndent(2, $"}}");
        CancellationToken.ThrowIfCancellationRequested();
    }

    private void AppendDecodeMethod()
    {
        var info = this.info;
        if (info.SourceType is SourceType.Null)
            return;
        Output.AppendIndent();
        Output.AppendIndent(2, $"public override {SymbolTypeFullName} Decode(in System.ReadOnlySpan<byte> span)");
        Output.AppendIndent(2, $"{{");
        var invoke = "decoder.Invoke(span)";
        var method = info.MethodBody;
        var action = string.IsNullOrEmpty(method)
            ? $"new {SymbolTypeFullName}({invoke})"
            : method.Replace(ConstructorParameter, invoke);
        Output.AppendIndent(3, $"return {action};");
        Output.AppendIndent(2, $"}}");
        CancellationToken.ThrowIfCancellationRequested();
    }

    private void AppendConverterCreatorBody()
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
            AppendAssignConverterExplicit(element, $"cvt{i}", GetConverterTypeFullName(i), GetTypeFullName(i));
            CancellationToken.ThrowIfCancellationRequested();
        }
        var tail = method is null ? ");" : ", decoder);";
        if (method is not null)
            Output.AppendIndent(3, $"var decoder = Mikodev.Binary.Components.Collection.{method}(", ");", elements.Length, x => $"cvt{x}");
        Output.AppendIndent(3, $"var converter = new {OutputConverterTypeName}(", tail, elements.Length, x => $"cvt{x}");
    }

    protected override void Handle()
    {
        AppendConverterHead();
        AppendEncodeMethod();
        AppendDecodeMethod();
        AppendConverterTail();

        AppendConverterCreatorHead();
        AppendConverterCreatorBody();
        AppendConverterCreatorTail();
    }
}
