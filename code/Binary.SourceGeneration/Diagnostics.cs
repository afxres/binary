namespace Mikodev.Binary.SourceGeneration;

using Microsoft.CodeAnalysis;

public static class Diagnostics
{
    public const string DiagnosticCategory = "SourceGeneration";

    // ↓ source generator context

    public static DiagnosticDescriptor ContextTypeNotPartial { get; } = new DiagnosticDescriptor(
        id: "BINSRCGEN01",
        title: "Source Generator Context Type Is Not Partial",
        messageFormat: "The 'partial' keyword is required for the source generator context, type: {0}",
        category: DiagnosticCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor ContextTypeInGlobalNamespace { get; } = new DiagnosticDescriptor(
        id: "BINSRCGEN02",
        title: "Source Generator Context Type Is in the Global Namespace",
        messageFormat: "The source generator context must not be in the global namespace, type: {0}",
        category: DiagnosticCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor ContextTypeNested { get; } = new DiagnosticDescriptor(
        id: "BINSRCGEN03",
        title: "Source Generator Context Type Is Nested",
        messageFormat: "The source generator context must not be a nested type, type: {0}",
        category: DiagnosticCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor ContextTypeGeneric { get; } = new DiagnosticDescriptor(
        id: "BINSRCGEN04",
        title: "Source Generator Context Type Is Generic",
        messageFormat: "The source generator context must not be a generic type, type: {0}",
        category: DiagnosticCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor ContextTypeRecord { get; } = new DiagnosticDescriptor(
        id: "BINSRCGEN05",
        title: "Source Generator Context Type Is a Record",
        messageFormat: "The source generator context must not be a record type, type: {0}",
        category: DiagnosticCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    // ↑ source generator context

    // ↓ include

    public static DiagnosticDescriptor TypeInclusionDuplicated { get; } = new DiagnosticDescriptor(
        id: "BINSRCGEN11",
        title: "Type Inclusion Is Duplicated",
        messageFormat: "The type is included more than once: {0}",
        category: DiagnosticCategory,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor NoConverterGenerated { get; } = new DiagnosticDescriptor(
        id: "BINSRCGEN12",
        title: "No Converter Generated",
        messageFormat: "The converter could not be generated because the type may have been explicitly excluded, type: {0}",
        category: DiagnosticCategory,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor TypeNotRecognized { get; } = new DiagnosticDescriptor(
        id: "BINSRCGEN13",
        title: "Type Not Recognized",
        messageFormat: "The converter could not be generated because the type could not be identified, pattern: {0}, type: {1}",
        category: DiagnosticCategory,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    // ↑ include

    // ↓ not valid

    public static DiagnosticDescriptor InvalidTypeForIncludeAttribute { get; } = new DiagnosticDescriptor(
        id: "BINSRCGEN21",
        title: "Invalid Type",
        messageFormat: "A valid type is required (array, class, enum, interface, or struct), type: {0}",
        category: DiagnosticCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor InvalidTypeForMember { get; } = new DiagnosticDescriptor(
        id: "BINSRCGEN22",
        title: "Invalid Type",
        messageFormat: "A valid type is required (array, class, enum, interface, or struct), type: {0}, member name: {1}, containing type: {2}",
        category: DiagnosticCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor SelfTypeReferenceFound { get; } = new DiagnosticDescriptor(
        id: "BINSRCGEN23",
        title: "Self-Type Reference Found",
        messageFormat: "A self-type reference was found, member name: {0}, containing type: {1}",
        category: DiagnosticCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    // ↑ not valid

    // ↓ member

    public static DiagnosticDescriptor NoAvailableMemberFound { get; } = new DiagnosticDescriptor(
        id: "BINSRCGEN31",
        title: "No Available Member Found",
        messageFormat: "No available member was found, type: {0}",
        category: DiagnosticCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor AmbiguousMemberFound { get; } = new DiagnosticDescriptor(
        id: "BINSRCGEN32",
        title: "Ambiguous Member Found",
        messageFormat: "An ambiguous member was found, member name: {0}, type: {1}",
        category: DiagnosticCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor RequirePublicInstanceMember { get; } = new DiagnosticDescriptor(
        id: "BINSRCGEN33",
        title: "Public Instance Member Required",
        messageFormat: "A public instance member is required, member name: {0}, containing type: {1}",
        category: DiagnosticCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor IndexerNotAllowed { get; } = new DiagnosticDescriptor(
        id: "BINSRCGEN34",
        title: "Indexer Not Allowed",
        messageFormat: "An indexer is not allowed, containing type: {0}",
        category: DiagnosticCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor PublicGetterRequired { get; } = new DiagnosticDescriptor(
        id: "BINSRCGEN35",
        title: "Public Getter Required",
        messageFormat: "A public getter is required, member name: {0}, containing type: {1}",
        category: DiagnosticCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor ByReferencePropertyNotAllowed { get; } = new DiagnosticDescriptor(
        id: "BINSRCGEN36",
        title: "By-Reference Property Not Allowed",
        messageFormat: "A property must not be passed by reference, member name: {0}, containing type: {1}",
        category: DiagnosticCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    // ↑ member

    // ↓ keys

    public static DiagnosticDescriptor NamedKeyDuplicated { get; } = new DiagnosticDescriptor(
        id: "BINSRCGEN41",
        title: "Named Key Is Duplicated",
        messageFormat: "The named key is duplicated: {0}",
        category: DiagnosticCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor TupleKeyDuplicated { get; } = new DiagnosticDescriptor(
        id: "BINSRCGEN42",
        title: "Tuple Key Is Duplicated",
        messageFormat: "The tuple key is duplicated: {0}",
        category: DiagnosticCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor NamedKeyIsNullOrEmpty { get; } = new DiagnosticDescriptor(
        id: "BINSRCGEN43",
        title: "Named Key Is Null or Empty",
        messageFormat: "A named key cannot be null or empty.",
        category: DiagnosticCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor TupleKeyIsNotSequential { get; } = new DiagnosticDescriptor(
        id: "BINSRCGEN44",
        title: "Tuple Key Is Not Sequential",
        messageFormat: "Tuple keys must start at zero and be sequential, type: {0}",
        category: DiagnosticCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    // ↑ keys

    // ↓ attributes

    public static DiagnosticDescriptor MultipleAttributesFoundOnType { get; } = new DiagnosticDescriptor(
        id: "BINSRCGEN51",
        title: "Multiple Attributes Found",
        messageFormat: "Multiple attributes were found, type: {0}",
        category: DiagnosticCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor MultipleAttributesFoundOnMember { get; } = new DiagnosticDescriptor(
        id: "BINSRCGEN52",
        title: "Multiple Attributes Found",
        messageFormat: "Multiple attributes were found, member name: {0}, containing type: {1}",
        category: DiagnosticCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor NamedObjectAttributeRequired { get; } = new DiagnosticDescriptor(
        id: "BINSRCGEN53",
        title: "'NamedObjectAttribute' Required",
        messageFormat: "A 'NamedObjectAttribute' is required for 'NamedKeyAttribute', member name: {0}, containing type: {1}",
        category: DiagnosticCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor TupleObjectAttributeRequired { get; } = new DiagnosticDescriptor(
        id: "BINSRCGEN54",
        title: "'TupleObjectAttribute' Required",
        messageFormat: "A 'TupleObjectAttribute' is required for 'TupleKeyAttribute', member name: {0}, containing type: {1}",
        category: DiagnosticCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor ConverterTypeRequired { get; } = new DiagnosticDescriptor(
        id: "BINSRCGEN55",
        title: "Converter Type Required",
        messageFormat: "A converter type is required.",
        category: DiagnosticCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor ConverterCreatorTypeRequired { get; } = new DiagnosticDescriptor(
        id: "BINSRCGEN56",
        title: "Converter Creator Type Required",
        messageFormat: "A converter creator type is required.",
        category: DiagnosticCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor KeyAttributeRequiredForConverterAttribute { get; } = new DiagnosticDescriptor(
        id: "BINSRCGEN57",
        title: "Key Attribute Required for Converter Attribute",
        messageFormat: "A 'NamedKeyAttribute' or 'TupleKeyAttribute' is required for 'ConverterAttribute', member name: {0}, containing type: {1}",
        category: DiagnosticCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor KeyAttributeRequiredForConverterCreatorAttribute { get; } = new DiagnosticDescriptor(
        id: "BINSRCGEN58",
        title: "Key Attribute Required for Converter Creator Attribute",
        messageFormat: "A 'NamedKeyAttribute' or 'TupleKeyAttribute' is required for 'ConverterCreatorAttribute', member name: {0}, containing type: {1}",
        category: DiagnosticCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor NamedKeyAttributeRequiredForRequiredMember { get; } = new DiagnosticDescriptor(
        id: "BINSRCGEN59",
        title: "'NamedKeyAttribute' Required for Required Member",
        messageFormat: "The required member must have a 'NamedKeyAttribute', member name: {0}, containing type: {1}",
        category: DiagnosticCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor TupleKeyAttributeRequiredForRequiredMember { get; } = new DiagnosticDescriptor(
        id: "BINSRCGEN60",
        title: "'TupleKeyAttribute' Required for Required Member",
        messageFormat: "The required member must have a 'TupleKeyAttribute', member name: {0}, containing type: {1}",
        category: DiagnosticCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    // ↑ attributes

    // ↓ union

    public static DiagnosticDescriptor UnionCaseTypeAmbiguous { get; } = new DiagnosticDescriptor(
        id: "BINSRCGEN71",
        title: "Union Case Type Is Ambiguous",
        messageFormat: "The converter could not be generated because the nullable case type shares one boxed representation with another case type, type: {0}",
        category: DiagnosticCategory,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static DiagnosticDescriptor UnionCaseFactoryInherited { get; } = new DiagnosticDescriptor(
        id: "BINSRCGEN72",
        title: "Union Case Factory Is Inherited",
        messageFormat: "The converter could not be generated because the union case factory is inherited from a base interface, type: {0}",
        category: DiagnosticCategory,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    // ↑ union
}
