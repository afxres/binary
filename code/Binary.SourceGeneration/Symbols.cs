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

    public static int CompareInheritance(Compilation compilation, ITypeSymbol x, ITypeSymbol y)
    {
        if (x.IsValueType || y.IsValueType)
            throw new ArgumentException("Require reference type.");
        var alpha = compilation.ClassifyCommonConversion(x, y);
        var bravo = compilation.ClassifyCommonConversion(y, x);
        if (alpha.IsIdentity || bravo.IsIdentity)
            throw new ArgumentException("Identical types detected.");
        if (alpha.IsReference && alpha.IsImplicit)
            return -1;
        if (bravo.IsReference && bravo.IsImplicit)
            return 1;
        return 0;
    }

    public static ImmutableArray<ISymbol> GetAllPropertiesForInterfaceType(Compilation compilation, ITypeSymbol type, out ImmutableArray<string> conflict, CancellationToken cancellation)
    {
        if (type.TypeKind is not TypeKind.Interface)
            throw new ArgumentException("Require interface type.");

        var source = ImmutableArray.CreateRange<ITypeSymbol>(type.AllInterfaces).Add(type);
        var result = ImmutableArray.CreateBuilder<ISymbol>();
        var errors = ImmutableArray.CreateBuilder<string>();
        var dictionary = new SortedDictionary<string, List<IPropertySymbol>>();

        void Insert(IPropertySymbol member)
        {
            var same = new List<IPropertySymbol>();
            var less = new List<IPropertySymbol>();
            if (dictionary.TryGetValue(member.Name, out var values))
            {
                foreach (var i in values)
                {
                    var signal = CompareInheritance(compilation, i.ContainingType, member.ContainingType);
                    if (signal is 0)
                        same.Add(i);
                    else if (signal is -1)
                        less.Add(i);
                    cancellation.ThrowIfCancellationRequested();
                }
            }

            if (less.Count is 0)
                less.Add(member);
            less.AddRange(same);
            dictionary[member.Name] = less;
        }

        foreach (var target in source)
        {
            cancellation.ThrowIfCancellationRequested();
            var members = target.GetMembers().OfType<IPropertySymbol>().ToList();
            foreach (var member in members)
            {
                // ignore overriding or shadowing
                if (member.IsIndexer)
                    result.Add(member);
                else
                    Insert(member);
                cancellation.ThrowIfCancellationRequested();
            }
        }

        foreach (var i in dictionary)
        {
            var values = i.Value;
            if (values.Count is 1)
                result.Add(values.First());
            else
                errors.Add(i.Key);
            cancellation.ThrowIfCancellationRequested();
        }

        conflict = errors.ToImmutable();
        if (conflict.Length is not 0)
            return [];
        return result.ToImmutable();
    }

    public static ImmutableArray<ISymbol> GetAllFieldsAndPropertiesForNonInterfaceType(ITypeSymbol type, CancellationToken cancellation)
    {
        if (type.TypeKind is TypeKind.Interface)
            throw new ArgumentException("Require not interface type.");

        var result = ImmutableArray.CreateBuilder<ISymbol>();
        var dictionary = new SortedDictionary<string, ISymbol>();
        for (var target = type; target is not null; target = target.BaseType)
        {
            cancellation.ThrowIfCancellationRequested();
            var members = target.GetMembers();
            foreach (var member in members)
            {
                cancellation.ThrowIfCancellationRequested();
                if (member is not IFieldSymbol and not IPropertySymbol)
                    continue;
                // ignore overriding or shadowing
                if (member is IPropertySymbol { IsIndexer: true })
                    result.Add(member);
                else if (dictionary.ContainsKey(member.Name) is false)
                    dictionary.Add(member.Name, member);
                cancellation.ThrowIfCancellationRequested();
            }
        }
        result.AddRange(dictionary.Values);
        return result.ToImmutable();
    }

    public static ImmutableArray<ISymbol> GetAllFieldsAndProperties(Compilation compilation, ITypeSymbol type, out ImmutableArray<string> conflict, CancellationToken cancellation)
    {
        if (type.TypeKind is TypeKind.Interface)
            return GetAllPropertiesForInterfaceType(compilation, type, out conflict, cancellation);
        conflict = [];
        return GetAllFieldsAndPropertiesForNonInterfaceType(type, cancellation);
    }
}
