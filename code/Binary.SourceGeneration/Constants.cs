namespace Mikodev.Binary.SourceGeneration;

using Microsoft.CodeAnalysis;

public static class Constants
{
    public const string GlobalNamespace = "global::";

    public const string LambdaIdFunction = "static x => x";

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

    public const string ConverterAttributeTypeName = "Mikodev.Binary.Attributes.ConverterAttribute";

    public const string ConverterCreatorAttributeTypeName = "Mikodev.Binary.Attributes.ConverterCreatorAttribute";

    public const string DiagnosticCategory = "SourceGeneration";

    // ↓ source generator context

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

    // ↑ source generator context

    // ↓ include

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

    // ↑ include

    // ↓ not supported

    public static DiagnosticDescriptor RequireSupportedTypeForIncludeAttribute { get; } = new DiagnosticDescriptor(
        id: "BINSRCGEN21",
        title: "Require Supported Type.",
        messageFormat: "Require supported type (array, class, enum, interface or struct), type: {0}",
        category: DiagnosticCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor RequireSupportedTypeForMember { get; } = new DiagnosticDescriptor(
        id: "BINSRCGEN22",
        title: "Require Supported Type.",
        messageFormat: "Require supported type (array, class, enum, interface or struct), type: {0}, member name: {1}, containing type: {2}",
        category: DiagnosticCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    // ↑ not supported

    // ↓ member

    public static DiagnosticDescriptor NoAvailableMemberFound { get; } = new DiagnosticDescriptor(
        id: "BINSRCGEN31",
        title: "No Available Member Found.",
        messageFormat: "No available member found, type: {0}",
        category: DiagnosticCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor RequirePublicInstanceMember { get; } = new DiagnosticDescriptor(
        id: "BINSRCGEN32",
        title: "Require Public Instance Member.",
        messageFormat: "Require public instance member, member name: {0}, containing type: {1}",
        category: DiagnosticCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor RequireNotIndexer { get; } = new DiagnosticDescriptor(
        id: "BINSRCGEN33",
        title: "Require Not Indexer.",
        messageFormat: "Require not an indexer, containing type: {0}",
        category: DiagnosticCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor RequirePublicGetter { get; } = new DiagnosticDescriptor(
        id: "BINSRCGEN34",
        title: "Require Public Getter.",
        messageFormat: "Require a public getter, member name: {0}, containing type: {1}",
        category: DiagnosticCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor RequireNotByReferenceProperty { get; } = new DiagnosticDescriptor(
        id: "BINSRCGEN35",
        title: "Require Not By Reference Property.",
        messageFormat: "Require not by reference property, member name: {0}, containing type: {1}",
        category: DiagnosticCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    // ↑ member

    // ↓ keys

    public static DiagnosticDescriptor NamedKeyDuplicated { get; } = new DiagnosticDescriptor(
        id: "BINSRCGEN41",
        title: "Named Key Duplicated.",
        messageFormat: "Named key duplicated, key: {0}",
        category: DiagnosticCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor TupleKeyDuplicated { get; } = new DiagnosticDescriptor(
        id: "BINSRCGEN42",
        title: "Tuple Key Duplicated.",
        messageFormat: "Tuple key duplicated, key: {0}",
        category: DiagnosticCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor NamedKeyNullOrEmpty { get; } = new DiagnosticDescriptor(
        id: "BINSRCGEN43",
        title: "Named Key Null Or Empty.",
        messageFormat: "Named key can not be null or empty.",
        category: DiagnosticCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor TupleKeyNotSequential { get; } = new DiagnosticDescriptor(
        id: "BINSRCGEN44",
        title: "Tuple Key Not Sequential.",
        messageFormat: "Tuple key must start at zero and must be sequential, type: {0}",
        category: DiagnosticCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    // ↑ keys

    // ↓ attributes

    public static DiagnosticDescriptor MultipleAttributesFoundOnType { get; } = new DiagnosticDescriptor(
        id: "BINSRCGEN51",
        title: "Multiple Attributes Found.",
        messageFormat: "Multiple attributes found, type: {0}",
        category: DiagnosticCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor MultipleAttributesFoundOnMember { get; } = new DiagnosticDescriptor(
        id: "BINSRCGEN52",
        title: "Multiple Attributes Found.",
        messageFormat: "Multiple attributes found, member name: {0}, containing type: {1}",
        category: DiagnosticCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor RequireNamedObjectAttribute { get; } = new DiagnosticDescriptor(
        id: "BINSRCGEN53",
        title: "Require 'NamedObjectAttribute'.",
        messageFormat: "Require 'NamedObjectAttribute' for 'NamedKeyAttribute', member name: {0}, containing type: {1}",
        category: DiagnosticCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor RequireTupleObjectAttribute { get; } = new DiagnosticDescriptor(
        id: "BINSRCGEN54",
        title: "Require 'TupleObjectAttribute'.",
        messageFormat: "Require 'TupleObjectAttribute' for 'TupleKeyAttribute', member name: {0}, containing type: {1}",
        category: DiagnosticCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor RequireConverterType { get; } = new DiagnosticDescriptor(
        id: "BINSRCGEN55",
        title: "Require Converter Type.",
        messageFormat: "Require converter type.",
        category: DiagnosticCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor RequireConverterCreatorType { get; } = new DiagnosticDescriptor(
        id: "BINSRCGEN56",
        title: "Require Converter Creator Type.",
        messageFormat: "Require converter creator type.",
        category: DiagnosticCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor RequireKeyAttributeForConverterAttribute { get; } = new DiagnosticDescriptor(
        id: "BINSRCGEN57",
        title: "Require Key Attribute For Converter Attribute.",
        messageFormat: "Require 'NamedKeyAttribute' or 'TupleKeyAttribute' for 'ConverterAttribute', member name: {0}, containing type: {1}",
        category: DiagnosticCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor RequireKeyAttributeForConverterCreatorAttribute { get; } = new DiagnosticDescriptor(
        id: "BINSRCGEN58",
        title: "Require Key Attribute For Converter Creator Attribute.",
        messageFormat: "Require 'NamedKeyAttribute' or 'TupleKeyAttribute' for 'ConverterCreatorAttribute', member name: {0}, containing type: {1}",
        category: DiagnosticCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    // ↑ attributes
}
