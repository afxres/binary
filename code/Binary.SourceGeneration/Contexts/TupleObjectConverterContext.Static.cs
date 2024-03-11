namespace Mikodev.Binary.SourceGeneration.Contexts;

using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

public sealed partial class TupleObjectConverterContext
{
    private static readonly ImmutableArray<string> SystemTupleMemberNames = ImmutableArray.Create(["Item1", "Item2", "Item3", "Item4", "Item5", "Item6", "Item7", "Rest"]);

    private static int GetCustomTupleKey(SourceGeneratorContext context, ISymbol member)
    {
        return context.GetAttribute(member, Constants.TupleKeyAttributeTypeName)?.ConstructorArguments.Single().Value as int? ?? -1;
    }

    private static ImmutableHashSet<INamedTypeSymbol> CreateResource(Compilation compilation)
    {
        return Enumerable.Range(1, 8)
            .Select(x => compilation.GetTypeByMetadataName($"System.Tuple`{x}")?.ConstructUnboundGenericType())
            .OfType<INamedTypeSymbol>()
            .ToImmutableHashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
    }

    private static bool IsSystemTuple(SourceGeneratorContext context, ITypeSymbol type)
    {
        if (type.IsTupleType)
            return true;
        if (type is not INamedTypeSymbol symbol || symbol.IsGenericType is false)
            return false;
        const string ResourceKey = "Tuple";
        var types = (ImmutableHashSet<INamedTypeSymbol>)context.GetOrCreateResource(ResourceKey, CreateResource);
        var unbound = symbol.ConstructUnboundGenericType();
        return types.Contains(unbound);
    }

    public static SourceResult? Invoke(SourceGeneratorContext context, SourceGeneratorTracker tracker, ITypeSymbol symbol)
    {
        var system = IsSystemTuple(context, symbol);
        var attribute = system ? null : context.GetAttribute(symbol, Constants.TupleObjectAttributeTypeName);
        if (system is false && attribute is null)
            return null;
        var typeInfo = context.GetTypeInfo(symbol);
        var dictionary = new SortedDictionary<int, SymbolTupleMemberInfo>();
        var cancellation = context.CancellationToken;
        foreach (var member in typeInfo.FilteredFieldsAndProperties)
        {
            var key = system
                ? SystemTupleMemberNames.IndexOf(member.Name)
                : GetCustomTupleKey(context, member);
            if (key is -1)
                continue;
            var memberInfo = new SymbolTupleMemberInfo(member);
            dictionary.Add(key, memberInfo);
            cancellation.ThrowIfCancellationRequested();
        }
        var members = dictionary.Values.ToImmutableArray();
        if (members.Length is 0)
            return new SourceResult(SourceStatus.NoAvailableMember);
        var constructor = Symbols.GetConstructor(context, typeInfo, members);
        return new TupleObjectConverterContext(context, tracker, symbol, members, constructor).Invoke();
    }
}
