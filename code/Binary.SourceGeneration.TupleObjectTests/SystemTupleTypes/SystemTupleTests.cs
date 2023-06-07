namespace Mikodev.Binary.SourceGeneration.TupleObjectTests.SystemTupleTypes;

using Mikodev.Binary.Attributes;
using System;
using System.Collections.Generic;
using Xunit;

[SourceGeneratorContext]
[SourceGeneratorInclude<Tuple<short, int>>]
[SourceGeneratorInclude<Tuple<int, string>>]
[SourceGeneratorInclude<Tuple<string, int, double>>]
[SourceGeneratorInclude<ValueTuple<int, long>>]
[SourceGeneratorInclude<ValueTuple<int, string>>]
[SourceGeneratorInclude<ValueTuple<string, int, double>>]
public partial class SystemTupleSourceGeneratorContext { }

public class SystemTupleTests
{
    public static IEnumerable<object[]> TupleData()
    {
        yield return new object[] { Tuple.Create((short)1, 2), 6 };
        yield return new object[] { Tuple.Create(4096, "String"), 0 };
        yield return new object[] { Tuple.Create("First", 2, 3.0), 0 };
    }

    public static IEnumerable<object[]> ValueTupleData()
    {
        yield return new object[] { (1, 2L), 12 };
        yield return new object[] { (4096, "String"), 0 };
        yield return new object[] { ("First", 2, 3.0), 0 };
    }

    [Theory(DisplayName = "System Tuple Integration Test")]
    [MemberData(nameof(TupleData))]
    [MemberData(nameof(ValueTupleData))]
    public void IntegrationTest<T>(T source, int converterLength)
    {
        var generator = Generator.CreateAotBuilder()
            .AddConverterCreators(SystemTupleSourceGeneratorContext.ConverterCreators.Values)
            .Build();
        var converter = generator.GetConverter<T>();
        Assert.Equal(typeof(SystemTupleSourceGeneratorContext).Assembly, converter.GetType().Assembly);
        Assert.Equal(converterLength, converter.Length);
        var buffer = converter.Encode(source);
        var result = converter.Decode(buffer);
        Assert.Equal(source, result);

        var allocator = new Allocator();
        converter.EncodeAuto(ref allocator, source);
        var intent = new ReadOnlySpan<byte>(allocator.ToArray());
        var decode = converter.DecodeAuto(ref intent);
        Assert.Equal(source, decode);
        Assert.Equal(0, intent.Length);
    }
}
