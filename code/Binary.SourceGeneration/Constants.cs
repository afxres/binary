namespace Mikodev.Binary.SourceGeneration;

using Microsoft.CodeAnalysis;

public static class Constants
{
    public const string SourceGeneratorContextAttributeTypeName = "Mikodev.Binary.Attributes.SourceGeneratorContextAttribute";

    public const string SourceGeneratorIncludeAttributeTypeName = "Mikodev.Binary.Attributes.SourceGeneratorIncludeAttribute`1";

    public const string NamedObjectAttributeTypeName = "Mikodev.Binary.Attributes.NamedObjectAttribute";

    public const string TupleObjectAttributeTypeName = "Mikodev.Binary.Attributes.TupleObjectAttribute";

    public const string NamedKeyAttributeTypeName = "Mikodev.Binary.Attributes.NamedKeyAttribute";

    public const string TupleKeyAttributeTypeName = "Mikodev.Binary.Attributes.TupleKeyAttribute";

    public const string AllocatorTypeName = "Mikodev.Binary.Allocator";

    public const string ConverterTypeName = "Mikodev.Binary.Converter";

    public const string IConverterTypeName = "Mikodev.Binary.IConverter";

    public const string IConverterCreatorTypeName = "Mikodev.Binary.IConverterCreator";

    public const string IGeneratorContextTypeName = "Mikodev.Binary.IGeneratorContext";

    public const string IEnumerableTypeName = "System.Collections.Generic.IEnumerable`1";

    public const string EqualityComparerTypeName = "System.Collections.Generic.EqualityComparer";

    public const string ConverterAttributeTypeName = "Mikodev.Binary.Attributes.ConverterAttribute";

    public const string ConverterCreatorAttributeTypeName = "Mikodev.Binary.Attributes.ConverterCreatorAttribute";

    public const string DiagnosticCategory = "SourceGeneration";

    public static DiagnosticDescriptor ContextTypeNotPartial { get; } = new DiagnosticDescriptor(
        id: "BINSRCGEN01",
        title: "Source Generator Context Type Not Partial.",
        messageFormat: "Require 'partial' keyword for source generator context, type: {0}",
        category: DiagnosticCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor ContextTypeNotInNamespace { get; } = new DiagnosticDescriptor(
        id: "BINSRCGEN02",
        title: "Source Generator Context Type Not In Namespace.",
        messageFormat: "Require not global namespace for source generator context, type: {0}",
        category: DiagnosticCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor ContextTypeNested { get; } = new DiagnosticDescriptor(
        id: "BINSRCGEN03",
        title: "Source Generator Context Type Is Nested.",
        messageFormat: "Require not nested type for source generator context, type: {0}",
        category: DiagnosticCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor ContextTypeGeneric { get; } = new DiagnosticDescriptor(
        id: "BINSRCGEN04",
        title: "Source Generator Context Type Is Generic.",
        messageFormat: "Require not generic type for source generator context, type: {0}",
        category: DiagnosticCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor IncludeTypeDuplicated { get; } = new DiagnosticDescriptor(
        id: "BINSRCGEN11",
        title: "Type Inclusion Duplicated.",
        messageFormat: "Type inclusion duplicated, type: {0}",
        category: DiagnosticCategory,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor NoConverterGenerated { get; } = new DiagnosticDescriptor(
        id: "BINSRCGEN12",
        title: "No Converter Generated.",
        messageFormat: "No converter generated, type: {0}",
        category: DiagnosticCategory,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor NamedKeyDuplicated { get; } = new DiagnosticDescriptor(
        id: "BINSRCGEN21",
        title: "Named Key Duplicated.",
        messageFormat: "Named key duplicated, key: {0}",
        category: DiagnosticCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor NamedKeyNullOrEmpty { get; } = new DiagnosticDescriptor(
        id: "BINSRCGEN22",
        title: "Named Key Null Or Empty.",
        messageFormat: "Named key can not be null or empty.",
        category: DiagnosticCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor TupleKeyDuplicated { get; } = new DiagnosticDescriptor(
        id: "BINSRCGEN31",
        title: "Tuple Key Duplicated.",
        messageFormat: "Tuple key duplicated, key: {0}",
        category: DiagnosticCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor NoAvailableMemberFound { get; } = new DiagnosticDescriptor(
        id: "BINSRCGEN41",
        title: "No Available Member Found.",
        messageFormat: "No available member found, type: {0}",
        category: DiagnosticCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor MultipleTypeAttributesFound { get; } = new DiagnosticDescriptor(
        id: "BINSRCGEN42",
        title: "Multiple Attributes Found.",
        messageFormat: "Multiple attributes found, type: {0}",
        category: DiagnosticCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor MultipleMemberAttributesFound { get; } = new DiagnosticDescriptor(
        id: "BINSRCGEN43",
        title: "Multiple Attributes Found.",
        messageFormat: "Multiple attributes found, member name: {0}, type: {1}",
        category: DiagnosticCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor RequireConverterType { get; } = new DiagnosticDescriptor(
        id: "BINSRCGEN44",
        title: "Require Converter Type.",
        messageFormat: "Require converter type.",
        category: DiagnosticCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor RequireConverterCreatorType { get; } = new DiagnosticDescriptor(
        id: "BINSRCGEN45",
        title: "Require Converter Creator Type.",
        messageFormat: "Require converter creator type.",
        category: DiagnosticCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}
