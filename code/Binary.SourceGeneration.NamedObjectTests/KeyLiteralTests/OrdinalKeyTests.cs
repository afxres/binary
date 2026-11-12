namespace Mikodev.Binary.SourceGeneration.NamedObjectTests.KeyLiteralTests;

using Mikodev.Binary;
using Mikodev.Binary.Attributes;
using Xunit;

[SourceGeneratorContext]
[SourceGeneratorInclude<ClassWithKeysDistinctByOrdinal>]
public partial class OrdinalKeyGeneratorContext { }

[NamedObject]
public class ClassWithKeysDistinctByOrdinal
{
    // equal under culture-aware comparison, distinct under ordinal comparison
    [NamedKey("\u00E9")]
    public int Composed { get; init; }

    [NamedKey("e\u0301")]
    public int Decomposed { get; init; }
}

public class OrdinalKeyTests
{
    [Fact(DisplayName = "Keys Distinct Only By Ordinal")]
    public void KeysDistinctOnlyByOrdinal()
    {
        var generatorAot = Generator.CreateAotBuilder().AddConverterCreators(OrdinalKeyGeneratorContext.ConverterCreators.Values).Build();
        var generatorDefault = Generator.CreateDefault();
        var source = new ClassWithKeysDistinctByOrdinal { Composed = 1024, Decomposed = 2048 };
        var converterAot = generatorAot.GetConverter<ClassWithKeysDistinctByOrdinal>();
        var converterDefault = generatorDefault.GetConverter<ClassWithKeysDistinctByOrdinal>();
        var bufferAot = converterAot.Encode(source);
        var bufferDefault = converterDefault.Encode(source);
        Assert.Equal(bufferAot, bufferDefault);

        var result = converterDefault.Decode(bufferDefault);
        Assert.Equal(1024, result.Composed);
        Assert.Equal(2048, result.Decomposed);
    }
}
