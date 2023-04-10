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
        TypeAliases.Add("System.String", "String");
        TypeAliases.Add("System.ReadOnlySpan<byte>", "Span", typeOnly: true);
        TypeAliases.Add(Constants.AllocatorTypeName, "Allocator", typeOnly: true);
        for (var i = 0; i < members.Length; i++)
            TypeAliases.Add(members[i].TypeSymbol, i.ToString());
        this.members = members;
        this.constructor = constructor;
    }

    private void AppendConstructor(StringBuilder builder)
    {
        var members = this.members;
        builder.AppendIndent(2, $"public {ClosureTypeName}(", ")", members.Length, x => $"byte[] key{x}, _C{x} arg{x}");
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
        builder.AppendIndent(2, $"public void Encode(ref _TAllocator allocator, _TSelf item)");
        builder.AppendIndent(2, $"{{");
        for (var i = 0; i < members.Length; i++)
        {
            var member = members[i];
            if (member.IsOptional is false)
            {
                builder.AppendIndent(3, $"_TAllocator.Append(ref allocator, new _TSpan(this.key{i}));");
                builder.AppendIndent(3, $"this.cvt{i}.EncodeWithLengthPrefix(ref allocator, item.{member.Name});");
                CancellationToken.ThrowIfCancellationRequested();
            }
            else
            {
                builder.AppendIndent(3, $"var loc{i} = item.{member.Name};");
                builder.AppendIndent(3, $"if ({Constants.EqualityComparerTypeName}<_T{i}>.Default.Equals(loc{i}, default) is false)");
                builder.AppendIndent(3, $"{{");
                builder.AppendIndent(4, $"_TAllocator.Append(ref allocator, this.key{i});");
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
        builder.AppendIndent(2, $"public _TSelf Decode(scoped Mikodev.Binary.Components.NamedObjectConstructorParameter parameter)");
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

        constructor.AppendCreateInstance(builder, CancellationToken);
        builder.AppendIndent(2, $"}}");
    }

    private void AppendFields(StringBuilder builder)
    {
        var members = this.members;
        for (var i = 0; i < members.Length; i++)
        {
            builder.AppendIndent(2, $"private readonly byte[] key{i};");
            builder.AppendIndent();
            builder.AppendIndent(2, $"private readonly _C{i} cvt{i};");
            builder.AppendIndent();
            CancellationToken.ThrowIfCancellationRequested(); ;
        }
    }

    private void AppendConverterCreatorBody(StringBuilder builder)
    {
        var members = this.members;
        var decoder = this.constructor is null ? "null" : "closure.Decode";

        builder.AppendIndent(3, $"var names = new string[] {{ ", $" }};", members, x => x.NamedKeyLiteral);
        builder.AppendIndent(3, $"var optional = new bool[] {{ ", $" }};", members, x => x.IsOptional ? "true" : "false");
        builder.AppendIndent(3, $"var stringConverter = (_CString)context.GetConverter(typeof(_TString));");
        for (var i = 0; i < members.Length; i++)
        {
            var member = members[i];
            AppendAssignConverter(builder, member, $"cvt{i}", $"_C{i}", $"_T{i}");
            builder.AppendIndent(3, $"var key{i} = _TAllocator.Invoke({member.NamedKeyLiteral}, stringConverter.EncodeWithLengthPrefix);");
            CancellationToken.ThrowIfCancellationRequested(); ;
        }
        builder.AppendIndent(3, $"var closure = new {ClosureTypeName}(", ");", members.Length, x => $"key{x}, cvt{x}");
        builder.AppendIndent(3, $"var converter = Mikodev.Binary.Components.NamedObject.GetNamedObjectConverter<_TSelf>(closure.Encode, {decoder}, stringConverter, names, optional);");
        builder.AppendIndent(3, $"return ({Constants.IConverterTypeName})converter;");
    }

    private void Invoke()
    {
        var builder = new StringBuilder();
        AppendFileHead(builder);

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

        AppendFileTail(builder);
        Finish(builder);
    }
}
