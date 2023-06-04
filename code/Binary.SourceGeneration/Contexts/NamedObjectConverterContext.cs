namespace Mikodev.Binary.SourceGeneration.Contexts;

using Microsoft.CodeAnalysis;
using Mikodev.Binary.SourceGeneration;
using System.Collections.Immutable;
using System.Text;

public sealed partial class NamedObjectConverterContext : SymbolConverterContext
{
    private readonly ImmutableArray<SymbolNamedMemberInfo> members;

    private readonly SymbolConstructorInfo<SymbolNamedMemberInfo>? constructor;

    private NamedObjectConverterContext(SourceGeneratorContext context, SourceGeneratorTracker tracker, ITypeSymbol symbol, ImmutableArray<SymbolNamedMemberInfo> members, SymbolConstructorInfo<SymbolNamedMemberInfo>? constructor) : base(context, tracker, symbol)
    {
        for (var i = 0; i < members.Length; i++)
            AddType(i, members[i].Type);
        this.members = members;
        this.constructor = constructor;
    }

    private void AppendConverterHead(StringBuilder builder)
    {
        var members = this.members;
        var tail = ", Mikodev.Binary.Converter<string> converter, System.Collections.Generic.IEnumerable<string> names, System.Collections.Generic.IEnumerable<bool> optional)";
        builder.AppendIndent(1, $"private sealed class {OutputConverterTypeName}(", tail, members.Length, i => $"byte[] key{i}, {GetConverterTypeFullName(i)} cvt{i}");
        builder.AppendIndent(2, $": Mikodev.Binary.Components.NamedObjectConverter<{SymbolTypeFullName}>(converter, names, optional)");
        builder.AppendIndent(1, $"{{");
        CancellationToken.ThrowIfCancellationRequested();
    }

    private void AppendConverterTail(StringBuilder builder)
    {
        builder.AppendIndent(1, $"}}");
    }

    private void AppendEncodeMethod(StringBuilder builder)
    {
        var members = this.members;
        builder.AppendIndent(2, $"public override void Encode(ref {Constants.AllocatorTypeName} allocator, {SymbolTypeFullName} item)");
        builder.AppendIndent(2, $"{{");

        if (Symbol.IsValueType is false)
        {
            builder.AppendIndent(3, $"if (item is null)");
            builder.AppendIndent(4, "return;");
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
            var optional = members[i].IsOptional;
            if (optional)
            {
                builder.AppendIndent(3, $"if (System.Collections.Generic.EqualityComparer<{GetTypeFullName(i)}>.Default.Equals(var{i}, default) is false)");
                builder.AppendIndent(3, $"{{");
            }
            var indent = optional ? 4 : 3;
            builder.AppendIndent(indent, $"{Constants.AllocatorTypeName}.Append(ref allocator, new System.ReadOnlySpan<byte>(key{i}));");
            builder.AppendIndent(indent, $"cvt{i}.EncodeWithLengthPrefix(ref allocator, var{i});");
            if (optional)
            {
                builder.AppendIndent(3, $"}}");
            }
            CancellationToken.ThrowIfCancellationRequested();
        }

        builder.AppendIndent(2, $"}}");
    }

    private void AppendDecodeMethod(StringBuilder builder)
    {
        var members = this.members;
        var constructor = this.constructor;
        builder.AppendIndent();
        builder.AppendIndent(2, $"public override {SymbolTypeFullName} Decode(scoped Mikodev.Binary.Components.NamedObjectParameter parameter)");
        builder.AppendIndent(2, $"{{");

        if (constructor is null)
        {
            builder.AppendIndent(3, $"throw new System.NotSupportedException($\"No suitable constructor found, type: {{typeof({SymbolTypeFullName})}}\");");
        }
        else
        {
            for (var i = 0; i < members.Length; i++)
            {
                var member = members[i];
                if (member.IsOptional is false)
                    builder.AppendIndent(3, $"var var{i} = cvt{i}.Decode(parameter.GetValue({i}));");
                else
                    builder.AppendIndent(3, $"var var{i} = parameter.HasValue({i}) ? cvt{i}.Decode(parameter.GetValue({i})) : default;");
                CancellationToken.ThrowIfCancellationRequested();
            }
            constructor.Append(builder, SymbolTypeFullName, CancellationToken);
        }

        builder.AppendIndent(2, $"}}");
    }

    private void AppendConverterCreatorBody(StringBuilder builder)
    {
        var members = this.members;
        builder.AppendIndent(3, $"var names = new string[] {{ ", $" }};", members.Length, x => members[x].NamedKeyLiteral);
        builder.AppendIndent(3, $"var optional = new bool[] {{ ", $" }};", members.Length, x => members[x].IsOptional ? "true" : "false");
        builder.AppendIndent(3, $"var encoding = ({Constants.ConverterTypeName}<string>)context.GetConverter(typeof(string));");

        for (var i = 0; i < members.Length; i++)
        {
            var member = members[i];
            AppendAssignConverter(builder, member, $"cvt{i}", GetConverterTypeFullName(i), GetTypeFullName(i));
            builder.AppendIndent(3, $"var key{i} = {Constants.AllocatorTypeName}.Invoke(names[{i}], encoding.EncodeWithLengthPrefix);");
            CancellationToken.ThrowIfCancellationRequested();
        }

        builder.AppendIndent(3, $"var converter = new {OutputConverterTypeName}(", ", encoding, names, optional);", members.Length, x => $"key{x}, cvt{x}");
    }

    protected override void Invoke(StringBuilder builder)
    {
        AppendConverterHead(builder);
        AppendEncodeMethod(builder);
        AppendDecodeMethod(builder);
        AppendConverterTail(builder);
        builder.AppendIndent();

        AppendConverterCreatorHead(builder);
        AppendConverterCreatorBody(builder);
        AppendConverterCreatorTail(builder);
    }
}
