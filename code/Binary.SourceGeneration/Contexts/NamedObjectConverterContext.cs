namespace Mikodev.Binary.SourceGeneration.Contexts;

using Microsoft.CodeAnalysis;
using Mikodev.Binary.SourceGeneration;
using System.Collections.Immutable;
using System.Text;

public sealed partial class NamedObjectConverterContext : SymbolConverterContext
{
    private readonly ImmutableArray<SymbolNamedMemberInfo> members;

    private readonly SymbolConstructorInfo<SymbolNamedMemberInfo>? constructor;

    private NamedObjectConverterContext(SourceGeneratorContext context, ITypeSymbol symbol, ImmutableArray<SymbolNamedMemberInfo> members, SymbolConstructorInfo<SymbolNamedMemberInfo>? constructor) : base(context, symbol)
    {
        for (var i = 0; i < members.Length; i++)
            AddType(i, members[i].TypeSymbol);
        this.members = members;
        this.constructor = constructor;
    }

    private void AppendConstructor(StringBuilder builder)
    {
        var members = this.members;
        builder.AppendIndent(2, $"public {ClosureTypeName}(", ")", members.Length, i => $"byte[] key{i}, {GetConverterTypeFullName(i)} arg{i}");
        builder.AppendIndent(2, $"{{");
        for (var i = 0; i < members.Length; i++)
        {
            var member = members[i];
            builder.AppendIndent(3, $"this.key{i} = key{i};");
            builder.AppendIndent(3, $"this.cvt{i} = arg{i};");
            CancellationToken.ThrowIfCancellationRequested(); ;
        }
        builder.AppendIndent(2, $"}}");
    }

    private void AppendEncodeMethod(StringBuilder builder)
    {
        var members = this.members;
        builder.AppendIndent();
        builder.AppendIndent(2, $"public void Encode(ref {Constants.AllocatorTypeName} allocator, {SymbolTypeFullName} item)");
        builder.AppendIndent(2, $"{{");
        for (var i = 0; i < members.Length; i++)
        {
            var member = members[i];
            if (member.IsOptional is false)
            {
                builder.AppendIndent(3, $"{Constants.AllocatorTypeName}.Append(ref allocator, new System.ReadOnlySpan<byte>(this.key{i}));");
                builder.AppendIndent(3, $"this.cvt{i}.EncodeWithLengthPrefix(ref allocator, item.{member.Name});");
                CancellationToken.ThrowIfCancellationRequested();
            }
            else
            {
                builder.AppendIndent(3, $"var loc{i} = item.{member.Name};");
                builder.AppendIndent(3, $"if (System.Collections.Generic.EqualityComparer<{GetTypeFullName(i)}>.Default.Equals(loc{i}, default) is false)");
                builder.AppendIndent(3, $"{{");
                builder.AppendIndent(4, $"{Constants.AllocatorTypeName}.Append(ref allocator, this.key{i});");
                builder.AppendIndent(4, $"this.cvt{i}.EncodeWithLengthPrefix(ref allocator, loc{i});");
                builder.AppendIndent(3, $"}}");
                CancellationToken.ThrowIfCancellationRequested();
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
        builder.AppendIndent(2, $"public {SymbolTypeFullName} Decode(scoped Mikodev.Binary.Components.NamedObjectConstructorParameter parameter)");
        builder.AppendIndent(2, $"{{");

        for (var i = 0; i < members.Length; i++)
        {
            var member = members[i];
            if (member.IsOptional is false)
                builder.AppendIndent(3, $"var var{i} = this.cvt{i}.Decode(parameter.GetValue({i}));");
            else
                builder.AppendIndent(3, $"var var{i} = parameter.HasValue({i}) ? this.cvt{i}.Decode(parameter.GetValue({i})) : default;");
            CancellationToken.ThrowIfCancellationRequested(); ;
        }

        constructor.Append(builder, SymbolTypeFullName, CancellationToken);
        builder.AppendIndent(2, $"}}");
    }

    private void AppendFields(StringBuilder builder)
    {
        var members = this.members;
        for (var i = 0; i < members.Length; i++)
        {
            builder.AppendIndent(2, $"private readonly byte[] key{i};");
            builder.AppendIndent();
            builder.AppendIndent(2, $"private readonly {GetConverterTypeFullName(i)} cvt{i};");
            builder.AppendIndent();
            CancellationToken.ThrowIfCancellationRequested(); ;
        }
    }

    private void AppendConverterCreatorBody(StringBuilder builder)
    {
        var members = this.members;
        var decoder = this.constructor is null ? "null" : "closure.Decode";

        builder.AppendIndent(3, $"var names = new string[] {{ ", $" }};", members.Length, x => members[x].NamedKeyLiteral);
        builder.AppendIndent(3, $"var optional = new bool[] {{ ", $" }};", members.Length, x => members[x].IsOptional ? "true" : "false");
        builder.AppendIndent(3, $"var stringConverter = ({Constants.ConverterTypeName}<string>)context.GetConverter(typeof(string));");
        for (var i = 0; i < members.Length; i++)
        {
            var member = members[i];
            AppendAssignConverter(builder, member, $"cvt{i}", GetConverterTypeFullName(i), GetTypeFullName(i));
            builder.AppendIndent(3, $"var key{i} = {Constants.AllocatorTypeName}.Invoke(names[{i}], stringConverter.EncodeWithLengthPrefix);");
            CancellationToken.ThrowIfCancellationRequested(); ;
        }
        builder.AppendIndent(3, $"var closure = new {ClosureTypeName}(", ");", members.Length, x => $"key{x}, cvt{x}");
        builder.AppendIndent(3, $"var converter = Mikodev.Binary.Components.NamedObject.GetNamedObjectConverter<{SymbolTypeFullName}>(closure.Encode, {decoder}, stringConverter, names, optional);");
        builder.AppendIndent(3, $"return ({Constants.IConverterTypeName})converter;");
    }

    protected override void Invoke(StringBuilder builder)
    {
        AppendClosureHead(builder);
        AppendFields(builder);
        AppendConstructor(builder);
        AppendEncodeMethod(builder);
        AppendDecodeMethod(builder);
        AppendClosureTail(builder);
        builder.AppendIndent();

        AppendConverterCreatorHead(builder);
        AppendConverterCreatorBody(builder);
        AppendConverterCreatorTail(builder);
    }
}
