namespace Mikodev.Binary.SourceGeneration;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Mikodev.Binary.SourceGeneration.Internal;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

public static partial class Symbols
{
    private const string NamedObjectAttribute = "NamedObjectAttribute";

    private const string TupleObjectAttribute = "TupleObjectAttribute";

    public static bool ValidateContextType(SourceGeneratorContext context, TypeDeclarationSyntax declaration, INamedTypeSymbol symbol)
    {
        if (ValidateContextType(declaration, symbol) is not { } descriptor)
            return true;
        context.Collect(descriptor.With(symbol, [GetSymbolDiagnosticDisplayString(symbol)]));
        return false;
    }

    public static bool ValidateIncludeType(SourceGeneratorContext context, IReadOnlyDictionary<ITypeSymbol, AttributeData> dictionary, AttributeData attribute, ITypeSymbol symbol)
    {
        if (ValidateIncludeType(dictionary, symbol) is not { } descriptor)
            return true;
        context.Collect(descriptor.With(attribute, [GetSymbolDiagnosticDisplayString(symbol)]));
        return false;
    }

    public static SymbolTypeKind ValidateType(SourceGeneratorContext context, ITypeSymbol symbol)
    {
        var cancellation = context.CancellationToken;
        var converterAttribute = context.GetAttribute(symbol, Constants.ConverterAttributeTypeName);
        var converterCreatorAttribute = context.GetAttribute(symbol, Constants.ConverterCreatorAttributeTypeName);
        var namedObjectAttribute = context.GetAttribute(symbol, Constants.NamedObjectAttributeTypeName);
        var tupleObjectAttribute = context.GetAttribute(symbol, Constants.TupleObjectAttributeTypeName);

        var diagnostics = new List<Diagnostic>();
        var symbolText = GetSymbolDiagnosticDisplayString(symbol);
        var attributes = new[] { converterAttribute, converterCreatorAttribute, namedObjectAttribute, tupleObjectAttribute }
            .OfType<AttributeData>()
            .ToList();

        ValidateConverterAttribute(context, converterAttribute, diagnostics);
        ValidateConverterCreatorAttribute(context, converterCreatorAttribute, diagnostics);
        cancellation.ThrowIfCancellationRequested();

        if (attributes.Count is 0 or 1)
            ValidateType(context, symbol, symbolText, attributes.SingleOrDefault(), diagnostics);
        else
            diagnostics.Add(Constants.MultipleAttributesFoundOnType.With(symbol, [symbolText]));

        if (diagnostics.Count is 0)
            return attributes.Count is 0 ? SymbolTypeKind.RawType : SymbolTypeKind.CustomType;
        foreach (var diagnostic in diagnostics)
            context.Collect(diagnostic);
        return SymbolTypeKind.BadType;
    }

    private static DiagnosticDescriptor? ValidateContextType(TypeDeclarationSyntax declaration, INamedTypeSymbol symbol)
    {
        if (declaration.Modifiers.Any(x => x.IsKind(SyntaxKind.PartialKeyword)) is false)
            return Constants.ContextTypeNotPartial;
        if (symbol.ContainingNamespace.IsGlobalNamespace)
            return Constants.ContextTypeNotInNamespace;
        if (symbol.ContainingType is not null)
            return Constants.ContextTypeNested;
        if (symbol.IsGenericType)
            return Constants.ContextTypeGeneric;
        return null;
    }

    private static DiagnosticDescriptor? ValidateIncludeType(IReadOnlyDictionary<ITypeSymbol, AttributeData> dictionary, ITypeSymbol symbol)
    {
        if (IsTypeInvalid(symbol))
            return Constants.RequireValidTypeForIncludeAttribute;
        else if (dictionary.ContainsKey(symbol))
            return Constants.IncludeTypeDuplicated;
        return null;
    }

