namespace Mikodev.Binary.SourceGeneration.Contexts;

using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

public sealed partial class NamedObjectConverterContext
{
    private static SymbolNamedMemberInfo? GetNamedMember(ISymbol member, string literal, bool isTypeRequired)
    {
        if (member is IFieldSymbol field)
            return new SymbolNamedMemberInfo(member, field.Type, field.Name, field.IsReadOnly, literal, isTypeRequired && (field.IsRequired is false));
        if (member is IPropertySymbol property)
            return new SymbolNamedMemberInfo(member, property.Type, property.Name, property.IsReadOnly, literal, isTypeRequired && (property.IsRequired is false));
        return null;
    }

    private static void GetNamedMember(SourceGeneratorContext context, ISymbol member, bool isTypeRequired, SortedDictionary<string, SymbolNamedMemberInfo> dictionary)
    {
        Symbols.ValidateMemberAttributes(context, member);
        if (member.IsStatic || member.DeclaredAccessibility is not Accessibility.Public)
            return;
        if (member is IPropertySymbol property && property.IsIndexer)
            return;
        var attributes = member.GetAttributes();
        var attribute = attributes.FirstOrDefault(x => context.Equals(x.AttributeClass, Constants.NamedKeyAttributeTypeName));
        if (attribute is null)
            return;
        var key = attribute.ConstructorArguments.FirstOrDefault().Value as string ?? string.Empty;
        if (string.IsNullOrEmpty(key))
            context.Throw(Constants.NamedKeyNullOrEmpty, Symbols.GetLocation(attribute), null);
        var info = GetNamedMember(member, Symbols.ToLiteral(key), isTypeRequired);
        if (info is null)
            return;
        if (dictionary.ContainsKey(key))
            context.Throw(Constants.NamedKeyDuplicated, Symbols.GetLocation(attribute), new object[] { key });
        dictionary.Add(key, info);
    }

    private static bool IsRequired(ISymbol member) => member switch
    {
        IFieldSymbol field => field.IsRequired,
        IPropertySymbol property => property.IsRequired,
        _ => false,
    };

    public static string? Invoke(SourceGeneratorContext context, ITypeSymbol symbol)
    {
        if (symbol.GetAttributes().FirstOrDefault(x => context.Equals(x.AttributeClass, Constants.NamedObjectAttributeTypeName)) is not { } attribute)
            return null;

        var required = symbol.GetMembers().Any(IsRequired);
        var memberDictionary = new SortedDictionary<string, SymbolNamedMemberInfo>();
        foreach (var i in symbol.GetMembers())
            GetNamedMember(context, i, required, memberDictionary);
        var members = memberDictionary.Values.ToImmutableArray();
        // let compiler report it if required member not set (linq expression generator will report if required member not set)
        if (members.Length is 0)
            context.Throw(Constants.NoAvailableMemberFound, Symbols.GetLocation(attribute), new object[] { symbol.Name });
        var closure = new NamedObjectConverterContext(context, symbol, members);
        closure.Invoke();
        return closure.ConverterCreatorTypeName;
    }
}
