namespace Mikodev.Binary.SourceGeneration.Contexts;

using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

public sealed partial class NamedObjectConverterContext
{
    private static SymbolNamedMemberInfo GetNamedMember(ISymbol member, string literal, bool isTypeRequired)
    {
        if (member is IFieldSymbol field)
            return new SymbolNamedMemberInfo(field, literal, isTypeRequired && (field.IsRequired is false));
        else
            return new SymbolNamedMemberInfo((IPropertySymbol)member, literal, isTypeRequired && (((IPropertySymbol)member).IsRequired is false));
    }

    private static void GetCustomNamedMember(SourceGeneratorContext context, ISymbol member, bool isTypeRequired, SortedDictionary<string, SymbolNamedMemberInfo> dictionary)
    {
        var attributes = member.GetAttributes();
        var attribute = attributes.FirstOrDefault(x => context.Equals(x.AttributeClass, Constants.NamedKeyAttributeTypeName));
        if (attribute is null)
            return;
        var key = (string)attribute.ConstructorArguments.Single().Value!;
        var info = GetNamedMember(member, Symbols.ToLiteral(key), isTypeRequired);
        dictionary.Add(key, info);
    }

    private static void GetSimpleNamedMember(ISymbol member, bool isTypeRequired, SortedDictionary<string, SymbolNamedMemberInfo> dictionary)
    {
        var key = member.Name;
        var info = GetNamedMember(member, Symbols.ToLiteral(key), isTypeRequired);
        dictionary.Add(key, info);
    }

    public static object? Invoke(SourceGeneratorContext context, ITypeSymbol symbol)
    {
        var attribute = symbol.GetAttributes().FirstOrDefault(x => context.Equals(x.AttributeClass, Constants.NamedObjectAttributeTypeName));
        if (attribute is null && Symbols.IsIgnoredType(context, symbol))
            return null;
        var required = Symbols.IsTypeWithRequiredModifier(symbol);
        var dictionary = new SortedDictionary<string, SymbolNamedMemberInfo>();
        var cancellation = context.SourceProductionContext.CancellationToken;
        foreach (var member in Symbols.GetObjectMembers(symbol))
        {
            if (attribute is null)
                GetSimpleNamedMember(member, required, dictionary);
            else
                GetCustomNamedMember(context, member, required, dictionary);
            cancellation.ThrowIfCancellationRequested();
        }
        var members = dictionary.Values.ToImmutableArray();
        // do not report error for plain object
        // let compiler report it if required member not set (linq expression generator will report if required member not set)
        if (members.Length is 0)
            return attribute is null ? null : (object)Diagnostic.Create(Constants.NoAvailableMemberFound, Symbols.GetLocation(attribute), new object[] { symbol.Name });
        var constructor = Symbols.GetConstructor(symbol, members);
        var closure = new NamedObjectConverterContext(context, symbol, members, constructor);
        closure.Invoke();
        return closure.ConverterCreatorTypeName;
    }
}
