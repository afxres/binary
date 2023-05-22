namespace Mikodev.Binary.SourceGeneration.Contexts;

using Microsoft.CodeAnalysis;
using Mikodev.Binary.SourceGeneration;
using System.Collections.Immutable;
using System.Text;

public sealed partial class TupleObjectConverterContext : SymbolConverterContext
{
    private readonly ImmutableArray<SymbolTupleMemberInfo> members;

    private readonly SymbolConstructorInfo<SymbolTupleMemberInfo>? constructor;

    private TupleObjectConverterContext(SourceGeneratorContext context, SourceGeneratorTracker tracker, ITypeSymbol symbol, ImmutableArray<SymbolTupleMemberInfo> members, SymbolConstructorInfo<SymbolTupleMemberInfo>? constructor) : base(context, tracker, symbol)
    {
        for (var i = 0; i < members.Length; i++)
            AddType(i, members[i].Type);
        this.members = members;
        this.constructor = constructor;
    }

    private void AppendConverterHead(StringBuilder builder)
    {
        var members = this.members;
        builder.AppendIndent(1, $"private sealed class {OutputConverterTypeName}(", ")", members.Length, i => $"{GetConverterTypeFullName(i)} cvt{i}");
        builder.AppendIndent(2, $": {SymbolConverterTypeFullName}(Mikodev.Binary.Components.TupleObject.GetTupleObjectLength(new {Constants.IConverterTypeName}[] {{ ", $" }}))", members.Length, x => $"cvt{x}");
        builder.AppendIndent(1, $"{{");
        CancellationToken.ThrowIfCancellationRequested();
    }

    private void AppendConverterTail(StringBuilder builder)
    {
        builder.AppendIndent(1, $"}}");
    }

    private void AppendEncodeMethod(StringBuilder builder, bool auto)
    {
        var members = this.members;
        var methodName = auto ? "EncodeAuto" : "Encode";
        builder.AppendIndent(2, $"public override void {methodName}(ref {Constants.AllocatorTypeName} allocator, {SymbolTypeFullName} item)");
        builder.AppendIndent(2, $"{{");
        if (Symbol.IsValueType is false)
        {
            builder.AppendIndent(3, $"System.ArgumentNullException.ThrowIfNull(item);");
            CancellationToken.ThrowIfCancellationRequested();
        }

        for (var i = 0; i < members.Length; i++)
        {
            var member = members[i];
            builder.AppendIndent(3, $"var var{i} = item.{member.NameInSourceCode};");
            CancellationToken.ThrowIfCancellationRequested();
        }
        for (var i = 0; i < members.Length; i++)
        {
            var last = (i == members.Length - 1);
            var method = (auto || last is false) ? "EncodeAuto" : "Encode";
            builder.AppendIndent(3, $"cvt{i}.{method}(ref allocator, var{i});");
            CancellationToken.ThrowIfCancellationRequested();
        }
        builder.AppendIndent(2, $"}}");
    }

    private void AppendDecodeMethod(StringBuilder builder, bool auto)
    {
        var members = this.members;
        var constructor = this.constructor;
        var modifier = auto ? "ref" : "in";
        var methodName = auto ? "DecodeAuto" : "Decode";
        builder.AppendIndent(2, $"public override {SymbolTypeFullName} {methodName}({modifier} System.ReadOnlySpan<byte> span)");
        builder.AppendIndent(2, $"{{");
        if (constructor is null)
        {
            builder.AppendIndent(3, $"throw new System.NotSupportedException($\"No suitable constructor found, type: {{typeof({SymbolTypeFullName})}}\");");
        }
        else
        {
            if (auto is false)
                builder.AppendIndent(3, $"var body = span;");
            var bufferName = auto ? "span" : "body";
            for (var i = 0; i < members.Length; i++)
            {
                var last = (i == members.Length - 1);
                var method = (auto || last is false) ? "DecodeAuto" : "Decode";
                var keyword = (auto is false && last) ? "in" : "ref";
                builder.AppendIndent(3, $"var var{i} = cvt{i}.{method}({keyword} {bufferName});");
                CancellationToken.ThrowIfCancellationRequested();
            }
            constructor.Append(builder, SymbolTypeFullName, CancellationToken);
        }
        builder.AppendIndent(2, $"}}");
    }

    private void AppendConverterCreatorBody(StringBuilder builder)
    {
        var members = this.members;
        for (var i = 0; i < members.Length; i++)
        {
            var member = members[i];
            AppendAssignConverter(builder, member, $"cvt{i}", GetConverterTypeFullName(i), GetTypeFullName(i));
            CancellationToken.ThrowIfCancellationRequested();
        }
        builder.AppendIndent(3, $"var converter = new {OutputConverterTypeName}(", ");", members.Length, x => $"cvt{x}");
    }

    protected override void Invoke(StringBuilder builder)
    {
        AppendConverterHead(builder);
        AppendEncodeMethod(builder, auto: false);
        builder.AppendIndent();
        AppendEncodeMethod(builder, auto: true);
        builder.AppendIndent();
        AppendDecodeMethod(builder, auto: false);
        builder.AppendIndent();
        AppendDecodeMethod(builder, auto: true);
        AppendConverterTail(builder);
        builder.AppendIndent();

        AppendConverterCreatorHead(builder);
        AppendConverterCreatorBody(builder);
        AppendConverterCreatorTail(builder);
    }
}
