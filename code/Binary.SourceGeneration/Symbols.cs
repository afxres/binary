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

    public static ITypeSymbol? GetConverterType(SourceGeneratorContext context, ISymbol symbol)
    {
        var attribute = symbol.GetAttributes().FirstOrDefault(x => context.Equals(x.AttributeClass, Constants.ConverterAttributeTypeName));
        if (attribute is null)
            return null;
        return attribute.ConstructorArguments.Single().Value as ITypeSymbol;
    }

    public static ITypeSymbol? GetConverterCreatorType(SourceGeneratorContext context, ISymbol symbol)
    {
        var attribute = symbol.GetAttributes().FirstOrDefault(x => context.Equals(x.AttributeClass, Constants.ConverterCreatorAttributeTypeName));
        if (attribute is null)
            return null;
        return attribute.ConstructorArguments.Single().Value as ITypeSymbol;
    }

    public static bool IsIgnoredType(SourceGeneratorContext context, ITypeSymbol symbol)
    {
        if (SymbolEqualityComparer.Default.Equals(symbol.ContainingAssembly, context.GetNamedTypeSymbol(Constants.IConverterTypeName)?.ContainingAssembly))
            return true;
        var objectSymbol = context.Compilation.GetSpecialType(SpecialType.System_Object);
        if (SymbolEqualityComparer.Default.Equals(symbol.ContainingAssembly, objectSymbol.ContainingAssembly))
            return true;
        var enumerableSymbol = context.Compilation.GetSpecialType(SpecialType.System_Collections_IEnumerable);
        if (symbol.AllInterfaces.Any(x => SymbolEqualityComparer.Default.Equals(x, enumerableSymbol)))
            return true;
        return false;
    }

    public static bool IsTypeWithRequiredModifier(ITypeSymbol symbol)
    {
        static bool Filter(ISymbol member) => member switch
        {
            IFieldSymbol field => field.IsRequired,
            IPropertySymbol property => property.IsRequired,
            _ => false,
        };

        return symbol.GetMembers().Any(Filter);
    }

    public static bool IsTypeSupported(ITypeSymbol symbol)
    {
        return symbol.TypeKind is TypeKind.Array or TypeKind.Class or TypeKind.Enum or TypeKind.Interface or TypeKind.Struct;
    }

    public static ImmutableArray<ISymbol> GetObjectMembers(ITypeSymbol symbol)
    {
        var builder = ImmutableArray.CreateBuilder<ISymbol>();
        foreach (var member in symbol.GetMembers())
        {
            if (member.IsStatic || member.DeclaredAccessibility is not Accessibility.Public)
                continue;
            var property = member as IPropertySymbol;
            if (property is not null && property.IsIndexer)
                continue;
            var memberType = property?.Type ?? (member as IFieldSymbol)?.Type;
            if (memberType is null)
                continue;
            if (IsTypeSupported(memberType) is false)
                continue;
            builder.Add(member);
        }
        return builder.ToImmutable();
    }
}
