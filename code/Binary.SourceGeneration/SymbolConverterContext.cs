namespace Mikodev.Binary.SourceGeneration;

using Microsoft.CodeAnalysis;
using Mikodev.Binary.SourceGeneration.Internal;
using System.Collections.Generic;
using System.Text;
using System.Threading;

public abstract class SymbolConverterContext
{
    private readonly SourceGeneratorContext context;

    private readonly SourceGeneratorTracker tracker;

    private readonly Dictionary<int, (string TypeFullName, string ConverterTypeFullName)> fullNameCache;

    protected ITypeSymbol Symbol { get; }

    protected string SymbolTypeFullName { get; }

    protected string SymbolConverterTypeFullName { get; }

    protected CancellationToken CancellationToken => this.context.CancellationToken;

    protected StringBuilder Output { get; } = new StringBuilder();

    protected string OutputConverterTypeName { get; }

    protected string OutputConverterCreatorTypeName { get; }

    protected SymbolConverterContext(SourceGeneratorContext context, SourceGeneratorTracker tracker, ITypeSymbol symbol)
    {
        var output = Symbols.GetOutputFullName(symbol);
        this.context = context;
        this.tracker = tracker;
        this.fullNameCache = [];
        Symbol = symbol;
        SymbolTypeFullName = context.GetTypeFullName(symbol);
        SymbolConverterTypeFullName = context.GetConverterTypeFullName(symbol);
        OutputConverterTypeName = $"{output}_Converter";
        OutputConverterCreatorTypeName = $"{output}_ConverterCreator";
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

    protected void AppendConverterCreatorHead()
    {
        Output.AppendIndent(1, $"private sealed class {OutputConverterCreatorTypeName} : {Constants.IConverterCreatorTypeName}");
        Output.AppendIndent(1, $"{{");

        Output.AppendIndent(2, $"public {Constants.IConverterTypeName} GetConverter(Mikodev.Binary.IGeneratorContext context, System.Type type)");
        Output.AppendIndent(2, $"{{");
        Output.AppendIndent(3, $"if (type != typeof({SymbolTypeFullName}))");
        Output.AppendIndent(4, $"return null;");
    }

    protected void AppendConverterCreatorTail()
    {
        Output.AppendIndent(3, $"return ({Constants.IConverterTypeName})converter;");
        Output.AppendIndent(2, $"}}");
        Output.AppendIndent(1, $"}}");
    }

    protected void AppendAssignConverterExplicitConverter(ITypeSymbol converter, string variableName, string converterTypeAlias)
    {
        var fullName = this.context.GetTypeFullName(converter);
        Output.AppendIndent(3, $"var {variableName} = ({converterTypeAlias})(new {fullName}());");
        CancellationToken.ThrowIfCancellationRequested();
    }

    protected void AppendAssignConverterExplicitConverterCreator(ITypeSymbol creator, string variableName, string converterTypeAlias, string memberTypeAlias)
    {
        var fullName = this.context.GetTypeFullName(creator);
        Output.AppendIndent(3, $"var {variableName} = ({converterTypeAlias})((({Constants.IConverterCreatorTypeName})(new {fullName}())).GetConverter(context, typeof({memberTypeAlias})));");
        CancellationToken.ThrowIfCancellationRequested();
    }

    protected void AppendAssignConverterExplicit(ITypeSymbol member, string variableName, string memberTypeAlias)
    {
        Output.AppendIndent(3, $"var {variableName} = Mikodev.Binary.GeneratorContextExtensions.GetConverter<{memberTypeAlias}>(context);");
        this.tracker.Invoke(member);
        CancellationToken.ThrowIfCancellationRequested();
    }

    protected void AppendAssignConverter(SymbolMemberInfo member, string variableName, string converterTypeAlias, string memberTypeAlias, bool allowsSelfTypeReference)
    {
        if (Symbols.GetConverterType(this.context, member.Symbol) is { } converter)
            AppendAssignConverterExplicitConverter(converter, variableName, converterTypeAlias);
        else if (Symbols.GetConverterCreatorType(this.context, member.Symbol) is { } creator)
            AppendAssignConverterExplicitConverterCreator(creator, variableName, converterTypeAlias, memberTypeAlias);
        else if (allowsSelfTypeReference && SymbolEqualityComparer.Default.Equals(member.Type, Symbol))
            Output.AppendIndent(3, $"var {variableName} = converter;");
        else
            AppendAssignConverterExplicit(member.Type, variableName, memberTypeAlias);
    }

    protected abstract void Handle();

    protected SourceResult Invoke()
    {
        Handle();
        var code = Output.ToString();
        var name = OutputConverterCreatorTypeName;
        return new SourceResult(name, code);
    }
}
