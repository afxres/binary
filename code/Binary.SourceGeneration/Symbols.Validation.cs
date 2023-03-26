namespace Mikodev.Binary.SourceGeneration;

using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

public static partial class Symbols
{
    public static void ValidateType(SourceGeneratorContext context, ITypeSymbol symbol)
    {
        var attributeNames = new[]
        {
            Constants.ConverterAttributeTypeName,
            Constants.ConverterCreatorAttributeTypeName,
            Constants.NamedObjectAttributeTypeName,
            Constants.TupleObjectAttributeTypeName
        };

        var cancellation = context.SourceProductionContext.CancellationToken;
        var diagnostics = new List<Diagnostic>();
        var attributes = GetAttributes(context, symbol, attributeNames);
        if (attributes.Length > 1)
            diagnostics.Add(Diagnostic.Create(Constants.MultipleTypeAttributesFound, GetLocation(symbol), new object[] { symbol.Name }));

        foreach (var i in symbol.GetMembers())
        {
            if (i is not IFieldSymbol and not IPropertySymbol)
                continue;
            ValidateMember(context, i, diagnostics);
            cancellation.ThrowIfCancellationRequested();
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

    public static void ValidateMember(SourceGeneratorContext context, ISymbol member, List<Diagnostic> diagnostics)
    {
        var attributeNames = new[]
        {
            Constants.ConverterAttributeTypeName,
            Constants.ConverterCreatorAttributeTypeName,
            Constants.NamedKeyAttributeTypeName,
            Constants.TupleKeyAttributeTypeName
        };

        var attributes = GetAttributes(context, member, attributeNames);
        if (attributes.Length is 0)
            return;

        var cancellation = context.SourceProductionContext.CancellationToken;
        if (member.IsStatic || member.DeclaredAccessibility is not Accessibility.Public)
            diagnostics.Add(Diagnostic.Create(Constants.RequirePublicInstanceMember, GetLocation(member), new object[] { member.Name, member.ContainingType.Name }));
        else if (member is IPropertySymbol { IsIndexer: true })
            diagnostics.Add(Diagnostic.Create(Constants.RequireNotIndexer, GetLocation(member), new object[] { member.ContainingType.Name }));
        else if (member is IPropertySymbol property && property.GetMethod?.DeclaredAccessibility is not Accessibility.Public)
            diagnostics.Add(Diagnostic.Create(Constants.RequirePublicGetter, GetLocation(member), new object[] { member.Name, member.ContainingType.Name }));
        cancellation.ThrowIfCancellationRequested();

        var converterAttribute = attributes.FirstOrDefault(x => context.Equals(x.AttributeClass, Constants.ConverterAttributeTypeName));
        var converterCreatorAttribute = attributes.FirstOrDefault(x => context.Equals(x.AttributeClass, Constants.ConverterCreatorAttributeTypeName));
        if (converterAttribute is not null && converterCreatorAttribute is not null)
            diagnostics.Add(Diagnostic.Create(Constants.MultipleMemberAttributesFound, GetLocation(member), new object[] { member.Name, member.ContainingType.Name }));
        cancellation.ThrowIfCancellationRequested();
    }
}
