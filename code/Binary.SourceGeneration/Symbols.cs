namespace Mikodev.Binary.SourceGeneration;

using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;
using System.Linq;

public static partial class Symbols
{
    public static IMethodSymbol? GetConstructor(ITypeSymbol type, INamedTypeSymbol argument)
    {
        static bool Filter(IMethodSymbol method, INamedTypeSymbol @interface)
        {
            if (method.DeclaredAccessibility is not Accessibility.Public)
                return false;
            var parameters = method.Parameters;
            if (parameters.Length is not 1)
                return false;
            var parameter = parameters.Single();
            return SymbolEqualityComparer.Default.Equals(parameter.Type, @interface);
        }

        if (type.IsAbstract)
            return null;
        if (type is not INamedTypeSymbol symbol)
            return null;
        var result = symbol.InstanceConstructors.FirstOrDefault(x => Filter(x, argument));
        return result;
    }

    public static SymbolConstructorInfo<T>? GetConstructor<T>(ITypeSymbol type, ImmutableArray<T> members) where T : SymbolMemberInfo
    {
        static string Select(string? text) =>
            text?.ToUpperInvariant()
            ?? string.Empty;

        if (type.IsAbstract)
            return null;
        if (type is not INamedTypeSymbol symbol)
            return null;
        var constructors = symbol.InstanceConstructors
            .Where(x => x.DeclaredAccessibility is Accessibility.Public)
            .OrderByDescending(x => x.Parameters.Length)
            .ToList();
        var hasDefaultConstructor = symbol.IsValueType || constructors.Any(x => x.Parameters.Length is 0);
        if (hasDefaultConstructor && members.All(x => x.IsReadOnly is false))
            return new SymbolConstructorInfo<T>(members, ImmutableArray.Create<int>(), Enumerable.Range(0, members.Length).ToImmutableArray());
        var selector = new Func<T, string>(x => Select(x.Name));
        if (members.Select(selector).Distinct().Count() != members.Length)
            return null;

        // select constructor with most parameters
        var dictionary = members.Select((x, i) => (Key: selector.Invoke(x), Index: i)).ToDictionary(x => x.Key, v => v.Index);
        foreach (var i in constructors)
        {
            var parameters = i.Parameters;
            var objectIndexes = parameters
                .Select(x => dictionary.TryGetValue(Select(x.Name), out var index) && SymbolEqualityComparer.Default.Equals(members[index].TypeSymbol, x.Type) ? index : -1)
                .Where(x => x is not -1)
                .ToImmutableArray();
            if (objectIndexes.Length is 0 || objectIndexes.Length != parameters.Length)
                continue;
            var directIndexes = Enumerable.Range(0, members.Length).Except(objectIndexes).ToImmutableArray();
            if (directIndexes.Any(x => members[x].IsReadOnly))
                continue;
            return new SymbolConstructorInfo<T>(members, objectIndexes, directIndexes);
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

    public static bool IsTypeUnsupported(SourceGeneratorContext context, ITypeSymbol symbol)
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
        if (symbol is IErrorTypeSymbol)
            return false;
        if (symbol.IsStatic)
            return false;
        if (symbol.IsRefLikeType)
            return false;
        return symbol.TypeKind is TypeKind.Array or TypeKind.Class or TypeKind.Enum or TypeKind.Interface or TypeKind.Struct;
    }

    public static bool IsPropertyReturnsByRefOrReturnsByRefReadonly(IPropertySymbol property)
    {
        return property.ReturnsByRef || property.ReturnsByRefReadonly;
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
            // support by reference properties or not? (linq expression generator not support by reference properties)
            if (property is not null && IsPropertyReturnsByRefOrReturnsByRefReadonly(property))
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
