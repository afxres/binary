namespace Mikodev.Binary.SourceGeneration.Contexts;

using Microsoft.CodeAnalysis;
using Mikodev.Binary.SourceGeneration;
using System.Collections.Immutable;
using System.Text;

public sealed partial class TupleObjectConverterContext : SymbolConverterContext
{
    private readonly ImmutableArray<SymbolTupleMemberInfo> members;

    private readonly SymbolConstructorInfo<SymbolTupleMemberInfo>? constructor;

    private TupleObjectConverterContext(SourceGeneratorContext context, ITypeSymbol symbol, ImmutableArray<SymbolTupleMemberInfo> members, SymbolConstructorInfo<SymbolTupleMemberInfo>? constructor) : base(context, symbol)
    {
        for (var i = 0; i < members.Length; i++)
            AddType(i, members[i].TypeSymbol);
        this.members = members;
        this.constructor = constructor;
    }

    private void AppendConstructor(StringBuilder builder)
    {
        var members = this.members;
        builder.AppendIndent(2, $"public {ConverterTypeName}(", ")", members.Length, i => $"{GetConverterTypeFullName(i)} arg{i}");
        builder.AppendIndent(3, $": base(Mikodev.Binary.Components.TupleObject.GetTupleObjectLength(new {Constants.IConverterTypeName}[] {{ ", $" }}))", members.Length, x => $"arg{x}");
        builder.AppendIndent(2, $"{{");
        for (var i = 0; i < members.Length; i++)
        {
            builder.AppendIndent(3, $"this.cvt{i} = arg{i};");
            CancellationToken.ThrowIfCancellationRequested();
        }
        builder.AppendIndent(2, $"}}");
    }

    private void AppendEncodeMethod(StringBuilder builder, bool auto)
    {
        var members = this.members;
        var methodName = auto ? "EncodeAuto" : "Encode";
        builder.AppendIndent();
        builder.AppendIndent(2, $"public override void {methodName}(ref {Constants.AllocatorTypeName} allocator, {SymbolTypeFullName} item)");
        builder.AppendIndent(2, $"{{");
        if (Symbol.IsValueType is false)
        {
            builder.AppendIndent(3, $"System.ArgumentNullException.ThrowIfNull(item);");
            CancellationToken.ThrowIfCancellationRequested();
        }

        for (var i = 0; i < members.Length; i++)
        {
            var last = (i == members.Length - 1);
            var member = members[i];
            var method = (auto || last is false) ? "EncodeAuto" : "Encode";
            builder.AppendIndent(3, $"this.cvt{i}.{method}(ref allocator, item.{member.Name});");
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
        builder.AppendIndent();
        builder.AppendIndent(2, $"public override {SymbolTypeFullName} {methodName}({modifier} global::System.ReadOnlySpan<byte> span)");
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
                builder.AppendIndent(3, $"var var{i} = this.cvt{i}.{method}({keyword} {bufferName});");
                CancellationToken.ThrowIfCancellationRequested();
            }
            constructor.AppendCreateInstance(builder, CancellationToken);
        }
        builder.AppendIndent(2, $"}}");
    }

    private void AppendFields(StringBuilder builder)
    {
        var members = this.members;
        for (var i = 0; i < members.Length; i++)
        {
            builder.AppendIndent(2, $"private readonly {GetConverterTypeFullName(i)} cvt{i};");
            builder.AppendIndent();
            CancellationToken.ThrowIfCancellationRequested();
        }
    }

    private void AppendConverterCreatorBody(StringBuilder builder)
    {
        var members = this.members;
        for (var i = 0; i < members.Length; i++)
        {
            var member = members[i];
            AppendAssignConverter(builder, member, $"cvt{i}", $"{GetConverterTypeFullName(i)}", GetTypeFullName(i));
            CancellationToken.ThrowIfCancellationRequested();
        }

        builder.AppendIndent(3, $"var converter = new {ConverterTypeName}(", ");", members.Length, x => $"cvt{x}");
        builder.AppendIndent(3, $"return ({Constants.IConverterTypeName})converter;");
    }

    protected override void Invoke(StringBuilder builder)
    {
        AppendConverterHead(builder);
        AppendFields(builder);
        AppendConstructor(builder);
        AppendEncodeMethod(builder, auto: false);
        AppendEncodeMethod(builder, auto: true);
        AppendDecodeMethod(builder, auto: false);
        AppendDecodeMethod(builder, auto: true);
        AppendConverterTail(builder);
        builder.AppendIndent();

        AppendConverterCreatorHead(builder);
        AppendConverterCreatorBody(builder);
        AppendConverterCreatorTail(builder);
    }
}
