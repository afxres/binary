namespace Mikodev.Binary.SourceGeneration.TupleObjectTests.DefaultValueTests;

using Mikodev.Binary.Attributes;
using System;
using System.Collections.Generic;
using Xunit;

[SourceGeneratorContext]
[SourceGeneratorInclude<Tuple<int>>]
[SourceGeneratorInclude<TupleAlpha<string>>]
partial class EncodeNullValueSourceGeneratorContext { }

[TupleObject]
class TupleAlpha<T>
{
    [TupleKey(0)]
    public T? Data;
}

public class EncodeNullValueTests
{
    public static IEnumerable<object[]> TupleData()
    {
        yield return new object[] { new Tuple<int>(7) };
        yield return new object[] { new TupleAlpha<string> { Data = "11" } };
    }

    [Theory(DisplayName = "Encode Null Value Test")]
    [MemberData(nameof(TupleData))]
    public void EncodeNullValueTest<T>(T source) where T : class
    {
        var generator = Generator.CreateAotBuilder()
            .AddConverterCreators(EncodeNullValueSourceGeneratorContext.ConverterCreators.Values)
            .Build();
        var converter = generator.GetConverter<T>();
        var generatorSecond = Generator.CreateDefault();
        var converterSecond = generatorSecond.GetConverter<T>();
        Assert.Equal(converter.GetType().Assembly, typeof(EncodeNullValueSourceGeneratorContext).Assembly);
        Assert.Equal(converterSecond.GetType().Assembly, typeof(IConverter).Assembly);

        var buffer = converter.Encode(source);
        var bufferSecond = converterSecond.Encode(source);
        Assert.Equal(buffer, bufferSecond);

        var a = Assert.Throws<ArgumentException>(() => converter.Encode(null));
        var b = Assert.Throws<ArgumentException>(() => converterSecond.Encode(null));
        var c = Assert.Throws<ArgumentException>(() =>
        {
            var allocator = new Allocator();
            converter.EncodeAuto(ref allocator, null);
        });
        var d = Assert.Throws<ArgumentException>(() =>
        {
            var allocator = new Allocator();
            converterSecond.EncodeAuto(ref allocator, null);
        });

        var message = $"Tuple can not be null, type: {typeof(T)}";
        var exceptions = new[] { a, b, c, d };
        Assert.All(exceptions, x => Assert.Null(x.ParamName));
        Assert.All(exceptions, x => Assert.StartsWith(message, x.Message));
    }
}
