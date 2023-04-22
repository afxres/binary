namespace Mikodev.Binary.SourceGeneration;

using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Text;
using System.Threading;

public abstract class SymbolConverterContext
{
    protected const int SymbolFullNameKey = -1;

    protected Dictionary<int, (string TypeFullName, string ConverterTypeFullName)> FullNameResources { get; } = new Dictionary<int, (string, string)>();

    protected ITypeSymbol Symbol { get; }

    protected string SymbolTypeFullName { get; }

    protected string SymbolConverterTypeFullName { get; }

    protected CancellationToken CancellationToken => SourceGeneratorContext.CancellationToken;

    protected SourceGeneratorContext SourceGeneratorContext { get; }

    protected string ClosureTypeName { get; }

    protected string ConverterTypeName { get; }

    protected string ConverterCreatorTypeName { get; }

    protected SymbolConverterContext(SourceGeneratorContext context, ITypeSymbol symbol)
    {
        var output = Symbols.GetOutputFullName(symbol);
        AddType(SymbolFullNameKey, symbol);
        Symbol = symbol;
        SymbolTypeFullName = GetTypeFullName(SymbolFullNameKey);
        SymbolConverterTypeFullName = GetConverterTypeFullName(SymbolFullNameKey);
        SourceGeneratorContext = context;
        ClosureTypeName = $"{output}_Closure";
        ConverterTypeName = $"{output}_Converter";
        ConverterCreatorTypeName = $"{output}_ConverterCreator";
    }

    protected void AddType(int key, ITypeSymbol symbol)
    {
        var fullName = Symbols.GetSymbolFullName(symbol);
        var converter = $"{Constants.GlobalNamespace}{Constants.ConverterTypeName}<{fullName}>";
        FullNameResources.Add(key, (fullName, converter));
    }

    protected string GetTypeFullName(int key)
    {
        return FullNameResources[key].TypeFullName;
    }

    protected string GetConverterTypeFullName(int key)
    {
        return FullNameResources[key].ConverterTypeFullName;
    }

    protected void AppendClosureHead(StringBuilder builder)
    {
        builder.AppendIndent(1, $"private sealed class {ClosureTypeName}");
        builder.AppendIndent(1, $"{{");
    }

    protected void AppendClosureTail(StringBuilder builder)
    {
        builder.AppendIndent(1, $"}}");
    }

    protected void AppendConverterHead(StringBuilder builder)
    {
        builder.AppendIndent(1, $"private sealed class {ConverterTypeName} : {GetConverterTypeFullName(-1)}");
        builder.AppendIndent(1, $"{{");
    }

    protected void AppendConverterTail(StringBuilder builder)
    {
        builder.AppendIndent(1, $"}}");
    }

    protected void AppendConverterCreatorHead(StringBuilder builder)
    {
        builder.AppendIndent(1, $"private sealed class {ConverterCreatorTypeName} : {Constants.IConverterCreatorTypeName}");
        builder.AppendIndent(1, $"{{");

        builder.AppendIndent(2, $"public {Constants.IConverterTypeName} GetConverter({Constants.IGeneratorContextTypeName} context, System.Type type)");
        builder.AppendIndent(2, $"{{");
        builder.AppendIndent(3, $"if (type != typeof({GetTypeFullName(-1)}))");
        builder.AppendIndent(4, $"return null;");
    }

    protected void AppendConverterCreatorTail(StringBuilder builder)
    {
        builder.AppendIndent(2, $"}}");
        builder.AppendIndent(1, $"}}");
    }

    protected void AppendAssignConverterExplicitConverter(StringBuilder builder, ITypeSymbol converter, string variableName, string converterTypeAlias)
    {
        var fullName = Symbols.GetSymbolFullName(converter);
        builder.AppendIndent(3, $"var {variableName} = ({converterTypeAlias})(new {fullName}());");
        CancellationToken.ThrowIfCancellationRequested();
    }

    protected void AppendAssignConverterExplicitConverterCreator(StringBuilder builder, ITypeSymbol creator, string variableName, string converterTypeAlias, string memberTypeAlias)
    {
        var fullName = Symbols.GetSymbolFullName(creator);
        builder.AppendIndent(3, $"var {variableName} = ({converterTypeAlias})((({Constants.IConverterCreatorTypeName})(new {fullName}())).GetConverter(context, typeof({memberTypeAlias})));");
        CancellationToken.ThrowIfCancellationRequested();
    }

    protected void AppendAssignConverterExplicit(StringBuilder builder, ITypeSymbol member, string variableName, string converterTypeAlias, string memberTypeAlias)
    {
        builder.AppendIndent(3, $"var {variableName} = ({converterTypeAlias})context.GetConverter(typeof({memberTypeAlias}));");
        SourceGeneratorContext.AddReferencedType(member);
        CancellationToken.ThrowIfCancellationRequested();
    }

    protected void AppendAssignConverter(StringBuilder builder, SymbolMemberInfo member, string variableName, string converterTypeAlias, string memberTypeAlias)
    {
        if (Symbols.GetConverterType(SourceGeneratorContext, member.Symbol) is { } converter)
            AppendAssignConverterExplicitConverter(builder, converter, variableName, converterTypeAlias);
        else if (Symbols.GetConverterCreatorType(SourceGeneratorContext, member.Symbol) is { } creator)
            AppendAssignConverterExplicitConverterCreator(builder, creator, variableName, converterTypeAlias, memberTypeAlias);
        else
            AppendAssignConverterExplicit(builder, member.TypeSymbol, variableName, converterTypeAlias, memberTypeAlias);
    }

    protected abstract void Invoke(StringBuilder builder);

    protected SymbolConverterContent Invoke()
    {
        var builder = new StringBuilder();
        Invoke(builder);
        var code = builder.ToString();
        return new SymbolConverterContent(ConverterCreatorTypeName, code);
    }
}
