namespace Mikodev.Binary.SourceGeneration.Contexts;

using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

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

    private static bool IsRecordEqualityContract(SourceGeneratorContext context, ISymbol member)
    {
        return member is IPropertySymbol { Name: "EqualityContract", IsImplicitlyDeclared: true } property && context.IsType(property.Type, "System.Type");
    }

    private static bool IsEmptyUnionCaseType(SourceGeneratorContext context, ITypeSymbol unionType, ITypeSymbol caseType)
    {
        if (caseType.IsAbstract || caseType.TypeKind is TypeKind.Interface)
            return false;
        if (context.IsType(caseType, "System.ValueTuple"))
            return true;
        if (SymbolEqualityComparer.Default.Equals(unionType.ContainingAssembly, caseType.ContainingAssembly) is false)
            return false;
        if (Symbols.GetConverterType(context, caseType) is not null || Symbols.GetConverterCreatorType(context, caseType) is not null)
            return false;
        if (context.GetAttribute(caseType, Constants.NamedObjectAttributeTypeName) is not null || context.GetAttribute(caseType, Constants.TupleObjectAttributeTypeName) is not null)
            return false;
        var cancellation = context.CancellationToken;
        var members = Symbols.GetAllFieldsAndPropertiesForNonInterfaceType(caseType, cancellation);
        if (members.Any(x => x.IsStatic is false && IsRecordEqualityContract(context, x) is false))
            return false;
        if (caseType.IsValueType)
            return true;
        return caseType is INamedTypeSymbol namedType && namedType.InstanceConstructors.Any(x => x.Parameters.Length is 0 && x.DeclaredAccessibility is Accessibility.Public);
    }

    public static SourceResult? Invoke(SourceGeneratorContext context, SourceGeneratorTracker tracker, ITypeSymbol symbol)
    {
        if (symbol is not INamedTypeSymbol namedType)
            return null;
        var attributes = symbol.GetAttributes();
        var systemUnionAttribute = context.GetAttribute(symbol, "System.Runtime.CompilerServices.UnionAttribute");
        var systemUnionInterface = symbol.Interfaces.FirstOrDefault(x => context.IsType(x, "System.Runtime.CompilerServices.IUnion"));
        if (systemUnionAttribute is null && systemUnionInterface is null)
            return null;
        var caseTypeList = default(List<ITypeSymbol>);
        if (symbol.Interfaces.FirstOrDefault(x => x.Name is "IUnionMembers") is { } customUnionInterface)
            caseTypeList = [.. customUnionInterface.GetMembers().OfType<IMethodSymbol>().Where(FilterUnionCreateMethod).Select(SelectUnionCaseType).OfType<ITypeSymbol>()];
        if (caseTypeList is null or { Count: 0 })
            caseTypeList = [.. namedType.InstanceConstructors.Select(SelectUnionCaseType).OfType<ITypeSymbol>()];
        if (caseTypeList is null or { Count: 0 } || caseTypeList.Distinct(SymbolEqualityComparer.Default).Count() != caseTypeList.Count)
            return new SourceResult.Diagnostic([(Diagnostics.TypeNotRecognized, ["UnionObject", Symbols.GetSymbolDiagnosticDisplayString(symbol)])]);
        var caseList = caseTypeList.Select((x, i) => new UnionCaseInfo(i, x, IsEmptyUnionCaseType(context, symbol, x))).ToList();
        caseList.Sort((a, b) => Symbols.CompareConversion(context.Compilation, a.Type, b.Type));
        return new UnionObjectConverterContext(context, tracker, symbol, [.. caseList]).Build();
    }
}
