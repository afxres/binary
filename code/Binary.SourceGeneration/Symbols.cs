namespace Mikodev.Binary.SourceGeneration;

using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;
using System.Linq;

public static partial class Symbols
{
    public static SymbolConstructorInfo<T>? GetConstructor<T>(ITypeSymbol type, ImmutableArray<T> members) where T : SymbolMemberInfo
    {
        static string Select(string? text) =>
            text?.ToUpperInvariant()
            ?? string.Empty;

        if (type is not INamedTypeSymbol symbol)
            return null;

        var constructors = symbol.InstanceConstructors.Where(x => x.DeclaredAccessibility is Accessibility.Public).ToList();
        var hasDefaultConstructor = symbol.IsValueType || constructors.Any(x => x.Parameters.Length is 0);
        if (hasDefaultConstructor && members.All(x => x.IsReadOnly is false))
            return new SymbolConstructorInfo<T>(members, ImmutableArray<T>.Empty);

        var selector = new Func<T, string>(x => Select(x.Name));
        if (members.Select(selector).Distinct().Count() != members.Length)
            return null;

        // select constructor with most parameters
        var dictionary = members.ToDictionary(selector);
        foreach (var i in constructors.OrderByDescending(x => x.Parameters.Length))
        {
            var parameters = i.Parameters;
            var result = parameters
                .Select(x => dictionary.TryGetValue(Select(x.Name), out var member) && SymbolEqualityComparer.Default.Equals(member.TypeSymbol, x.Type) ? member : null)
                .OfType<T>()
                .ToImmutableArray();
            if (result.Length is 0 || result.Length != parameters.Length)
                continue;
            var except = members.Except(result).ToImmutableArray();
            if (except.Any(x => x.IsReadOnly))
                continue;
            return new SymbolConstructorInfo<T>(members, result);
        }
        return null;
    }

    public static ImmutableArray<AttributeData> GetAttributes(SourceGeneratorContext context, ISymbol symbol, params string[] attributesTypes)
    {
        var attributes = symbol.GetAttributes()
            .Where(i => attributesTypes.Any(x => context.Equals(i.AttributeClass, x)))
            .ToImmutableArray();
        return attributes;
    }

    public static ITypeSymbol? GetConverterType(SourceGeneratorContext context, ISymbol symbol)
    {
        var attribute = symbol.GetAttributes().FirstOrDefault(x => context.Equals(x.AttributeClass, Constants.ConverterAttributeTypeName));
        if (attribute is null)
            return null;
        var argument = attribute.ConstructorArguments.Single();
        var type = argument.Value as ITypeSymbol;
        if (type is null || type.AllInterfaces.Any(x => context.Equals(x, Constants.IConverterTypeName)) is false)
            context.Throw(Constants.RequireConverterType, GetLocation(attribute), null);
        return type;
    }

    public static ITypeSymbol? GetConverterCreatorType(SourceGeneratorContext context, ISymbol symbol)
    {
        var attribute = symbol.GetAttributes().FirstOrDefault(x => context.Equals(x.AttributeClass, Constants.ConverterCreatorAttributeTypeName));
        if (attribute is null)
            return null;
        var argument = attribute.ConstructorArguments.Single();
        var type = argument.Value as ITypeSymbol;
        if (type is null || type.AllInterfaces.Any(x => context.Equals(x, Constants.IConverterCreatorTypeName)) is false)
            context.Throw(Constants.RequireConverterCreatorType, GetLocation(attribute), null);
        return type;
    }
}
