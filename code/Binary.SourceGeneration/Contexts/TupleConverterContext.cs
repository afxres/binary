namespace Mikodev.Binary.SourceGeneration.Contexts;

using Microsoft.CodeAnalysis;
using Mikodev.Binary.SourceGeneration;
using Mikodev.Binary.SourceGeneration.Internal;
using System.Collections.Immutable;

public sealed partial class TupleConverterContext : SymbolConverterContext
{
    private readonly ImmutableArray<SymbolTupleMemberInfo> members;

    private TupleConverterContext(SourceGeneratorContext context, SourceGeneratorTracker tracker, ITypeSymbol symbol, ImmutableArray<SymbolTupleMemberInfo> members) : base(context, tracker, symbol)
    {
        members.ForEach((index, value) => AddType(index, value.Type));
        this.members = members;
    }

    private void AppendConverterHead()
    {
        var members = this.members;
        Output.AppendIndent(1, $"private sealed class {OutputConverterTypeName}(", ")", members.Length, i => $"{GetConverterTypeFullName(i)} cvt{i}");
        Output.AppendIndent(2, $": Mikodev.Binary.Converter<{SymbolTypeFullName}>(Mikodev.Binary.Components.TupleObject.GetConverterLength([", $"]))", members.Length, x => $"cvt{x}");
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
        var members = this.members;
        if (auto)
            Output.AppendIndent();
        Output.AppendIndent(2, $"public override void {(auto ? "EncodeAuto" : "Encode")}(ref Mikodev.Binary.Allocator allocator, {SymbolTypeFullName} item)");
        Output.AppendIndent(2, $"{{");
        for (var i = 0; i < members.Length; i++)
        {
            var flag = auto || (i != members.Length - 1);
            Output.AppendIndent(3, $"cvt{i}.{(flag ? "EncodeAuto" : "Encode")}(ref allocator, item{members[i].Path});");
            CancellationToken.ThrowIfCancellationRequested();
        }
        Output.AppendIndent(2, $"}}");
    }

    private void AppendDecodeMethod(bool auto)
    {
        var members = this.members;
        Output.AppendIndent();
        Output.AppendIndent(2, $"public override {SymbolTypeFullName} {(auto ? "DecodeAuto" : "Decode")}({(auto ? "ref" : "in")} System.ReadOnlySpan<byte> span)");
        Output.AppendIndent(2, $"{{");
        if (auto is false)
            Output.AppendIndent(3, $"var copy = span;");
        Output.AppendIndent(3, $"var result = default({SymbolTypeFullName});");
        var bufferName = auto ? "span" : "copy";
        for (var i = 0; i < members.Length; i++)
        {
            var flag = auto || (i != members.Length - 1);
            Output.AppendIndent(3, $"result{members[i].Path} = cvt{i}.{(flag ? "DecodeAuto" : "Decode")}({(flag ? "ref" : "in")} {bufferName});");
            CancellationToken.ThrowIfCancellationRequested();
        }
        Output.AppendIndent(3, $"return result;");
        Output.AppendIndent(2, $"}}");
    }

    private void AppendConverterCreatorBody()
    {
        var members = this.members;
        for (var i = 0; i < members.Length; i++)
        {
            var member = members[i];
            AppendAssignConverterExplicit(member.Type, $"cvt{i}", GetTypeFullName(i));
            CancellationToken.ThrowIfCancellationRequested();
        }
        Output.AppendIndent(3, $"var converter = new {OutputConverterTypeName}(", ");", members.Length, x => $"cvt{x}");
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
