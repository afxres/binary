namespace Mikodev.Binary.SourceGeneration.Contexts;

using Microsoft.CodeAnalysis;
using Mikodev.Binary.SourceGeneration;
using Mikodev.Binary.SourceGeneration.Internal;
using System.Collections.Immutable;

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

    private void AppendConverterHead()
    {
        var members = this.members;
        Output.AppendIndent(1, $"private sealed class {OutputConverterTypeName}(", ")", members.Length, i => $"{GetConverterTypeFullName(i)} cvt{i}");
        Output.AppendIndent(2, $": Mikodev.Binary.Components.TupleObjectConverter<{SymbolTypeFullName}>(Mikodev.Binary.Components.TupleObject.GetConverterLength(new {Constants.IConverterTypeName}[] {{ ", $" }}))", members.Length, x => $"cvt{x}");
        Output.AppendIndent(1, $"{{");
        CancellationToken.ThrowIfCancellationRequested();
    }

    private void AppendConverterTail()
    {
        Output.AppendIndent(1, $"}}");
        Output.AppendIndent();
    }

    private void AppendExceptMethod()
    {
        if (Symbol.IsValueType)
            return;
        Output.AppendIndent(2, $"[System.Diagnostics.CodeAnalysis.DoesNotReturn]");
        Output.AppendIndent(2, $"private static void Except()");
        Output.AppendIndent(2, $"{{");
        Output.AppendIndent(3, $"throw new System.ArgumentNullException(\"item\", $\"Tuple can not be null, type: {{typeof({SymbolTypeFullName})}}\");");
        Output.AppendIndent(2, $"}}");
        Output.AppendIndent();
        CancellationToken.ThrowIfCancellationRequested();
    }

    private void AppendEnsureMethod()
    {
        if (Symbol.IsValueType)
            return;
        Output.AppendIndent(2, $"[System.Diagnostics.DebuggerStepThrough]");
        Output.AppendIndent(2, $"private static void Ensure({SymbolTypeFullName} item)");
        Output.AppendIndent(2, $"{{");
        Output.AppendIndent(3, $"if (item is not null)");
        Output.AppendIndent(4, $"return;");
        Output.AppendIndent(3, $"Except();");
        Output.AppendIndent(2, $"}}");
        Output.AppendIndent();
        CancellationToken.ThrowIfCancellationRequested();
    }

    private void AppendEnsureContext()
    {
        if (Symbol.IsValueType)
            return;
        Output.AppendIndent(3, "Ensure(item);");
        CancellationToken.ThrowIfCancellationRequested();
    }

    private void AppendEncodeMethod(bool auto)
    {
        var members = this.members;
        var methodName = auto ? "EncodeAuto" : "Encode";
        if (auto)
            Output.AppendIndent();
        Output.AppendIndent(2, $"public override void {methodName}(ref {Constants.AllocatorTypeName} allocator, {SymbolTypeFullName} item)");
        Output.AppendIndent(2, $"{{");
        AppendEnsureContext();
        for (var i = 0; i < members.Length; i++)
        {
            var member = members[i];
            Output.AppendIndent(3, $"var var{i} = item.{member.NameInSourceCode};");
            CancellationToken.ThrowIfCancellationRequested();
        }
        for (var i = 0; i < members.Length; i++)
        {
            var last = (i == members.Length - 1);
            var method = (auto || last is false) ? "EncodeAuto" : "Encode";
            Output.AppendIndent(3, $"cvt{i}.{method}(ref allocator, var{i});");
            CancellationToken.ThrowIfCancellationRequested();
        }
        Output.AppendIndent(2, $"}}");
    }

    private void AppendDecodeMethod(bool auto)
    {
        var constructor = this.constructor;
        if (constructor is null)
            return;
        var members = this.members;
        var modifier = auto ? "ref" : "in";
        var methodName = auto ? "DecodeAuto" : "Decode";
        Output.AppendIndent();
        Output.AppendIndent(2, $"public override {SymbolTypeFullName} {methodName}({modifier} System.ReadOnlySpan<byte> span)");
        Output.AppendIndent(2, $"{{");
        if (auto is false)
            Output.AppendIndent(3, $"var body = span;");
        var bufferName = auto ? "span" : "body";
        for (var i = 0; i < members.Length; i++)
        {
            var last = (i == members.Length - 1);
            var method = (auto || last is false) ? "DecodeAuto" : "Decode";
            var keyword = (auto is false && last) ? "in" : "ref";
            Output.AppendIndent(3, $"var var{i} = cvt{i}.{method}({keyword} {bufferName});");
            CancellationToken.ThrowIfCancellationRequested();
        }
        constructor.AppendTo(Output, SymbolTypeFullName, CancellationToken);
        Output.AppendIndent(2, $"}}");
    }

    private void AppendConverterCreatorBody()
    {
        var members = this.members;
        for (var i = 0; i < members.Length; i++)
        {
            var member = members[i];
            AppendAssignConverter(member, $"cvt{i}", GetConverterTypeFullName(i), GetTypeFullName(i));
            CancellationToken.ThrowIfCancellationRequested();
        }
        Output.AppendIndent(3, $"var converter = new {OutputConverterTypeName}(", ");", members.Length, x => $"cvt{x}");
    }

    protected override void Handle()
    {
        AppendConverterHead();
        AppendExceptMethod();
        AppendEnsureMethod();
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
