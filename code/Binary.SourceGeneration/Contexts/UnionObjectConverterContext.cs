namespace Mikodev.Binary.SourceGeneration.Contexts;

using Microsoft.CodeAnalysis;
using Mikodev.Binary.SourceGeneration.Internal;
using System.Collections.Immutable;
using System.Linq;
using UnionCaseInfo = (int Index, Microsoft.CodeAnalysis.ITypeSymbol Type);

public sealed partial class UnionObjectConverterContext : SymbolConverterContext
{
    private readonly ImmutableArray<UnionCaseInfo> cases;

    private UnionObjectConverterContext(SourceGeneratorContext context, SourceGeneratorTracker tracker, ITypeSymbol symbol, ImmutableArray<UnionCaseInfo> cases) : base(context, tracker, symbol)
    {
        var types = cases.Select(x => x.Type).ToImmutableArray();
        types.ForEach(AddType);
        this.cases = cases;
    }

    private void AppendConverterHead()
    {
        var cases = this.cases;
        Output.AppendIndent(1, $"private sealed class {OutputConverterTypeName}(", ")", cases.Length, i => $"{GetConverterTypeFullName(i)} cvt{i}");
        Output.AppendIndent(2, $": Mikodev.Binary.Converter<{SymbolTypeFullName}>(0)");
        Output.AppendIndent(1, $"{{");
        CancellationToken.ThrowIfCancellationRequested();
    }

    private void AppendConverterTail()
    {
        Output.AppendIndent(1, $"}}");
        Output.AppendIndent();
    }

    private void AppendEncodeMethod(bool auto)
    {
        var cases = this.cases;
        if (auto)
            Output.AppendIndent();
        Output.AppendIndent(2, $"public override void {(auto ? "EncodeAuto" : "Encode")}(ref Mikodev.Binary.Allocator allocator, {SymbolTypeFullName} item)");
        Output.AppendIndent(2, $"{{");
        Output.AppendIndent(3, $"switch (item)");
        Output.AppendIndent(3, $"{{");
        for (var i = 0; i < cases.Length; i++)
        {
            Output.AppendIndent(4, $"case {GetTypeFullName(i)} var{i}:");
            Output.AppendIndent(5, $"Mikodev.Binary.Converter.Encode(ref allocator, {cases[i].Index});");
            Output.AppendIndent(5, $"cvt{i}.{(auto ? "EncodeAuto" : "Encode")}(ref allocator, var{i});");
            Output.AppendIndent(5, $"break;");
            CancellationToken.ThrowIfCancellationRequested();
        }
        Output.AppendIndent(4, $"default:");
        Output.AppendIndent(5, $"throw new System.ArgumentException($\"Invalid or null union value, type: {{typeof({SymbolTypeFullName})}}\");");
        Output.AppendIndent(3, $"}}");
        Output.AppendIndent(2, $"}}");
    }

    private void AppendDecodeMethod(bool auto)
    {
        var cases = this.cases;
        Output.AppendIndent();
        Output.AppendIndent(2, $"public override {SymbolTypeFullName} {(auto ? "DecodeAuto" : "Decode")}({(auto ? "ref" : "in")} System.ReadOnlySpan<byte> span)");
        Output.AppendIndent(2, $"{{");
        if (auto is false)
            Output.AppendIndent(3, $"var copy = span;");
        var bufferName = auto ? "span" : "copy";
        Output.AppendIndent(3, $"var index = Mikodev.Binary.Converter.Decode(ref {bufferName});");
        Output.AppendIndent(3, $"return index switch");
        Output.AppendIndent(3, $"{{");
        for (var i = 0; i < cases.Length; i++)
        {
            Output.AppendIndent(4, $"{cases[i].Index} => cvt{i}.{(auto ? "DecodeAuto" : "Decode")}({(auto ? "ref" : "in")} {bufferName}),");
            CancellationToken.ThrowIfCancellationRequested();
        }
        Output.AppendIndent(4, $"_ => throw new System.ArgumentException($\"Invalid union tag '{{index}}', type: {{typeof({SymbolTypeFullName})}}\"),");
        Output.AppendIndent(3, $"}};");
        Output.AppendIndent(2, $"}}");
    }

    private void AppendConverterCreatorBody()
    {
        var cases = this.cases;
        for (var i = 0; i < cases.Length; i++)
        {
            var @case = cases[i];
            AppendAssignConverterExplicit(@case.Type, $"cvt{i}", GetTypeFullName(i));
            CancellationToken.ThrowIfCancellationRequested();
        }
        Output.AppendIndent(3, $"var converter = new {OutputConverterTypeName}(", ");", cases.Length, x => $"cvt{x}");
    }

    protected override void Handle()
    {
        AppendConverterHead();
        AppendEncodeMethod(auto: false);
        AppendEncodeMethod(auto: true);
        AppendDecodeMethod(auto: false);
        AppendDecodeMethod(auto: true);
        AppendConverterTail();

        AppendConverterCreatorHead();
        AppendConverterCreatorBody();
        AppendConverterCreatorTail();
    }
}
