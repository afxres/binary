namespace Mikodev.Binary.SourceGeneration.Contexts;

using Microsoft.CodeAnalysis;
using Mikodev.Binary.SourceGeneration.Internal;
using System;

public sealed partial class CollectionConverterContext : SymbolConverterContext
{
    private readonly TypeInfo info;

    private CollectionConverterContext(SourceGeneratorContext context, SourceGeneratorTracker tracker, ITypeSymbol symbol, TypeInfo info) : base(context, tracker, symbol)
    {
        var elements = info.ElementTypes;
        elements.ForEach(AddType);
        this.info = info;
    }

    private void AppendConverterHead()
    {
        var info = this.info;
        var elements = info.ElementTypes;
        Output.AppendIndent(1, $"private sealed class {OutputConverterTypeName}(", ")", elements.Length, i => $"{GetConverterTypeFullName(i)} cvt{i}");
        Output.AppendIndent(2, $": Mikodev.Binary.Converter<{SymbolTypeFullName}>");
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
        Output.AppendIndent(4, $"return;");
        CancellationToken.ThrowIfCancellationRequested();
    }

    private void AppendEncodeMethod()
    {
        var info = this.info;
        var elements = info.ElementTypes;
        Output.AppendIndent(2, $"public override void Encode(ref Mikodev.Binary.Allocator allocator, {SymbolTypeFullName} item)");
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

    private void AppendDecodeList()
    {
        Output.AppendIndent(3, $"var item = new System.Collections.Generic.List<{GetTypeFullName(0)}>();");
        Output.AppendIndent(3, $"while (body.Length is not 0)");
        Output.AppendIndent(4, $"item.Add(cvt0.DecodeAuto(ref body));");
    }

    private void AppendDecodeHashSet()
    {
        Output.AppendIndent(3, $"var item = new System.Collections.Generic.HashSet<{GetTypeFullName(0)}>();");
        Output.AppendIndent(3, $"while (body.Length is not 0)");
        Output.AppendIndent(4, $"_ = item.Add(cvt0.DecodeAuto(ref body));");
    }

    private void AppendDecodeDictionary()
    {
        Output.AppendIndent(3, $"var item = new System.Collections.Generic.Dictionary<{GetTypeFullName(0)}, {GetTypeFullName(1)}>();");
        Output.AppendIndent(3, $"while (body.Length is not 0)");
        Output.AppendIndent(3, $"{{");
        Output.AppendIndent(4, $"var var0 = cvt0.DecodeAuto(ref body);");
        Output.AppendIndent(4, $"var var1 = cvt1.DecodeAuto(ref body);");
        Output.AppendIndent(4, $"item.Add(var0, var1);");
        Output.AppendIndent(3, $"}}");
    }

    private void AppendDecodeKeyValueEnumerable()
    {
        Output.AppendIndent(3, $"var item = new System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<{GetTypeFullName(0)}, {GetTypeFullName(1)}>>();");
        Output.AppendIndent(3, $"while (body.Length is not 0)");
        Output.AppendIndent(3, $"{{");
        Output.AppendIndent(4, $"var var0 = cvt0.DecodeAuto(ref body);");
        Output.AppendIndent(4, $"var var1 = cvt1.DecodeAuto(ref body);");
        Output.AppendIndent(4, $"item.Add(System.Collections.Generic.KeyValuePair.Create(var0, var1));");
        Output.AppendIndent(3, $"}}");
    }

    private void AppendDecodeMethod()
    {
        var info = this.info;
        var action = info.ConstructorArgumentKind switch
        {
            ConstructorArgumentKind.List => AppendDecodeList,
            ConstructorArgumentKind.HashSet => AppendDecodeHashSet,
            ConstructorArgumentKind.Dictionary => AppendDecodeDictionary,
            ConstructorArgumentKind.KeyValueEnumerable => AppendDecodeKeyValueEnumerable,
            _ => default(Action),
        };
        Output.AppendIndent();
        Output.AppendIndent(2, $"public override {SymbolTypeFullName} Decode(in System.ReadOnlySpan<byte> span)");
        Output.AppendIndent(2, $"{{");
        if (action is null)
        {
            Output.AppendIndent(3, $"throw new System.NotSupportedException($\"No suitable constructor found, type: {{typeof({SymbolTypeFullName})}}\");");
        }
        else
        {
            Output.AppendIndent(3, $"var body = span;");
            action.Invoke();
            var method = info.ConstructorExpression;
            if (string.IsNullOrEmpty(method))
                method = $"new {SymbolTypeFullName}({ConstructorArgument})";
            Output.AppendIndent(3, $"return {method};");
        }
        Output.AppendIndent(2, $"}}");
        CancellationToken.ThrowIfCancellationRequested();
    }

    private void AppendConverterCreatorBody()
    {
        var info = this.info;
        var elements = info.ElementTypes;
        for (var i = 0; i < elements.Length; i++)
        {
            var element = elements[i];
            AppendAssignConverterExplicit(element, $"cvt{i}", GetTypeFullName(i));
            CancellationToken.ThrowIfCancellationRequested();
        }
        Output.AppendIndent(3, $"var converter = new {OutputConverterTypeName}(", ");", elements.Length, x => $"cvt{x}");
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
