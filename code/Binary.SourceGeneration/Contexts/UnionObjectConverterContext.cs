namespace Mikodev.Binary.SourceGeneration.Contexts;

using Microsoft.CodeAnalysis;
using Mikodev.Binary.SourceGeneration.Internal;
using System.Collections.Immutable;
using System.Linq;

public sealed partial class UnionObjectConverterContext : SymbolConverterContext
{
    private sealed record UnionCaseInfo(int Index, ITypeSymbol Type, bool Empty);

    private sealed record UnionCaseSlot(int Index, ITypeSymbol Type, int Position, string? ConverterName);

    private readonly ImmutableArray<UnionCaseSlot> slots;

    private UnionObjectConverterContext(SourceGeneratorContext context, SourceGeneratorTracker tracker, ITypeSymbol symbol, ImmutableArray<UnionCaseInfo> cases) : base(context, tracker, symbol)
    {
        var types = cases.Select(x => x.Type).ToImmutableArray();
        types.ForEach(AddType);
        var slots = ImmutableArray.CreateBuilder<UnionCaseSlot>(cases.Length);
        var converterCount = 0;
        for (var i = 0; i < cases.Length; i++)
            slots.Add(new UnionCaseSlot(cases[i].Index, cases[i].Type, i, cases[i].Empty ? null : $"cvt{converterCount++}"));
        this.slots = slots.ToImmutable();
    }

    private ImmutableArray<UnionCaseSlot> GetConverterSlots()
    {
        return [.. this.slots.Where(x => x.ConverterName is not null)];
    }

    private void AppendConverterHead()
    {
        var converters = GetConverterSlots();
        Output.AppendIndent(1, $"private sealed class {OutputConverterTypeName}(", ")", converters.Length, i => $"{GetConverterTypeFullName(converters[i].Position)} {converters[i].ConverterName}");
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
            Output.AppendIndent(4, slot.ConverterName is null ? $"case {GetTypeFullName(slot.Position)}:" : $"case {GetTypeFullName(slot.Position)} var{i}:");
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
        var converters = GetConverterSlots();
        foreach (var slot in converters)
        {
            AppendConverterFromContext(slot.Type, slot.ConverterName!, GetTypeFullName(slot.Position));
            CancellationToken.ThrowIfCancellationRequested();
        }
        Output.AppendIndent(3, $"var converter = new {OutputConverterTypeName}(", ");", converters.Length, i => converters[i].ConverterName!);
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
