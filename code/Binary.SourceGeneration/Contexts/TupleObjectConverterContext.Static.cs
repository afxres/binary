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
        var attributes = member.GetAttributes();
        var attribute = attributes.FirstOrDefault(x => context.Equals(x.AttributeClass, Constants.TupleKeyAttributeTypeName));
        if (attribute is null || attribute.ConstructorArguments.FirstOrDefault().Value is not int key)
            return;
        var info = GetTupleMember(member);
        if (dictionary.ContainsKey(key))
            context.Throw(Constants.TupleKeyDuplicated, Symbols.GetLocation(attribute), new object[] { key });
        dictionary.Add(key, info);
    }

    private static void GetSystemTupleMember(ISymbol member, SortedDictionary<int, SymbolTupleMemberInfo> dictionary)
    {
        var cursor = SystemTupleMemberNames.IndexOf(member.Name);
        if (cursor is -1)
            return;
        var info = GetTupleMember(member);
        if (info is null)
            return;
        dictionary.Add(cursor, info);
    }

    private static bool IsSystemTuple(SourceGeneratorContext context, ITypeSymbol type)
    {
        if (type.IsTupleType)
            return true;
        if (type is not INamedTypeSymbol symbol || symbol.IsGenericType is false)
            return false;
        const string SystemTupleTypeKey = "System.Tuple";
        if (context.Resources.TryGetValue(SystemTupleTypeKey, out var types) is false)
        {
            types = Enumerable.Range(1, 8)
                .Select(x => context.Compilation.GetTypeByMetadataName($"System.Tuple`{x}")?.ConstructUnboundGenericType())
                .OfType<INamedTypeSymbol>()
                .ToImmutableArray();
            context.Resources.Add(SystemTupleTypeKey, types);
        }
        return (types as IEnumerable<ISymbol>)?.Any(x => SymbolEqualityComparer.Default.Equals(x, symbol.ConstructUnboundGenericType())) is true;
    }

    public static string? Invoke(SourceGeneratorContext context, ITypeSymbol symbol)
    {
        var system = IsSystemTuple(context, symbol);
        var attribute = system ? null : symbol.GetAttributes().FirstOrDefault(x => context.Equals(x.AttributeClass, Constants.TupleObjectAttributeTypeName));
        if (system is false && attribute is null)
            return null;
        var dictionary = new SortedDictionary<int, SymbolTupleMemberInfo>();
        var cancellation = context.SourceProductionContext.CancellationToken;
        foreach (var member in Symbols.GetObjectMembers(symbol))
        {
            if (system)
                GetSystemTupleMember(member, dictionary);
            else
                GetCustomTupleMember(context, member, dictionary);
            cancellation.ThrowIfCancellationRequested();
        }
        var members = dictionary.Values.ToImmutableArray();
        if (members.Length is 0)
            context.Throw(Constants.NoAvailableMemberFound, Symbols.GetLocation(symbol), new object[] { symbol.Name });
        var closure = new TupleObjectConverterContext(context, symbol, members);
        closure.Invoke();
        return closure.ConverterCreatorTypeName;
    }
}
