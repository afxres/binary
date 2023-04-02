namespace Mikodev.Binary.SourceGeneration;

using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

public static partial class Symbols
{
    public static void ValidateType(SourceGeneratorContext context, ITypeSymbol symbol)
    {
        var converterAttribute = symbol.GetAttributes().FirstOrDefault(x => context.Equals(x.AttributeClass, Constants.ConverterAttributeTypeName));
        var converterCreatorAttribute = symbol.GetAttributes().FirstOrDefault(x => context.Equals(x.AttributeClass, Constants.ConverterCreatorAttributeTypeName));
        var namedObjectAttribute = symbol.GetAttributes().FirstOrDefault(x => context.Equals(x.AttributeClass, Constants.NamedObjectAttributeTypeName));
        var tupleObjectAttribute = symbol.GetAttributes().FirstOrDefault(x => context.Equals(x.AttributeClass, Constants.TupleObjectAttributeTypeName));

        var cancellation = context.SourceProductionContext.CancellationToken;
        var diagnostics = new List<Diagnostic>();
        var attributes = new[] { converterAttribute, converterCreatorAttribute, namedObjectAttribute, tupleObjectAttribute }
            .OfType<AttributeData>()
            .ToImmutableArray();

        ValidateConverter(context, converterAttribute, diagnostics);
        ValidateConverterCreator(context, converterCreatorAttribute, diagnostics);
        cancellation.ThrowIfCancellationRequested();

        if (attributes.Length > 1)
        {
            diagnostics.Add(Diagnostic.Create(Constants.MultipleTypeAttributesFound, GetLocation(symbol), new object[] { symbol.Name }));
        }
        else
        {
            var tupleKeys = new HashSet<int>();
            var namedKeys = new HashSet<string>();
            var typeAttribute = attributes.SingleOrDefault()?.AttributeClass;
            foreach (var i in symbol.GetMembers())
            {
                if (i is not IFieldSymbol and not IPropertySymbol)
                    continue;
                ValidateMember(context, i, typeAttribute, diagnostics, namedKeys, tupleKeys);
                cancellation.ThrowIfCancellationRequested();
            }
        }

        if (diagnostics.Count is 0)
            return;

        var error = false;
        var production = context.SourceProductionContext;
        foreach (var i in diagnostics)
        {
            if (i.Severity is DiagnosticSeverity.Error)
                error = true;
            production.ReportDiagnostic(i);
            cancellation.ThrowIfCancellationRequested();
        }

        if (error)
            throw new SourceGeneratorException();
        cancellation.ThrowIfCancellationRequested();
    }

    private static void ValidateConverter(SourceGeneratorContext context, AttributeData? attribute, List<Diagnostic> diagnostics)
    {
        if (attribute is null)
            return;
        var argument = attribute.ConstructorArguments.Single();
        if (argument.Value is not ITypeSymbol type || type.AllInterfaces.Any(x => context.Equals(x, Constants.IConverterTypeName)) is false)
            diagnostics.Add(Diagnostic.Create(Constants.RequireConverterType, GetLocation(attribute)));
        return;
    }

    private static void ValidateConverterCreator(SourceGeneratorContext context, AttributeData? attribute, List<Diagnostic> diagnostics)
    {
        if (attribute is null)
            return;
        var argument = attribute.ConstructorArguments.Single();
        if (argument.Value is not ITypeSymbol type || type.AllInterfaces.Any(x => context.Equals(x, Constants.IConverterCreatorTypeName)) is false)
            diagnostics.Add(Diagnostic.Create(Constants.RequireConverterCreatorType, GetLocation(attribute)));
        return;
    }

    private static void ValidateNamedKey(AttributeData? attribute, List<Diagnostic> diagnostics, HashSet<string> keys)
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

    private static void ValidateTupleKey(AttributeData? attribute, List<Diagnostic> diagnostics, HashSet<int> keys)
    {
        if (attribute is null)
            return;
        var key = (int)attribute.ConstructorArguments.Single().Value!;
        if (keys.Add(key) is false)
            diagnostics.Add(Diagnostic.Create(Constants.TupleKeyDuplicated, GetLocation(attribute), new object[] { key }));
        return;
    }

    private static void ValidateMember(SourceGeneratorContext context, ISymbol member, INamedTypeSymbol? typeAttribute, List<Diagnostic> diagnostics, HashSet<string> namedKeys, HashSet<int> tupleKeys)
    {
        var converterAttribute = member.GetAttributes().FirstOrDefault(x => context.Equals(x.AttributeClass, Constants.ConverterAttributeTypeName));
        var converterCreatorAttribute = member.GetAttributes().FirstOrDefault(x => context.Equals(x.AttributeClass, Constants.ConverterCreatorAttributeTypeName));
        var namedKeyAttribute = member.GetAttributes().FirstOrDefault(x => context.Equals(x.AttributeClass, Constants.NamedKeyAttributeTypeName));
        var tupleKeyAttribute = member.GetAttributes().FirstOrDefault(x => context.Equals(x.AttributeClass, Constants.TupleKeyAttributeTypeName));

        if (converterAttribute is null &&
            converterCreatorAttribute is null &&
            namedKeyAttribute is null &&
            tupleKeyAttribute is null)
            return;

        var cancellation = context.SourceProductionContext.CancellationToken;
        ValidateConverter(context, converterAttribute, diagnostics);
        ValidateConverterCreator(context, converterCreatorAttribute, diagnostics);
        ValidateNamedKey(namedKeyAttribute, diagnostics, namedKeys);
        ValidateTupleKey(tupleKeyAttribute, diagnostics, tupleKeys);
        cancellation.ThrowIfCancellationRequested();

        if (member.IsStatic || member.DeclaredAccessibility is not Accessibility.Public)
            diagnostics.Add(Diagnostic.Create(Constants.RequirePublicInstanceMember, GetLocation(member), new object[] { member.Name, member.ContainingType.Name }));
        else if (member is IPropertySymbol { IsIndexer: true })
            diagnostics.Add(Diagnostic.Create(Constants.RequireNotIndexer, GetLocation(member), new object[] { member.ContainingType.Name }));
        else if (member is IPropertySymbol property && property.GetMethod?.DeclaredAccessibility is not Accessibility.Public)
            diagnostics.Add(Diagnostic.Create(Constants.RequirePublicGetter, GetLocation(member), new object[] { member.Name, member.ContainingType.Name }));
        cancellation.ThrowIfCancellationRequested();

        if (converterAttribute is not null && converterCreatorAttribute is not null)
            diagnostics.Add(Diagnostic.Create(Constants.MultipleMemberAttributesFound, GetLocation(member), new object[] { member.Name, member.ContainingType.Name }));
        if (namedKeyAttribute is not null && context.Equals(typeAttribute, Constants.NamedObjectAttributeTypeName) is false)
            diagnostics.Add(Diagnostic.Create(Constants.RequireNamedObjectAttribute, GetLocation(namedKeyAttribute), new object[] { member.Name, member.ContainingType.Name }));
        if (tupleKeyAttribute is not null && context.Equals(typeAttribute, Constants.TupleObjectAttributeTypeName) is false)
            diagnostics.Add(Diagnostic.Create(Constants.RequireTupleObjectAttribute, GetLocation(tupleKeyAttribute), new object[] { member.Name, member.ContainingType.Name }));
        cancellation.ThrowIfCancellationRequested();
    }
}
