﻿namespace Mikodev.Binary.SourceGeneration.Contexts;

using Microsoft.CodeAnalysis;
using Mikodev.Binary.SourceGeneration;
using System.Collections.Immutable;
using System.Text;

public sealed partial class TupleObjectConverterContext : SymbolConverterContext
{
    private readonly ImmutableArray<SymbolTupleMemberInfo> members;

    private readonly SymbolConstructorInfo<SymbolTupleMemberInfo>? constructor;

    private TupleObjectConverterContext(SourceGeneratorContext context, ITypeSymbol symbol, ImmutableArray<SymbolTupleMemberInfo> members) : base(context, symbol)
    {
        TypeAliases.Add("System.ReadOnlySpan<byte>", "Span", SymbolTypeAliasesFlags.Type);
        TypeAliases.Add(Constants.AllocatorTypeName, "Allocator", SymbolTypeAliasesFlags.Type);
        for (var i = 0; i < members.Length; i++)
            TypeAliases.Add(members[i].TypeSymbol, i.ToString());
        this.members = members;
        this.constructor = Symbols.GetConstructor(symbol, members);
    }

    private void AppendConstructor(StringBuilder builder)
    {
        var members = this.members;
        builder.AppendIndent(2, $"public {ConverterTypeName}(", ")", members.Length, x => $"_C{x} arg{x}");
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
        builder.AppendIndent(2, $"public override void {methodName}(ref _TAllocator allocator, _TSelf item)");
        builder.AppendIndent(2, $"{{");
        if (TypeSymbol.IsValueType is false)
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
        var modifier = auto ? "ref" : "in";
        var methodName = auto ? "DecodeAuto" : "Decode";
        var constructor = this.constructor;
        if (constructor is null)
        {
            builder.AppendIndent();
            builder.AppendIndent(2, $"public override _TSelf {methodName}({modifier} _TSpan span) => throw new System.NotSupportedException();");
            CancellationToken.ThrowIfCancellationRequested();
            return;
        }

        var members = this.members;
        builder.AppendIndent();
        builder.AppendIndent(2, $"public override _TSelf {methodName}({modifier} _TSpan span)");
        builder.AppendIndent(2, $"{{");
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
        builder.AppendIndent(2, $"}}");
    }

    private void AppendFields(StringBuilder builder)
    {
        var members = this.members;
        for (var i = 0; i < members.Length; i++)
        {
            builder.AppendIndent(2, $"private readonly _C{i} cvt{i};");
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
            AppendAssignConverter(builder, member, $"cvt{i}", $"_C{i}", $"_T{i}");
            CancellationToken.ThrowIfCancellationRequested();
        }

        builder.AppendIndent(3, $"var converter = new {ConverterTypeName}(", ");", members.Length, x => $"cvt{x}");
        builder.AppendIndent(3, $"return ({Constants.IConverterTypeName})converter;");
    }

    private void Invoke()
    {
        var builder = new StringBuilder();
        AppendFileHead(builder);

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

        AppendFileTail(builder);
        Finish(builder);
    }
}