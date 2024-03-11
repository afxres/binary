namespace Mikodev.Binary.SourceGeneration.Contexts;

using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

public sealed partial class NamedObjectConverterContext
{
    private static string? GetCustomNamedKey(SourceGeneratorContext context, ISymbol member)
    {
        return context.GetAttribute(member, Constants.NamedKeyAttributeTypeName)?.ConstructorArguments.Single().Value as string;
    }

    public static SourceResult Invoke(SourceGeneratorContext context, SourceGeneratorTracker tracker, ITypeSymbol symbol)
    {
        var attribute = context.GetAttribute(symbol, Constants.NamedObjectAttributeTypeName);
        if (attribute is null && Symbols.IsTypeIgnored(context, symbol))
            return new SourceResult(SourceStatus.Skip);
        var typeInfo = context.GetTypeInfo(symbol);
        var required = typeInfo.RequiredFieldsAndProperties.Count is not 0;
        var dictionary = new SortedDictionary<string, SymbolNamedMemberInfo>();
        var cancellation = context.CancellationToken;
        foreach (var member in typeInfo.FilteredFieldsAndProperties)
        {
            var key = attribute is null
                ? member.Name
                : GetCustomNamedKey(context, member);
            if (key is null)
                continue;
            var literal = Symbols.ToLiteral(key);
            var optional = required && (Symbols.IsRequiredFieldOrProperty(member) is false);
            var memberInfo = new SymbolNamedMemberInfo(member, literal, optional);
            dictionary.Add(key, memberInfo);
            cancellation.ThrowIfCancellationRequested();
        }

        var members = dictionary.Values.ToImmutableArray();
        if (members.Length is 0)
            return new SourceResult(SourceStatus.NoAvailableMember);
        var constructor = Symbols.GetConstructor(context, typeInfo, members);
        return new NamedObjectConverterContext(context, tracker, symbol, members, constructor).Invoke();
    }
}