    private static void ValidateType(SourceGeneratorContext context, ITypeSymbol symbol, string symbolText, AttributeData? attribute, List<Diagnostic> diagnostics)
    {
        var tupleMembers = new SortedDictionary<int, ISymbol>();
        var namedMembers = new SortedDictionary<string, ISymbol>();
        var typeInfo = context.GetTypeInfo(symbol);
        var typeAttribute = attribute?.AttributeClass;
        foreach (var member in typeInfo.OriginalFieldsAndProperties)
            ValidateMember(context, symbolText, typeAttribute?.Name, member, typeInfo.RequiredFieldsAndProperties, diagnostics, namedMembers, tupleMembers);
        var tupleKeys = tupleMembers.Keys;
        if (tupleKeys.Count is not 0 && (tupleKeys.First() is not 0 || tupleKeys.Last() != tupleKeys.Count - 1))
            diagnostics.Add(Constants.TupleKeyNotSequential.With(symbol, [symbolText]));
        if (diagnostics.Count is not 0)
            return;

        var members = typeAttribute?.Name switch
        {
            NamedObjectAttribute => namedMembers.Values,
            TupleObjectAttribute => tupleMembers.Values,
            _ => default(IEnumerable<ISymbol>),
        };
        if (members is null)
            return;
        else if (typeInfo.ConflictFieldsAndProperties is { Length: not 0 } targets)
            targets.ForEach(name => diagnostics.Add(Constants.AmbiguousMemberFound.With(attribute, [name, symbolText])));
        else if (typeInfo.FilteredFieldsAndProperties.Intersect(members, SymbolEqualityComparer.Default).Any() is false)
            diagnostics.Add(Constants.NoAvailableMemberFound.With(attribute, [symbolText]));
        return;
    }

    private static void ValidateConverterAttribute(SourceGeneratorContext context, AttributeData? attribute, List<Diagnostic> diagnostics)
    {
        if (attribute is null)
            return;
        var argument = attribute.ConstructorArguments.Single();
        if (argument.Value is not ITypeSymbol type || type.AllInterfaces.Any(x => context.Equals(x, Constants.IConverterTypeName)) is false)
            diagnostics.Add(Constants.RequireConverterType.With(attribute));
        return;
    }

    private static void ValidateConverterCreatorAttribute(SourceGeneratorContext context, AttributeData? attribute, List<Diagnostic> diagnostics)
    {
        if (attribute is null)
            return;
        var argument = attribute.ConstructorArguments.Single();
        if (argument.Value is not ITypeSymbol type || type.AllInterfaces.Any(x => context.Equals(x, Constants.IConverterCreatorTypeName)) is false)
            diagnostics.Add(Constants.RequireConverterCreatorType.With(attribute));
        return;
    }

    private static void ValidateNamedKeyAttribute(ISymbol member, AttributeData? attribute, List<Diagnostic> diagnostics, SortedDictionary<string, ISymbol> namedMembers)
    {
        if (attribute is null)
            return;
        var key = (string?)attribute.ConstructorArguments.Single().Value;
        if (key is null || key.Length is 0)
            diagnostics.Add(Constants.NamedKeyNullOrEmpty.With(attribute));
        else if (namedMembers.TryAdd(key, member) is false)
            diagnostics.Add(Constants.NamedKeyDuplicated.With(attribute, [key]));
        return;
    }

    private static void ValidateTupleKeyAttribute(ISymbol member, AttributeData? attribute, List<Diagnostic> diagnostics, SortedDictionary<int, ISymbol> tupleMembers)
    {
        if (attribute is null)
            return;
        var key = (int)attribute.ConstructorArguments.Single().Value!;
        if (tupleMembers.TryAdd(key, member) is false)
            diagnostics.Add(Constants.TupleKeyDuplicated.With(attribute, [key]));
        return;
    }

