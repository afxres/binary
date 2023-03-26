namespace Mikodev.Binary.SourceGeneration.Contexts;

using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

public sealed partial class TupleObjectConverterContext
{
    private static readonly ImmutableArray<string> SystemTupleMemberNames = ImmutableArray.Create(new[] { "Item1", "Item2", "Item3", "Item4", "Item5", "Item6", "Item7", "Rest" });

    private static SymbolTupleMemberInfo? GetTupleMember(ISymbol member)
    {
        if (member is IFieldSymbol fieldSymbol)
            return new SymbolTupleMemberInfo(member, fieldSymbol.Type, fieldSymbol.Name, fieldSymbol.IsReadOnly);
        if (member is IPropertySymbol propertySymbol)
            return new SymbolTupleMemberInfo(member, propertySymbol.Type, propertySymbol.Name, propertySymbol.IsReadOnly);
        return null;
    }

    private static void GetCustomTupleMember(SourceGeneratorContext context, ISymbol member, SortedDictionary<int, SymbolTupleMemberInfo> dictionary)
    {
        if (member.IsStatic || member.DeclaredAccessibility is not Accessibility.Public)
            return;
        if (member is IPropertySymbol property && property.IsIndexer)
            return;
        var attributes = member.GetAttributes();
        var attribute = attributes.FirstOrDefault(x => context.Equals(x.AttributeClass, Constants.TupleKeyAttributeTypeName));
        if (attribute is null || attribute.ConstructorArguments.FirstOrDefault().Value is not int key)
            return;
        var info = GetTupleMember(member);
        if (info is null)
            return;
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
        var systemTuple = IsSystemTuple(context, symbol);
        var attribute = systemTuple ? null : symbol.GetAttributes().FirstOrDefault(x => context.Equals(x.AttributeClass, Constants.TupleObjectAttributeTypeName));
        if (systemTuple is false && attribute is null)
            return null;
        var memberDictionary = new SortedDictionary<int, SymbolTupleMemberInfo>();
        var selector = systemTuple
            ? new Action<ISymbol>(x => GetSystemTupleMember(x, memberDictionary))
            : (x => GetCustomTupleMember(context, x, memberDictionary));
        foreach (var i in symbol.GetMembers())
            selector.Invoke(i);
        var members = memberDictionary.Values.ToImmutableArray();
        if (members.Length is 0)
            context.Throw(Constants.NoAvailableMemberFound, Symbols.GetLocation(attribute), new object[] { symbol.Name });
        var closure = new TupleObjectConverterContext(context, symbol, members);
        closure.Invoke();
        return closure.ConverterCreatorTypeName;
    }
}
