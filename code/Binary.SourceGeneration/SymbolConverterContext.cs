namespace Mikodev.Binary.SourceGeneration;

using Microsoft.CodeAnalysis;
using System.IO;
using System.Text;
using System.Threading;

public abstract class SymbolConverterContext
{
    protected ITypeSymbol TypeSymbol { get; }

    protected SourceGeneratorContext SourceGeneratorContext { get; }

    protected SymbolTypeAliases TypeAliases { get; }

    protected CancellationToken CancellationToken { get; }

    protected string HintNameUnit { get; }

    protected string ClosureTypeName { get; }

    protected string ConverterTypeName { get; }

    protected string ConverterCreatorTypeName { get; }

    protected SymbolConverterContext(SourceGeneratorContext context, ITypeSymbol symbol)
    {
        var aliases = new SymbolTypeAliases();
        var output = Symbols.GetOutputFullName(symbol);
        aliases.Add(symbol, "Self");
        TypeSymbol = symbol;
        SourceGeneratorContext = context;
        CancellationToken = context.SourceProductionContext.CancellationToken;
        TypeAliases = aliases;
        HintNameUnit = output;
        ClosureTypeName = $"{output}Closure";
        ConverterTypeName = $"{output}Converter";
        ConverterCreatorTypeName = $"{output}ConverterCreator";
    }

    protected void AppendFileHead(StringBuilder builder)
    {
        builder.AppendIndent(0, $"namespace {SourceGeneratorContext.Namespace};");
        builder.AppendIndent();
        TypeAliases.AppendAliases(builder);

        builder.AppendIndent();
        builder.AppendIndent(0, $"partial class {SourceGeneratorContext.Name}");
        builder.AppendIndent(0, $"{{");
    }

    protected void AppendFileTail(StringBuilder builder)
    {
        builder.AppendIndent(0, $"}}");
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
        builder.AppendIndent(1, $"private sealed class {ConverterTypeName} : _CSelf");
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
        builder.AppendIndent(3, $"if (type != typeof(_TSelf))");
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

    protected void Finish(StringBuilder builder)
    {
        var code = builder.ToString();
        var file = Path.Combine(SourceGeneratorContext.HintNameUnit, $"{HintNameUnit}.g.cs");
        SourceGeneratorContext.SourceProductionContext.AddSource(file, code);
    }
}
