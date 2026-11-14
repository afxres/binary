namespace Mikodev.Binary.SourceGeneration.Contexts;

using Microsoft.CodeAnalysis;
using Mikodev.Binary.SourceGeneration.Internal;
using System.Collections.Immutable;
using System.Linq;

public sealed partial class UnionObjectConverterContext : SymbolConverterContext
{
    private sealed record UnionCaseInfo(int Index, ITypeSymbol Type, bool Empty);

    private sealed record UnionCaseSlot(int Index, ITypeSymbol Type, int Position, string PatternTypeName, string? ConverterName);

    private readonly ImmutableArray<UnionCaseSlot> slots;

    private UnionObjectConverterContext(SourceGeneratorContext context, SourceGeneratorTracker tracker, ITypeSymbol symbol, ImmutableArray<UnionCaseInfo> cases) : base(context, tracker, symbol)
    {
        var types = cases.Select(x => x.Type).ToImmutableArray();
        types.ForEach(AddType);
        var slots = ImmutableArray.CreateBuilder<UnionCaseSlot>(cases.Length);
        var converterCount = 0;
        for (var i = 0; i < cases.Length; i++)
        {
            var caseType = cases[i].Type;
            var patternTypeName = context.GetTypeFullName(Symbols.GetNullableUnderlyingType(caseType) ?? caseType);
            slots.Add(new UnionCaseSlot(cases[i].Index, cases[i].Type, i, patternTypeName, cases[i].Empty ? null : $"cvt{converterCount++}"));
        }
        this.slots = slots.ToImmutable();
    }

    private void AppendConverterHead()
    {
        var slots = this.slots;
        Output.AppendIndent(1, $"private sealed class {OutputConverterTypeName}(", ")", slots.Length, i => slots[i].ConverterName is not null, i => $"{GetConverterTypeFullName(slots[i].Position)} {slots[i].ConverterName}");
        Output.AppendIndent(2, $": Mikodev.Binary.Converter<{SymbolTypeFullName}>(0)");
        Output.AppendIndent(1, $"{{");
        CancellationToken.ThrowIfCancellationRequested();
    }

    private void AppendConverterTail()
    {
        Output.AppendIndent(1, $"}}");
        Output.AppendEmptyLine();
    }

    private void AppendEncodeMethod(bool auto)
    {
        var slots = this.slots;
        if (auto)
            Output.AppendEmptyLine();
        Output.AppendIndent(2, $"public override void {(auto ? "EncodeAuto" : "Encode")}(ref Mikodev.Binary.Allocator allocator, {SymbolTypeFullName} item)");
        Output.AppendIndent(2, $"{{");
        Output.AppendIndent(3, $"switch (item)");
        Output.AppendIndent(3, $"{{");
        for (var i = 0; i < slots.Length; i++)
        {
            var slot = slots[i];
            Output.AppendIndent(4, slot.ConverterName is null ? $"case {slot.PatternTypeName}:" : $"case {slot.PatternTypeName} var{i}:");
            Output.AppendIndent(5, $"Mikodev.Binary.Converter.Encode(ref allocator, {slot.Index});");
            if (slot.ConverterName is { } converter)
                Output.AppendIndent(5, $"{converter}.{(auto ? "EncodeAuto" : "Encode")}(ref allocator, var{i});");
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
        var slots = this.slots;
        Output.AppendEmptyLine();
        Output.AppendIndent(2, $"public override {SymbolTypeFullName} {(auto ? "DecodeAuto" : "Decode")}({(auto ? "ref" : "in")} System.ReadOnlySpan<byte> span)");
        Output.AppendIndent(2, $"{{");
        if (auto is false)
            Output.AppendIndent(3, $"var copy = span;");
        var bufferName = auto ? "span" : "copy";
        Output.AppendIndent(3, $"var index = Mikodev.Binary.Converter.Decode(ref {bufferName});");
        Output.AppendIndent(3, $"return index switch");
        Output.AppendIndent(3, $"{{");
        for (var i = 0; i < slots.Length; i++)
        {
            var slot = slots[i];
            var decode = slot.ConverterName is { } converter
                ? $"{converter}.{(auto ? "DecodeAuto" : "Decode")}({(auto ? "ref" : "in")} {bufferName})"
                : $"new {GetTypeFullName(slot.Position)}()";
            Output.AppendIndent(4, $"{slot.Index} => {decode},");
            CancellationToken.ThrowIfCancellationRequested();
        }
        Output.AppendIndent(4, $"_ => throw new System.ArgumentException($\"Invalid union tag '{{index}}', type: {{typeof({SymbolTypeFullName})}}\"),");
        Output.AppendIndent(3, $"}};");
        Output.AppendIndent(2, $"}}");
    }

    private void AppendConverterCreatorBody()
    {
        var slots = this.slots;
        foreach (var slot in slots)
        {
            if (slot.ConverterName is null)
                continue;
            AppendConverterFromContext(slot.Type, slot.ConverterName!, GetTypeFullName(slot.Position));
            CancellationToken.ThrowIfCancellationRequested();
        }
        Output.AppendIndent(3, $"var converter = new {OutputConverterTypeName}(", ");", slots.Length, i => slots[i].ConverterName is not null, i => slots[i].ConverterName!);
    }

    protected override void Emit()
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
