namespace Mikodev.Binary.SourceGeneration.Contexts;

using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

public sealed partial class TupleObjectConverterContext
{
    private static readonly ImmutableArray<string> SystemTupleMemberNames = ImmutableArray.Create(new[] { "Item1", "Item2", "Item3", "Item4", "Item5", "Item6", "Item7", "Rest" });

    private static SymbolTupleMemberInfo GetTupleMember(ISymbol member)
    {
        if (member is IFieldSymbol field)
            return new SymbolTupleMemberInfo(field);
        else
            return new SymbolTupleMemberInfo((IPropertySymbol)member);
    }

    private static void GetCustomTupleMember(SourceGeneratorContext context, ISymbol member, SortedDictionary<int, SymbolTupleMemberInfo> dictionary)
    {
        var attribute = context.GetAttribute(member, Constants.TupleKeyAttributeTypeName);
        if (attribute is null)
            return;
        var key = (int)attribute.ConstructorArguments.Single().Value!;
        var info = GetTupleMember(member);
        dictionary.Add(key, info);
    }

    private static void GetSystemTupleMember(ISymbol member, SortedDictionary<int, SymbolTupleMemberInfo> dictionary)
    {
        var key = SystemTupleMemberNames.IndexOf(member.Name);
        if (key is -1)
            return;
        var info = GetTupleMember(member);
        dictionary.Add(key, info);
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

    public static object? Invoke(SourceGeneratorContext context, ITypeSymbol symbol)
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
            if (system)
                GetSystemTupleMember(member, dictionary);
            else
                GetCustomTupleMember(context, member, dictionary);
            cancellation.ThrowIfCancellationRequested();
        }
        var members = dictionary.Values.ToImmutableArray();
        if (members.Length is 0)
            return Diagnostic.Create(Constants.NoAvailableMemberFound, Symbols.GetLocation(attribute), new object[] { Symbols.GetSymbolDiagnosticDisplayString(symbol) });
        var constructor = Symbols.GetConstructor(context, typeInfo, members);
        return new TupleObjectConverterContext(context, symbol, members, constructor).Invoke();
    }
}
