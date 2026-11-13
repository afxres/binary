namespace Mikodev.Binary.SourceGeneration.Contexts;

using Microsoft.CodeAnalysis;
using Mikodev.Binary.SourceGeneration.Internal;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

public sealed partial class NamedObjectConverterContext
{
    private static string? GetCustomNamedKey(SourceGeneratorContext context, ISymbol member)
    {
        var attribute = context.GetAttribute(member, Constants.NamedKeyAttributeTypeName);
        if (attribute is null)
            return null;
        return attribute.TryGetConstructorArgument<string>(out var key) ? key : null;
    }

    public static SourceResult? Invoke(SourceGeneratorContext context, SourceGeneratorTracker tracker, ITypeSymbol symbol)
    {
        var attribute = context.GetAttribute(symbol, Constants.NamedObjectAttributeTypeName);
        if (attribute is null && Symbols.IsTypeIgnored(context, symbol))
            return new SourceResult.Ignored();
        var typeInfo = context.GetTypeInfo(symbol);
        var required = typeInfo.RequiredFieldsAndProperties.Count is not 0;
        var dictionary = new SortedDictionary<string, SymbolNamedObjectMemberInfo>(StringComparer.Ordinal);
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
            var memberInfo = new SymbolNamedObjectMemberInfo(member, optional, key, literal);
            dictionary.Add(key, memberInfo);
            cancellation.ThrowIfCancellationRequested();
        }

        var members = dictionary.Values.ToImmutableArray();
        if (members.Length is 0)
        {
            var diagnosticArguments = ImmutableArray.CreateBuilder<(DiagnosticDescriptor, object?[]?)>();
            var symbolText = Symbols.GetSymbolDiagnosticDisplayString(symbol);
            if (context.GetTypeInfo(symbol).ConflictFieldsAndProperties is { Length: not 0 } targets)
                foreach (var name in targets)
                    diagnosticArguments.Add((Diagnostics.AmbiguousMemberFound, [name, symbolText]));
            else
                diagnosticArguments.Add((Diagnostics.NoAvailableMemberFound, [symbolText]));
            return new SourceResult.Diagnostic(diagnosticArguments.ToImmutable());
        }
        var constructor = Symbols.GetConstructor(context, typeInfo, members);
        return new NamedObjectConverterContext(context, tracker, symbol, members, constructor).Build();
    }
}
