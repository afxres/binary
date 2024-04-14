namespace Mikodev.Binary.SourceGeneration;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

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

    public static SymbolConstructorInfo<T>? GetConstructor<T>(SourceGeneratorContext context, SymbolTypeInfo typeInfo, ImmutableArray<T> members) where T : SymbolMemberInfo
    {
        static Dictionary<string, int>? CreateIgnoreCaseDictionary(ImmutableArray<T> members)
        {
            var dictionary = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);
            for (var i = 0; i < members.Length; i++)
            {
                var symbol = members[i].Symbol;
                if (dictionary.ContainsKey(symbol.Name))
                    return null;
                dictionary.Add(symbol.Name, i);
            }
            return dictionary;
        }

        static bool ValidateConstructorWithMembers(SourceGeneratorContext context, IMethodSymbol? constructor, ImmutableHashSet<ISymbol> required, ImmutableArray<T> members)
        {
            const string SetsRequiredMembersAttribute = "System.Diagnostics.CodeAnalysis.SetsRequiredMembersAttribute";
            if (members.Any(x => x.IsReadOnly))
                return false;
            if (required.Count is 0)
                return true;
            if (constructor is not null && context.GetAttribute(constructor, SetsRequiredMembersAttribute) is not null)
                return true;
            if (required.IsSubsetOf(members.Select(x => x.Symbol)))
                return true;
            return false;
        }

        var type = typeInfo.Symbol;
        var cancellation = context.CancellationToken;
        if (type.IsAbstract)
            return null;
        if (type is not INamedTypeSymbol symbol)
            return null;
        var constructors = symbol.InstanceConstructors
            .Where(x => x.DeclaredAccessibility is Accessibility.Public)
            .OrderByDescending(x => x.Parameters.Length)
            .ToList();
        cancellation.ThrowIfCancellationRequested();
        var defaultConstructor = constructors.FirstOrDefault(x => x.Parameters.Length is 0);
        var hasDefaultConstructor = symbol.IsValueType || defaultConstructor is not null;
        if (hasDefaultConstructor && ValidateConstructorWithMembers(context, defaultConstructor, typeInfo.RequiredFieldsAndProperties, members))
            return new SymbolConstructorInfo<T>(members, [], Enumerable.Range(0, members.Length).ToImmutableArray());
        if (CreateIgnoreCaseDictionary(members) is not { } dictionary)
            return null;

        // select constructor with most parameters
        foreach (var i in constructors)
        {
            const int NotFound = -1;
            cancellation.ThrowIfCancellationRequested();
            var parameters = i.Parameters;
            if (parameters.Length is 0)
                continue;
            var objectIndexes = parameters
                .Select(x => dictionary.TryGetValue(x.Name, out var index) && SymbolEqualityComparer.Default.Equals(members[index].Type, x.Type) ? index : NotFound)
                .Where(x => x is not NotFound)
                .ToImmutableArray();
            if (objectIndexes.Length != parameters.Length)
                continue;
            var directIndexes = Enumerable.Range(0, members.Length).Except(objectIndexes).ToImmutableArray();
            if (ValidateConstructorWithMembers(context, i, typeInfo.RequiredFieldsAndProperties, directIndexes.Select(x => members[x]).ToImmutableArray()) is false)
                continue;
            return new SymbolConstructorInfo<T>(members, objectIndexes, directIndexes);
        }
        return null;
    }

    public static ITypeSymbol? GetConverterType(SourceGeneratorContext context, ISymbol symbol)
    {
        var attribute = context.GetAttribute(symbol, Constants.ConverterAttributeTypeName);
        if (attribute is null)
            return null;
        return attribute.ConstructorArguments.Single().Value as ITypeSymbol;
    }

    public static ITypeSymbol? GetConverterCreatorType(SourceGeneratorContext context, ISymbol symbol)
    {
        var attribute = context.GetAttribute(symbol, Constants.ConverterCreatorAttributeTypeName);
        if (attribute is null)
            return null;
        return attribute.ConstructorArguments.Single().Value as ITypeSymbol;
    }

    public static Location GetLocation(ISymbol symbol)
    {
        return symbol.Locations.FirstOrDefault() ?? Location.None;
    }

    public static Location GetLocation(AttributeData? attribute)
    {
        var reference = attribute?.ApplicationSyntaxReference;
        if (reference is not null)
            return Location.Create(reference.SyntaxTree, reference.Span);
        return Location.None;
    }

    public static bool HasPublicSetter(IPropertySymbol property)
    {
        var setter = property.SetMethod;
        if (setter is null)
            return false;
        return setter.DeclaredAccessibility is Accessibility.Public;
    }

    public static bool IsKeyword(string name)
    {
        return SyntaxFacts.GetKeywordKind(name) != SyntaxKind.None || SyntaxFacts.GetContextualKeywordKind(name) != SyntaxKind.None;
    }

    public static bool IsRequiredFieldOrProperty(ISymbol symbol)
    {
        return symbol is IFieldSymbol field
            ? field.IsRequired
            : ((IPropertySymbol)symbol).IsRequired;
    }

    public static bool IsReadOnlyFieldOrProperty(ISymbol symbol)
    {
        return symbol is IFieldSymbol field
            ? field.IsReadOnly
            : HasPublicSetter((IPropertySymbol)symbol) is false;
    }

    public static bool IsReturnsByRefOrReturnsByRefReadonly(IPropertySymbol property)
    {
        return property.ReturnsByRef || property.ReturnsByRefReadonly;
    }

    public static bool IsTypeIgnored(SourceGeneratorContext context, ITypeSymbol symbol)
    {
        if (SymbolEqualityComparer.Default.Equals(symbol.ContainingAssembly, context.Compilation.GetTypeByMetadataName(Constants.IConverterTypeName)?.ContainingAssembly))
            return true;
        var objectSymbol = context.Compilation.GetSpecialType(SpecialType.System_Object);
        if (SymbolEqualityComparer.Default.Equals(symbol.ContainingAssembly, objectSymbol.ContainingAssembly))
            return true;
        var enumerableSymbol = context.Compilation.GetSpecialType(SpecialType.System_Collections_IEnumerable);
        if (symbol.AllInterfaces.Any(x => SymbolEqualityComparer.Default.Equals(x, enumerableSymbol)))
            return true;
        return false;
    }

    public static bool IsTypeInvalid(ITypeSymbol symbol)
    {
        if (symbol is IErrorTypeSymbol)
            return true;
        if (symbol.IsStatic)
            return true;
        if (symbol.IsRefLikeType)
            return true;
        return symbol.TypeKind is not TypeKind.Array and not TypeKind.Class and not TypeKind.Enum and not TypeKind.Interface and not TypeKind.Struct;
    }

    public static ImmutableArray<ISymbol> FilterFieldsAndProperties(ImmutableArray<ISymbol> members, CancellationToken cancellation)
    {
        var builder = ImmutableArray.CreateBuilder<ISymbol>();
        foreach (var member in members)
        {
            cancellation.ThrowIfCancellationRequested();
            if (member.IsStatic || member.DeclaredAccessibility is not Accessibility.Public)
                continue;
            var property = member as IPropertySymbol;
            if (property is not null && property.IsIndexer)
                continue;
            // support by reference properties or not? (linq expression generator not support by reference properties)
            if (property is not null && IsReturnsByRefOrReturnsByRefReadonly(property))
                continue;
            var memberType = property?.Type ?? (member as IFieldSymbol)?.Type;
            if (memberType is null)
                continue;
            if (IsTypeInvalid(memberType))
                continue;
            builder.Add(member);
        }
        return builder.ToImmutable();
    }

    public static ImmutableArray<ISymbol> GetAllFieldsAndProperties(Compilation compilation, ITypeSymbol type, out ImmutableSortedSet<string> conflict, CancellationToken cancellation)
    {
        static ImmutableHashSet<ITypeSymbol> Expand(ITypeSymbol type)
        {
            var result = ImmutableHashSet.CreateBuilder<ITypeSymbol>(SymbolEqualityComparer.Default);
            for (var i = type; i != null; i = i.BaseType)
                _ = result.Add(i);
            return result.ToImmutable();
        }

        var source = type.TypeKind is TypeKind.Interface
            ? ImmutableHashSet.Create<ITypeSymbol>(SymbolEqualityComparer.Default, type).Union(type.AllInterfaces)
            : Expand(type);
        var result = ImmutableArray.CreateBuilder<ISymbol>();
        var errors = ImmutableSortedSet.CreateBuilder<string>();
        var dictionary = new SortedDictionary<string, ISymbol>();
        foreach (var target in source)
        {
            cancellation.ThrowIfCancellationRequested();
            var members = target.GetMembers();
            foreach (var member in members)
            {
                cancellation.ThrowIfCancellationRequested();
                var field = member as IFieldSymbol;
                var property = member as IPropertySymbol;
                if (field is null && property is null)
                    continue;

                // ignore overriding or shadowing
                var indexer = property is not null && property.IsIndexer;
                if (indexer)
                    result.Add(member);
                else if (dictionary.TryGetValue(member.Name, out var exists) is false)
                    dictionary.Add(member.Name, member);
                else if (compilation.ClassifyConversion(target, exists.ContainingType) is { } alpha && (alpha.IsIdentity is false && alpha.IsImplicit && alpha.IsReference))
                    dictionary[member.Name] = member;
                else if (compilation.ClassifyConversion(exists.ContainingType, target) is { } bravo && (alpha.IsImplicit == bravo.IsImplicit))
                    _ = errors.Add(member.Name);
            }
        }
        conflict = errors.ToImmutable();
        if (conflict.Count is not 0)
            return [];
        result.AddRange(dictionary.Values);
        return result.ToImmutable();
    }
}
