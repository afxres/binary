namespace Mikodev.Binary.SourceGeneration;

using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Text;
using System.Threading;

public abstract class SymbolConverterContext
{
    private readonly SourceGeneratorContext context;

    private readonly Dictionary<int, (string TypeFullName, string ConverterTypeFullName)> fullNameCache;

    protected ITypeSymbol Symbol { get; }

    protected string SymbolTypeFullName { get; }

    protected string SymbolConverterTypeFullName { get; }

    protected CancellationToken CancellationToken => this.context.CancellationToken;

    protected string ClosureTypeName { get; }

    protected string ConverterTypeName { get; }

    protected string ConverterCreatorTypeName { get; }

    protected SymbolConverterContext(SourceGeneratorContext context, ITypeSymbol symbol)
    {
        var output = Symbols.GetOutputFullName(symbol);
        this.context = context;
        this.fullNameCache = new Dictionary<int, (string, string)>();
        Symbol = symbol;
        SymbolTypeFullName = context.GetTypeFullName(symbol);
        SymbolConverterTypeFullName = context.GetConverterTypeFullName(symbol);
        ClosureTypeName = $"{output}_Closure";
        ConverterTypeName = $"{output}_Converter";
        ConverterCreatorTypeName = $"{output}_ConverterCreator";
    }

    protected void AddType(int key, ITypeSymbol symbol)
    {
        var fullName = this.context.GetTypeFullName(symbol);
        var converter = this.context.GetConverterTypeFullName(symbol);
        this.fullNameCache.Add(key, (fullName, converter));
    }

    protected string GetTypeFullName(int key)
    {
        return this.fullNameCache[key].TypeFullName;
    }

    protected string GetConverterTypeFullName(int key)
    {
        return this.fullNameCache[key].ConverterTypeFullName;
    }

    protected void AppendConverterCreatorHead(StringBuilder builder)
    {
        builder.AppendIndent(1, $"private sealed class {ConverterCreatorTypeName} : {Constants.IConverterCreatorTypeName}");
        builder.AppendIndent(1, $"{{");

        builder.AppendIndent(2, $"public {Constants.IConverterTypeName} GetConverter({Constants.IGeneratorContextTypeName} context, System.Type type)");
        builder.AppendIndent(2, $"{{");
        builder.AppendIndent(3, $"if (type != typeof({SymbolTypeFullName}))");
        builder.AppendIndent(4, $"return null;");
    }

    protected void AppendConverterCreatorTail(StringBuilder builder)
    {
        builder.AppendIndent(2, $"}}");
        builder.AppendIndent(1, $"}}");
    }

    protected void AppendAssignConverterExplicitConverter(StringBuilder builder, ITypeSymbol converter, string variableName, string converterTypeAlias)
    {
        var fullName = this.context.GetTypeFullName(converter);
        builder.AppendIndent(3, $"var {variableName} = ({converterTypeAlias})(new {fullName}());");
        CancellationToken.ThrowIfCancellationRequested();
    }

    protected void AppendAssignConverterExplicitConverterCreator(StringBuilder builder, ITypeSymbol creator, string variableName, string converterTypeAlias, string memberTypeAlias)
    {
        var fullName = this.context.GetTypeFullName(creator);
        builder.AppendIndent(3, $"var {variableName} = ({converterTypeAlias})((({Constants.IConverterCreatorTypeName})(new {fullName}())).GetConverter(context, typeof({memberTypeAlias})));");
        CancellationToken.ThrowIfCancellationRequested();
    }

    protected void AppendAssignConverterExplicit(StringBuilder builder, ITypeSymbol member, string variableName, string converterTypeAlias, string memberTypeAlias)
    {
        builder.AppendIndent(3, $"var {variableName} = ({converterTypeAlias})context.GetConverter(typeof({memberTypeAlias}));");
        this.context.AddReferencedType(member);
        CancellationToken.ThrowIfCancellationRequested();
    }

    protected void AppendAssignConverter(StringBuilder builder, SymbolMemberInfo member, string variableName, string converterTypeAlias, string memberTypeAlias)
    {
        if (Symbols.GetConverterType(this.context, member.Symbol) is { } converter)
            AppendAssignConverterExplicitConverter(builder, converter, variableName, converterTypeAlias);
        else if (Symbols.GetConverterCreatorType(this.context, member.Symbol) is { } creator)
            AppendAssignConverterExplicitConverterCreator(builder, creator, variableName, converterTypeAlias, memberTypeAlias);
        else
            AppendAssignConverterExplicit(builder, member.Type, variableName, converterTypeAlias, memberTypeAlias);
    }

    protected abstract void Invoke(StringBuilder builder);

    protected SymbolConverterContent Invoke()
    {
        var builder = new StringBuilder();
        Invoke(builder);
        return new SymbolConverterContent(ConverterCreatorTypeName, builder.ToString());
    }
}
