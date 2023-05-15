﻿namespace Mikodev.Binary.SourceGeneration;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

public static partial class Symbols
{
    public static bool ValidateContextType(SourceProductionContext production, TypeDeclarationSyntax declaration, INamedTypeSymbol symbol)
    {
        if (ValidateContextType(declaration, symbol) is not { } descriptor)
            return true;
        production.ReportDiagnostic(Diagnostic.Create(descriptor, GetLocation(symbol), new object[] { GetSymbolDiagnosticDisplayString(symbol) }));
        return false;
    }

    public static bool ValidateIncludeType(SourceProductionContext production, IReadOnlyDictionary<ITypeSymbol, AttributeData> dictionary, AttributeData attribute, ITypeSymbol symbol)
    {
        if (ValidateIncludeType(dictionary, symbol) is not { } descriptor)
            return true;
        production.ReportDiagnostic(Diagnostic.Create(descriptor, GetLocation(attribute), new object[] { GetSymbolDiagnosticDisplayString(symbol) }));
        return false;
    }

    public static bool ValidateType(SourceProductionContext production, SourceGeneratorContext context, ITypeSymbol symbol)
    {
        var cancellation = context.CancellationToken;
        var converterAttribute = context.GetAttribute(symbol, Constants.ConverterAttributeTypeName);
        var converterCreatorAttribute = context.GetAttribute(symbol, Constants.ConverterCreatorAttributeTypeName);
        var namedObjectAttribute = context.GetAttribute(symbol, Constants.NamedObjectAttributeTypeName);
        var tupleObjectAttribute = context.GetAttribute(symbol, Constants.TupleObjectAttributeTypeName);

        var symbolDisplay = GetSymbolDiagnosticDisplayString(symbol);
        var diagnostics = new List<Diagnostic>();
        var attributes = new[] { converterAttribute, converterCreatorAttribute, namedObjectAttribute, tupleObjectAttribute }
            .OfType<AttributeData>()
            .ToImmutableArray();

        ValidateConverterAttribute(context, converterAttribute, diagnostics);
        ValidateConverterCreatorAttribute(context, converterCreatorAttribute, diagnostics);
        cancellation.ThrowIfCancellationRequested();

        if (attributes.Length > 1)
            diagnostics.Add(Diagnostic.Create(Constants.MultipleAttributesFoundOnType, GetLocation(symbol), new object[] { symbolDisplay }));
        else
            ValidateType(context, symbol, symbolDisplay, attributes.SingleOrDefault()?.AttributeClass, diagnostics);

        if (diagnostics.Count is 0)
            return true;
        foreach (var diagnostic in diagnostics)
            production.ReportDiagnostic(diagnostic);
        return false;
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

    private static void ValidateType(SourceGeneratorContext context, ITypeSymbol symbol, string symbolDisplay, INamedTypeSymbol? typeAttribute, List<Diagnostic> diagnostics)
    {
        var tupleKeys = new SortedSet<int>();
        var namedKeys = new HashSet<string>();
        var typeInfo = context.GetTypeInfo(symbol);
        foreach (var member in typeInfo.OriginalFieldsAndProperties)
            ValidateMember(context, symbolDisplay, typeAttribute?.Name, member, typeInfo.RequiredFieldsAndProperties, diagnostics, namedKeys, tupleKeys);
        if (tupleKeys.Count is not 0 && (tupleKeys.Min is not 0 || tupleKeys.Max != tupleKeys.Count - 1))
            diagnostics.Add(Diagnostic.Create(Constants.TupleKeyNotSequential, GetLocation(symbol), new object[] { symbolDisplay }));
        return;
    }

    private static void ValidateConverterAttribute(SourceGeneratorContext context, AttributeData? attribute, List<Diagnostic> diagnostics)
    {
        if (attribute is null)
            return;
        var argument = attribute.ConstructorArguments.Single();
        if (argument.Value is not ITypeSymbol type || type.AllInterfaces.Any(x => context.Equals(x, Constants.IConverterTypeName)) is false)
            diagnostics.Add(Diagnostic.Create(Constants.RequireConverterType, GetLocation(attribute)));
        return;
    }

    private static void ValidateConverterCreatorAttribute(SourceGeneratorContext context, AttributeData? attribute, List<Diagnostic> diagnostics)
    {
        if (attribute is null)
            return;
        var argument = attribute.ConstructorArguments.Single();
        if (argument.Value is not ITypeSymbol type || type.AllInterfaces.Any(x => context.Equals(x, Constants.IConverterCreatorTypeName)) is false)
            diagnostics.Add(Diagnostic.Create(Constants.RequireConverterCreatorType, GetLocation(attribute)));
        return;
    }

    private static void ValidateNamedKeyAttribute(AttributeData? attribute, List<Diagnostic> diagnostics, HashSet<string> keys)
    {
        if (attribute is null)
            return;
        var key = (string?)attribute.ConstructorArguments.Single().Value;
        if (key is null || key.Length is 0)
            diagnostics.Add(Diagnostic.Create(Constants.NamedKeyNullOrEmpty, GetLocation(attribute)));
        else if (keys.Add(key) is false)
            diagnostics.Add(Diagnostic.Create(Constants.NamedKeyDuplicated, GetLocation(attribute), new object[] { key }));
        return;
    }

    private static void ValidateTupleKeyAttribute(AttributeData? attribute, List<Diagnostic> diagnostics, SortedSet<int> keys)
    {
        if (attribute is null)
            return;
        var key = (int)attribute.ConstructorArguments.Single().Value!;
        if (keys.Add(key) is false)
            diagnostics.Add(Diagnostic.Create(Constants.TupleKeyDuplicated, GetLocation(attribute), new object[] { key }));
        return;
    }

    private static void ValidateMember(SourceGeneratorContext context, string containingTypeName, string? typeAttribute, ISymbol member, ImmutableHashSet<ISymbol> requiredMembers, List<Diagnostic> diagnostics, HashSet<string> namedKeys, SortedSet<int> tupleKeys)
    {
        const string NamedObjectAttribute = "NamedObjectAttribute";
        const string TupleObjectAttribute = "TupleObjectAttribute";

        var cancellation = context.CancellationToken;
        var converterAttribute = context.GetAttribute(member, Constants.ConverterAttributeTypeName);
        var converterCreatorAttribute = context.GetAttribute(member, Constants.ConverterCreatorAttributeTypeName);
        var namedKeyAttribute = context.GetAttribute(member, Constants.NamedKeyAttributeTypeName);
        var tupleKeyAttribute = context.GetAttribute(member, Constants.TupleKeyAttributeTypeName);
        cancellation.ThrowIfCancellationRequested();

        var memberName = member.Name;
        var hasKey = namedKeyAttribute is not null || tupleKeyAttribute is not null;
        var requiredMemberWithoutKey = hasKey is false && requiredMembers.Count is not 0 && IsRequired(member);
        if (requiredMemberWithoutKey && typeAttribute is NamedObjectAttribute)
            diagnostics.Add(Diagnostic.Create(Constants.RequireNamedKeyAttributeForRequiredMember, GetLocation(member), new object[] { memberName, containingTypeName }));
        if (requiredMemberWithoutKey && typeAttribute is TupleObjectAttribute)
            diagnostics.Add(Diagnostic.Create(Constants.RequireTupleKeyAttributeForRequiredMember, GetLocation(member), new object[] { memberName, containingTypeName }));
        cancellation.ThrowIfCancellationRequested();

        if (converterAttribute is null &&
            converterCreatorAttribute is null &&
            namedKeyAttribute is null &&
            tupleKeyAttribute is null)
            return;

        ValidateConverterAttribute(context, converterAttribute, diagnostics);
        ValidateConverterCreatorAttribute(context, converterCreatorAttribute, diagnostics);
        ValidateNamedKeyAttribute(namedKeyAttribute, diagnostics, namedKeys);
        ValidateTupleKeyAttribute(tupleKeyAttribute, diagnostics, tupleKeys);
        cancellation.ThrowIfCancellationRequested();

        var property = member as IPropertySymbol;
        if (member.IsStatic || member.DeclaredAccessibility is not Accessibility.Public)
            diagnostics.Add(Diagnostic.Create(Constants.RequirePublicInstanceMember, GetLocation(member), new object[] { memberName, containingTypeName }));
        else if (property is not null && property.IsIndexer)
            diagnostics.Add(Diagnostic.Create(Constants.RequireNotIndexer, GetLocation(member), new object[] { containingTypeName }));
        else if (property is not null && property.GetMethod?.DeclaredAccessibility is not Accessibility.Public)
            diagnostics.Add(Diagnostic.Create(Constants.RequirePublicGetter, GetLocation(member), new object[] { memberName, containingTypeName }));
        cancellation.ThrowIfCancellationRequested();

        if (converterAttribute is not null && converterCreatorAttribute is not null)
            diagnostics.Add(Diagnostic.Create(Constants.MultipleAttributesFoundOnMember, GetLocation(member), new object[] { memberName, containingTypeName }));
        if (namedKeyAttribute is not null && typeAttribute is not NamedObjectAttribute)
            diagnostics.Add(Diagnostic.Create(Constants.RequireNamedObjectAttribute, GetLocation(namedKeyAttribute), new object[] { memberName, containingTypeName }));
        if (tupleKeyAttribute is not null && typeAttribute is not TupleObjectAttribute)
            diagnostics.Add(Diagnostic.Create(Constants.RequireTupleObjectAttribute, GetLocation(tupleKeyAttribute), new object[] { memberName, containingTypeName }));
        cancellation.ThrowIfCancellationRequested();

        if (property is not null && IsReturnsByRefOrReturnsByRefReadonly(property))
            diagnostics.Add(Diagnostic.Create(Constants.RequireNotByReferenceProperty, GetLocation(member), new object[] { memberName, containingTypeName }));
        cancellation.ThrowIfCancellationRequested();

        var memberType = property?.Type ?? ((IFieldSymbol)member).Type;
        if (IsTypeInvalid(memberType))
            diagnostics.Add(Diagnostic.Create(Constants.RequireValidTypeForMember, GetLocation(member), new object[] { GetSymbolDiagnosticDisplayString(memberType), memberName, containingTypeName }));
        cancellation.ThrowIfCancellationRequested();

        if (hasKey is false && converterAttribute is not null)
            diagnostics.Add(Diagnostic.Create(Constants.RequireKeyAttributeForConverterAttribute, GetLocation(converterAttribute), new object[] { memberName, containingTypeName }));
        if (hasKey is false && converterCreatorAttribute is not null)
            diagnostics.Add(Diagnostic.Create(Constants.RequireKeyAttributeForConverterCreatorAttribute, GetLocation(converterCreatorAttribute), new object[] { memberName, containingTypeName }));
        cancellation.ThrowIfCancellationRequested();
    }
}
