﻿namespace Mikodev.Binary.SourceGeneration;

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

    public static SymbolConstructorInfo<T>? GetConstructor<T>(SourceGeneratorContext context, ITypeSymbol type, ImmutableArray<T> members) where T : SymbolMemberInfo
    {
        static bool ValidateConstructorWithMembers(SourceGeneratorContext context, IMethodSymbol? constructor, ImmutableHashSet<ISymbol> required, ImmutableArray<T> members)
        {
            const string SetsRequiredMembersAttribute = "System.Diagnostics.CodeAnalysis.SetsRequiredMembersAttribute";
            if (members.Any(x => x.IsReadOnly))
                return false;
            if (required.Count is 0)
                return true;
            if (constructor is not null && constructor.GetAttributes().Any(x => context.Equals(x.AttributeClass, SetsRequiredMembersAttribute)))
                return true;
            if (required.IsSubsetOf(members.Select(x => x.Symbol)))
                return true;
            return false;
        }

        if (type.IsAbstract)
            return null;
        if (type is not INamedTypeSymbol symbol)
            return null;
        var comparer = StringComparer.InvariantCultureIgnoreCase;
        var required = symbol.GetMembers().Where(IsMemberRequired).ToImmutableHashSet(SymbolEqualityComparer.Default);
        var constructors = symbol.InstanceConstructors
            .Where(x => x.DeclaredAccessibility is Accessibility.Public)
            .OrderByDescending(x => x.Parameters.Length)
            .ToList();
        var defaultConstructor = constructors.FirstOrDefault(x => x.Parameters.Length is 0);
        var hasDefaultConstructor = symbol.IsValueType || defaultConstructor is not null;
        if (hasDefaultConstructor && ValidateConstructorWithMembers(context, defaultConstructor, required, members))
            return new SymbolConstructorInfo<T>(members, ImmutableArray.Create<int>(), Enumerable.Range(0, members.Length).ToImmutableArray());
        if (members.Select(x => x.Name).Distinct(comparer).Count() != members.Length)
            return null;

        // select constructor with most parameters
        var dictionary = members.Select((x, i) => (Key: x.Name, Index: i)).ToDictionary(x => x.Key, v => v.Index, comparer);
        foreach (var i in constructors)
        {
            var parameters = i.Parameters;
            if (parameters.Length is 0)
                continue;
            var objectIndexes = parameters
                .Select(x => dictionary.TryGetValue(x.Name, out var index) && SymbolEqualityComparer.Default.Equals(members[index].TypeSymbol, x.Type) ? index : -1)
                .Where(x => x is not -1)
                .ToImmutableArray();
            if (objectIndexes.Length != parameters.Length)
                continue;
            var directIndexes = Enumerable.Range(0, members.Length).Except(objectIndexes).ToImmutableArray();
            if (ValidateConstructorWithMembers(context, i, required, directIndexes.Select(x => members[x]).ToImmutableArray()) is false)
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

    public static bool IsMemberRequired(ISymbol symbol)
    {
        return symbol switch
        {
            IFieldSymbol field => field.IsRequired,
            IPropertySymbol property => property.IsRequired,
            _ => false,
        };
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
