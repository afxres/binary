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
        var tail = ", byte[][] keys, Mikodev.Binary.Converter<string> converter, System.Collections.Generic.IEnumerable<string> names, System.Collections.Generic.IEnumerable<bool> optional)";
        builder.AppendIndent(1, $"private sealed class {OutputConverterTypeName}(", tail, members.Length, i => $"{GetConverterTypeFullName(i)} cvt{i}");
        builder.AppendIndent(2, $": Mikodev.Binary.Components.NamedObjectConverter<{SymbolTypeFullName}>(converter, names, optional)");
        builder.AppendIndent(1, $"{{");
        CancellationToken.ThrowIfCancellationRequested();
    }

    private void AppendConverterTail(StringBuilder builder)
    {
        builder.AppendIndent(1, $"}}");
    }

    private void AppendEnsureFragment(StringBuilder builder)
    {
        if (Symbol.IsValueType)
            return;
        builder.AppendIndent(3, $"if (item is null)");
        builder.AppendIndent(4, "return;");
        CancellationToken.ThrowIfCancellationRequested();
    }

    private void AppendEncodeFragment(StringBuilder builder, int indent, int i)
    {
        builder.AppendIndent(indent, $"{Constants.AllocatorTypeName}.Append(ref allocator, new System.ReadOnlySpan<byte>(keys[{i}]));");
        builder.AppendIndent(indent, $"cvt{i}.EncodeWithLengthPrefix(ref allocator, var{i});");
        CancellationToken.ThrowIfCancellationRequested();
    }

    private void AppendEncodeMethod(StringBuilder builder)
    {
        var members = this.members;
        builder.AppendIndent(2, $"public override void Encode(ref {Constants.AllocatorTypeName} allocator, {SymbolTypeFullName} item)");
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
            if (members[i].IsOptional is false)
            {
                AppendEncodeFragment(builder, 3, i);
            }
            else
            {
                builder.AppendIndent(3, $"if (System.Collections.Generic.EqualityComparer<{GetTypeFullName(i)}>.Default.Equals(var{i}, default) is false)");
                builder.AppendIndent(3, $"{{");
                AppendEncodeFragment(builder, 4, i);
                builder.AppendIndent(3, $"}}");
            }
        }
        builder.AppendIndent(2, $"}}");
    }

    private void AppendDecodeMethod(StringBuilder builder)
    {
        var constructor = this.constructor;
        if (constructor is null)
            return;
        var members = this.members;
        builder.AppendIndent();
        builder.AppendIndent(2, $"public override {SymbolTypeFullName} Decode(scoped Mikodev.Binary.Components.NamedObjectParameter parameter)");
        builder.AppendIndent(2, $"{{");
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
            CancellationToken.ThrowIfCancellationRequested();
        }
        builder.AppendIndent(3, $"var keys = System.Array.ConvertAll(names, x => Mikodev.Binary.Allocator.Invoke(x, encoding.EncodeWithLengthPrefix));");
        builder.AppendIndent(3, $"var converter = new {OutputConverterTypeName}(", ", keys, encoding, names, optional);", members.Length, x => $"cvt{x}");
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
