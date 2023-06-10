namespace Mikodev.Binary.SourceGeneration.Contexts;

using Microsoft.CodeAnalysis;
using Mikodev.Binary.SourceGeneration;
using Mikodev.Binary.SourceGeneration.Internal;
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
        builder.AppendIndent(2, $": Mikodev.Binary.Components.TupleObjectConverter<{SymbolTypeFullName}>(Mikodev.Binary.Components.TupleObject.GetConverterLength(new {Constants.IConverterTypeName}[] {{ ", $" }}))", members.Length, x => $"cvt{x}");
        builder.AppendIndent(1, $"{{");
        CancellationToken.ThrowIfCancellationRequested();
    }

    private void AppendConverterTail(StringBuilder builder)
    {
        builder.AppendIndent(1, $"}}");
        builder.AppendIndent();
    }

    private void AppendExceptMethod(StringBuilder builder)
    {
        if (Symbol.IsValueType)
            return;
        builder.AppendIndent(2, $"[System.Diagnostics.CodeAnalysis.DoesNotReturn]");
        builder.AppendIndent(2, $"private static void Except()");
        builder.AppendIndent(2, $"{{");
        builder.AppendIndent(3, $"throw new System.ArgumentNullException(\"item\", $\"Tuple can not be null, type: {{typeof({SymbolTypeFullName})}}\");");
        builder.AppendIndent(2, $"}}");
        builder.AppendIndent();
        CancellationToken.ThrowIfCancellationRequested();
    }

    private void AppendEnsureMethod(StringBuilder builder)
    {
        if (Symbol.IsValueType)
            return;
        builder.AppendIndent(2, $"[System.Diagnostics.DebuggerStepThrough]");
        builder.AppendIndent(2, $"private static void Ensure({SymbolTypeFullName} item)");
        builder.AppendIndent(2, $"{{");
        builder.AppendIndent(3, $"if (item is not null)");
        builder.AppendIndent(4, $"return;");
        builder.AppendIndent(3, $"Except();");
        builder.AppendIndent(2, $"}}");
        builder.AppendIndent();
        CancellationToken.ThrowIfCancellationRequested();
    }

    private void AppendEnsureFragment(StringBuilder builder)
    {
        if (Symbol.IsValueType)
            return;
        builder.AppendIndent(3, "Ensure(item);");
        CancellationToken.ThrowIfCancellationRequested();
    }

    private void AppendEncodeMethod(StringBuilder builder, bool auto)
    {
        var members = this.members;
        var methodName = auto ? "EncodeAuto" : "Encode";
        if (auto)
            builder.AppendIndent();
        builder.AppendIndent(2, $"public override void {methodName}(ref {Constants.AllocatorTypeName} allocator, {SymbolTypeFullName} item)");
        builder.AppendIndent(2, $"{{");
        AppendEnsureFragment(builder);
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
        var constructor = this.constructor;
        if (constructor is null)
            return;
        var members = this.members;
        var modifier = auto ? "ref" : "in";
        var methodName = auto ? "DecodeAuto" : "Decode";
        builder.AppendIndent();
        builder.AppendIndent(2, $"public override {SymbolTypeFullName} {methodName}({modifier} System.ReadOnlySpan<byte> span)");
        builder.AppendIndent(2, $"{{");
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
        AppendExceptMethod(builder);
        AppendEnsureMethod(builder);
        AppendEncodeMethod(builder, auto: false);
        AppendEncodeMethod(builder, auto: true);
        AppendDecodeMethod(builder, auto: false);
        AppendDecodeMethod(builder, auto: true);
        AppendConverterTail(builder);

        AppendConverterCreatorHead(builder);
        AppendConverterCreatorBody(builder);
        AppendConverterCreatorTail(builder);
    }
}
