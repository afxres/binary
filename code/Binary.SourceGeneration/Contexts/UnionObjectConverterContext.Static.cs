namespace Mikodev.Binary.SourceGeneration.Contexts;

using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;
using UnionCaseInfo = (int Index, Microsoft.CodeAnalysis.ITypeSymbol Type);

public sealed partial class UnionObjectConverterContext
{
    public static SourceResult? Invoke(SourceGeneratorContext context, SourceGeneratorTracker tracker, ITypeSymbol symbol)
    {
        if (symbol.Interfaces.Any(x => x.Name is "IUnion") is false || symbol is not INamedTypeSymbol namedType)
            return null;
        var caseList = new List<UnionCaseInfo>();
        foreach (var i in namedType.InstanceConstructors)
        {
            var parameters = i.Parameters;
            if (parameters.Length is not 1 || i.DeclaredAccessibility is not Accessibility.Public)
                continue;
            caseList.Add((caseList.Count, parameters.Single().Type));
        }
        if (caseList.Select(x => x.Type).Distinct(SymbolEqualityComparer.Default).Count() != caseList.Count)
            return null;
        caseList.Sort((a, b) => Symbols.CompareInheritance(context.Compilation, a.Type, b.Type));
        return new UnionObjectConverterContext(context, tracker, symbol, [.. caseList]).Invoke();
    }
}