    private static void ValidateMember(SourceGeneratorContext context, string containingTypeName, string? typeAttribute, ISymbol member, ImmutableHashSet<ISymbol> requiredMembers, List<Diagnostic> diagnostics, SortedDictionary<string, ISymbol> namedMembers, SortedDictionary<int, ISymbol> tupleMembers)
    {
        var cancellation = context.CancellationToken;
        var converterAttribute = context.GetAttribute(member, Constants.ConverterAttributeTypeName);
        var converterCreatorAttribute = context.GetAttribute(member, Constants.ConverterCreatorAttributeTypeName);
        var namedKeyAttribute = context.GetAttribute(member, Constants.NamedKeyAttributeTypeName);
        var tupleKeyAttribute = context.GetAttribute(member, Constants.TupleKeyAttributeTypeName);
        cancellation.ThrowIfCancellationRequested();

        var memberName = member.Name;
        var hasKey = namedKeyAttribute is not null || tupleKeyAttribute is not null;
        var requiredMemberWithoutKey = hasKey is false && requiredMembers.Count is not 0 && IsRequiredFieldOrProperty(member);
        if (requiredMemberWithoutKey && typeAttribute is NamedObjectAttribute)
            diagnostics.Add(Constants.RequireNamedKeyAttributeForRequiredMember.With(member, [memberName, containingTypeName]));
        if (requiredMemberWithoutKey && typeAttribute is TupleObjectAttribute)
            diagnostics.Add(Constants.RequireTupleKeyAttributeForRequiredMember.With(member, [memberName, containingTypeName]));
        cancellation.ThrowIfCancellationRequested();

        if (converterAttribute is null &&
            converterCreatorAttribute is null &&
            namedKeyAttribute is null &&
            tupleKeyAttribute is null)
            return;

        ValidateConverterAttribute(context, converterAttribute, diagnostics);
        ValidateConverterCreatorAttribute(context, converterCreatorAttribute, diagnostics);
        ValidateNamedKeyAttribute(member, namedKeyAttribute, diagnostics, namedMembers);
        ValidateTupleKeyAttribute(member, tupleKeyAttribute, diagnostics, tupleMembers);
        cancellation.ThrowIfCancellationRequested();

        var property = member as IPropertySymbol;
        if (member.IsStatic || member.DeclaredAccessibility is not Accessibility.Public)
            diagnostics.Add(Constants.RequirePublicInstanceMember.With(member, [memberName, containingTypeName]));
        else if (property is not null && property.IsIndexer)
            diagnostics.Add(Constants.RequireNotIndexer.With(member, [containingTypeName]));
        else if (property is not null && property.GetMethod?.DeclaredAccessibility is not Accessibility.Public)
            diagnostics.Add(Constants.RequirePublicGetter.With(member, [memberName, containingTypeName]));
        cancellation.ThrowIfCancellationRequested();

        if (converterAttribute is not null && converterCreatorAttribute is not null)
            diagnostics.Add(Constants.MultipleAttributesFoundOnMember.With(member, [memberName, containingTypeName]));
        if (namedKeyAttribute is not null && typeAttribute is not NamedObjectAttribute)
            diagnostics.Add(Constants.RequireNamedObjectAttribute.With(namedKeyAttribute, [memberName, containingTypeName]));
        if (tupleKeyAttribute is not null && typeAttribute is not TupleObjectAttribute)
            diagnostics.Add(Constants.RequireTupleObjectAttribute.With(tupleKeyAttribute, [memberName, containingTypeName]));
        cancellation.ThrowIfCancellationRequested();

        if (property is not null && IsReturnsByRefOrReturnsByRefReadonly(property))
            diagnostics.Add(Constants.RequireNotByReferenceProperty.With(member, [memberName, containingTypeName]));
        cancellation.ThrowIfCancellationRequested();

        var memberType = property?.Type ?? ((IFieldSymbol)member).Type;
        if (IsTypeInvalid(memberType))
            diagnostics.Add(Constants.RequireValidTypeForMember.With(member, [GetSymbolDiagnosticDisplayString(memberType), memberName, containingTypeName]));
        cancellation.ThrowIfCancellationRequested();

        if (hasKey is false && converterAttribute is not null)
            diagnostics.Add(Constants.RequireKeyAttributeForConverterAttribute.With(converterAttribute, [memberName, containingTypeName]));
        if (hasKey is false && converterCreatorAttribute is not null)
            diagnostics.Add(Constants.RequireKeyAttributeForConverterCreatorAttribute.With(converterCreatorAttribute, [memberName, containingTypeName]));
        cancellation.ThrowIfCancellationRequested();
    }
}
