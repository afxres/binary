namespace Mikodev.Binary.SourceGeneration.Contexts;

using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;
using UnionCaseInfo = (int Index, Microsoft.CodeAnalysis.ITypeSymbol Type);

public sealed partial class UnionObjectConverterContext
{
    private static bool FilterUnionCreateMethod(IMethodSymbol m)
    {
        return m.Name is "Create" && m.IsStatic && m.Parameters.Length is 1;
    }

    private static ITypeSymbol? SelectUnionCaseType(IMethodSymbol i)
    {
        var parameters = i.Parameters;
        if (parameters.Length is not 1 || i.DeclaredAccessibility is not Accessibility.Public)
            return null;
        var parameter = i.Parameters.Single();
        if (parameter.RefKind is not RefKind.None and not RefKind.In)
            return null;
        return parameter.Type;
    }

    public static SourceResult? Invoke(SourceGeneratorContext context, SourceGeneratorTracker tracker, ITypeSymbol symbol)
    {
        if (symbol is not INamedTypeSymbol namedType)
            return null;
        var attributes = symbol.GetAttributes();
        var systemUnionAttribute = context.GetAttribute(symbol, "System.Runtime.CompilerServices.UnionAttribute");
        var systemUnionInterface = symbol.Interfaces.FirstOrDefault(x => context.Equals(x, "System.Runtime.CompilerServices.IUnion"));
        if (systemUnionAttribute is null && systemUnionInterface is null)
            return null;
        var caseTypeList = default(List<ITypeSymbol>);
        if (symbol.Interfaces.FirstOrDefault(x => x.Name is "IUnionMembers") is { } customUnionInterface)
            caseTypeList = [.. customUnionInterface.GetMembers().OfType<IMethodSymbol>().Where(FilterUnionCreateMethod).Select(SelectUnionCaseType).OfType<ITypeSymbol>()];
        if (caseTypeList is null or { Count: 0 })
            caseTypeList = [.. namedType.InstanceConstructors.Select(SelectUnionCaseType).OfType<ITypeSymbol>()];
        if (caseTypeList is null or { Count: 0 } || caseTypeList.Distinct(SymbolEqualityComparer.Default).Count() != caseTypeList.Count)
            return new SourceResultWithDiagnostic([(Constants.TypeNotRecognized, ["UnionObject", Symbols.GetSymbolDiagnosticDisplayString(symbol)])]);
        var caseList = caseTypeList.Select((x, i) => new UnionCaseInfo(i, x)).ToList();
        caseList.Sort((a, b) => Symbols.CompareConversion(context.Compilation, a.Type, b.Type));
        return new UnionObjectConverterContext(context, tracker, symbol, [.. caseList]).Invoke();
    }
}
