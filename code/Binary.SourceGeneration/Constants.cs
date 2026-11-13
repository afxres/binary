namespace Mikodev.Binary.SourceGeneration;

using System.Collections.Immutable;

public static class Constants
{
    public const string SourceGeneratorContextAttributeTypeName = "Mikodev.Binary.Attributes.SourceGeneratorContextAttribute";

    public const string SourceGeneratorIncludeAttributeTypeName = "Mikodev.Binary.Attributes.SourceGeneratorIncludeAttribute`1";

    public const string NamedObjectAttributeTypeName = "Mikodev.Binary.Attributes.NamedObjectAttribute";

    public const string TupleObjectAttributeTypeName = "Mikodev.Binary.Attributes.TupleObjectAttribute";

    public const string NamedKeyAttributeTypeName = "Mikodev.Binary.Attributes.NamedKeyAttribute";

    public const string TupleKeyAttributeTypeName = "Mikodev.Binary.Attributes.TupleKeyAttribute";

    public const string IConverterTypeName = "Mikodev.Binary.IConverter";

    public const string IConverterCreatorTypeName = "Mikodev.Binary.IConverterCreator";

    public const string ConverterAttributeTypeName = "Mikodev.Binary.Attributes.ConverterAttribute";

    public const string ConverterCreatorAttributeTypeName = "Mikodev.Binary.Attributes.ConverterCreatorAttribute";

    public static ImmutableArray<string> SystemTupleMemberNames { get; } = ["Item1", "Item2", "Item3", "Item4", "Item5", "Item6", "Item7", "Rest"];
}
