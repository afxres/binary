namespace Mikodev.Binary.SourceGeneration.TupleObjectTests.SystemTupleTypes;

using Mikodev.Binary.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

[SourceGeneratorContext]
[SourceGeneratorInclude<(int, long, int, double, short, ulong, float, byte, int, int)>]
[SourceGeneratorInclude<(int, (long, int), double, short, (ulong, float, byte), int, int)>]
public partial class EmbeddedValueTupleSourceGenerationContext { }

public class EmbeddedValueTupleTests
{
    public static IEnumerable<object[]> EmbeddedValueTupleData()
    {
        yield return new object[] { (1, (2L, 3), 4.0, (short)5, (6UL, 7f, (byte)8), 9, 10), (1, 2L, 3, 4.0, (short)5, 6UL, 7f, (byte)8, 9, 10) };
    }

    [Theory(DisplayName = "Embedded Value Tuple Integration Test")]
    [MemberData(nameof(EmbeddedValueTupleData))]
    public void IntegrationTest<A, B>(A embeddedTuple, B equivalentTuple)
    {
        var creators = EmbeddedValueTupleSourceGenerationContext.ConverterCreators;
        var expectedCreatorCount = typeof(EmbeddedValueTupleSourceGenerationContext).GetCustomAttributes(false)
            .Select(x => x.GetType())
            .Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(SourceGeneratorIncludeAttribute<>))
            .Count();
        Assert.Equal(expectedCreatorCount, creators.Count);
        Assert.Contains(typeof(A), creators.Keys);
        Assert.Contains(typeof(B), creators.Keys);

        var generatorAot = Generator.CreateAotBuilder().AddConverterCreators(creators.Values).Build();
        var converterAotA = generatorAot.GetConverter<A>();
        var converterAotB = generatorAot.GetConverter<B>();
        Assert.Equal(converterAotA.GetType().Assembly, typeof(EmbeddedValueTupleSourceGenerationContext).Assembly);
        Assert.Equal(converterAotB.GetType().Assembly, typeof(EmbeddedValueTupleSourceGenerationContext).Assembly);
        var bufferAotA = converterAotA.Encode(embeddedTuple);
        var bufferAotB = converterAotB.Encode(equivalentTuple);
        Assert.Equal(bufferAotA, bufferAotB);
        Assert.Equal(converterAotA.Length, converterAotB.Length);
        var resultAotA = converterAotA.Decode(bufferAotA);
        var resultAotB = converterAotB.Decode(bufferAotB);
        Assert.Equal(resultAotA, embeddedTuple);
        Assert.Equal(resultAotB, equivalentTuple);

        var generatorJit = Generator.CreateDefault();
        var converterJitA = generatorJit.GetConverter<A>();
        var converterJitB = generatorJit.GetConverter<B>();
        var bufferJitA = converterJitA.Encode(embeddedTuple);
        var bufferJitB = converterJitB.Encode(equivalentTuple);
        Assert.Equal(bufferJitA, bufferJitB);
        Assert.Equal(bufferAotA, bufferJitA);
        Assert.Equal(bufferAotB, bufferJitB);
        Assert.Equal(converterJitA.Length, converterJitB.Length);
        Assert.Equal(converterAotA.Length, converterJitA.Length);
        Assert.Equal(converterAotB.Length, converterJitB.Length);
        var resultJitA = converterJitA.Decode(bufferJitA);
        var resultJitB = converterJitB.Decode(bufferJitB);
        Assert.Equal(resultJitA, embeddedTuple);
        Assert.Equal(resultJitB, equivalentTuple);
    }
}
