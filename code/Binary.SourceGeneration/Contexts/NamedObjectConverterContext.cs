namespace Mikodev.Binary.SourceGeneration.Contexts;

using Microsoft.CodeAnalysis;
using Mikodev.Binary.SourceGeneration;
using Mikodev.Binary.SourceGeneration.Internal;
using System.Collections.Immutable;

public sealed partial class NamedObjectConverterContext : SymbolConverterContext
{
    private readonly ImmutableArray<SymbolNamedMemberInfo> members;

    private readonly SymbolConstructorInfo<SymbolNamedMemberInfo>? constructor;

    private NamedObjectConverterContext(SourceGeneratorContext context, SourceGeneratorTracker tracker, ITypeSymbol symbol, ImmutableArray<SymbolNamedMemberInfo> members, SymbolConstructorInfo<SymbolNamedMemberInfo>? constructor) : base(context, tracker, symbol)
    {
        members.AsSpan().ForEach((index, value) => AddType(index, value.Type));
        this.members = members;
        this.constructor = constructor;
    }

    private void AppendConverterHead()
    {
        var members = this.members;
        var tail = ", byte[][] keys, Mikodev.Binary.Converter<string> converter, System.Collections.Generic.IEnumerable<string> names, System.Collections.Generic.IEnumerable<bool> optional)";
        Output.AppendIndent(1, $"private sealed class {OutputConverterTypeName}(", tail, members.Length, i => $"{GetConverterTypeFullName(i)} cvt{i}");
        Output.AppendIndent(2, $": Mikodev.Binary.Components.NamedObjectConverter<{SymbolTypeFullName}>(converter, names, optional)");
        Output.AppendIndent(1, $"{{");
        CancellationToken.ThrowIfCancellationRequested();
    }

    private void AppendConverterTail()
    {
        Output.AppendIndent(1, $"}}");
        Output.AppendIndent();
    }

    private void AppendEnsureContext()
    {
        if (Symbol.IsValueType)
            return;
        Output.AppendIndent(3, $"if (item is null)");
        Output.AppendIndent(4, $"return;");
        CancellationToken.ThrowIfCancellationRequested();
    }

    private void AppendEncodeMethod()
    {
        var members = this.members;
        Output.AppendIndent(2, $"public override void Encode(ref Mikodev.Binary.Allocator allocator, {SymbolTypeFullName} item)");
        Output.AppendIndent(2, $"{{");
        AppendEnsureContext();
        for (var i = 0; i < members.Length; i++)
        {
            if (members[i].IsOptional is false)
            {
                Output.AppendIndent(3, $"Mikodev.Binary.Allocator.Append(ref allocator, new System.ReadOnlySpan<byte>(keys[{i}]));");
                Output.AppendIndent(3, $"cvt{i}.EncodeWithLengthPrefix(ref allocator, item.{members[i].NameInSourceCode});");
            }
            else
            {
                Output.AppendIndent(3, $"var var{i} = item.{members[i].NameInSourceCode};");
                Output.AppendIndent(3, $"if (System.Collections.Generic.EqualityComparer<{GetTypeFullName(i)}>.Default.Equals(var{i}, default) is false)");
                Output.AppendIndent(3, $"{{");
                Output.AppendIndent(4, $"Mikodev.Binary.Allocator.Append(ref allocator, new System.ReadOnlySpan<byte>(keys[{i}]));");
                Output.AppendIndent(4, $"cvt{i}.EncodeWithLengthPrefix(ref allocator, var{i});");
                Output.AppendIndent(3, $"}}");
            }
            CancellationToken.ThrowIfCancellationRequested();
        }
        Output.AppendIndent(2, $"}}");
    }

    private void AppendDecodeMethod()
    {
        var members = this.members;
        var constructor = this.constructor;
        Output.AppendIndent();
        Output.AppendIndent(2, $"public override {SymbolTypeFullName} Decode(scoped Mikodev.Binary.Components.NamedObjectParameter parameter)");
        Output.AppendIndent(2, $"{{");
        if (constructor is null)
        {
            Output.AppendIndent(3, $"throw new System.NotSupportedException($\"No suitable constructor found, type: {{typeof({SymbolTypeFullName})}}\");");
        }
        else
        {
            for (var i = 0; i < members.Length; i++)
            {
                if (members[i].IsOptional is false)
                    Output.AppendIndent(3, $"var var{i} = cvt{i}.Decode(parameter.GetValue({i}));");
                else
                    Output.AppendIndent(3, $"var var{i} = parameter.HasValue({i}) ? cvt{i}.Decode(parameter.GetValue({i})) : default;");
                CancellationToken.ThrowIfCancellationRequested();
            }
            constructor.AppendTo(Output, SymbolTypeFullName, CancellationToken);
        }
        Output.AppendIndent(2, $"}}");
    }

    private void AppendConverterCreatorBody()
    {
        var members = this.members;
        Output.AppendIndent(3, $"var names = new string[] {{ ", $" }};", members.Length, x => members[x].NamedKeyLiteral);
        Output.AppendIndent(3, $"var optional = new bool[] {{ ", $" }};", members.Length, x => members[x].IsOptional ? "true" : "false");
        Output.AppendIndent(3, $"var encoding = Mikodev.Binary.GeneratorContextExtensions.GetConverter<string>(context);");
        for (var i = 0; i < members.Length; i++)
        {
            var member = members[i];
            AppendAssignConverter(member, $"cvt{i}", GetConverterTypeFullName(i), GetTypeFullName(i));
            CancellationToken.ThrowIfCancellationRequested();
        }
        Output.AppendIndent(3, $"var keys = System.Array.ConvertAll(names, x => Mikodev.Binary.Allocator.Invoke(x, encoding.EncodeWithLengthPrefix));");
        Output.AppendIndent(3, $"var converter = new {OutputConverterTypeName}(", ", keys, encoding, names, optional);", members.Length, x => $"cvt{x}");
    }

    protected override void Handle()
    {
        AppendConverterHead();
        AppendEncodeMethod();
        AppendDecodeMethod();
        AppendConverterTail();

        AppendConverterCreatorHead();
        AppendConverterCreatorBody();
        AppendConverterCreatorTail();
    }
}
