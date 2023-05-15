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
        var attribute = context.GetAttribute(member, Constants.NamedKeyAttributeTypeName);
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
        var attribute = context.GetAttribute(symbol, Constants.NamedObjectAttributeTypeName);
        if (attribute is null && Symbols.IsTypeIgnored(context, symbol))
            return null;
        var typeInfo = context.GetTypeInfo(symbol);
        var required = typeInfo.RequiredFieldsAndProperties.Count is not 0;
        var dictionary = new SortedDictionary<string, SymbolNamedMemberInfo>();
        var cancellation = context.CancellationToken;
        foreach (var member in typeInfo.FilteredFieldsAndProperties)
        {
            if (attribute is null)
                GetSimpleNamedMember(member, required, dictionary);
            else
                GetCustomNamedMember(context, member, required, dictionary);
            cancellation.ThrowIfCancellationRequested();
        }

        // do not report error for plain object
        var members = dictionary.Values.ToImmutableArray();
        if (members.Length is 0 && attribute is null)
            return null;
        if (members.Length is 0 && attribute is not null)
            return Diagnostic.Create(Constants.NoAvailableMemberFound, Symbols.GetLocation(attribute), new object[] { Symbols.GetSymbolDiagnosticDisplayString(symbol) });
        var constructor = Symbols.GetConstructor(context, typeInfo, members);
        return new NamedObjectConverterContext(context, symbol, members, constructor).Invoke();
    }
}
