namespace Mikodev.Binary.SourceGeneration.TupleObjectTests.DefaultValueTests;

using Mikodev.Binary.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
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

        var a = Assert.Throws<ArgumentNullException>(() => converter.Encode(null));
        var b = Assert.Throws<ArgumentNullException>(() => converterSecond.Encode(null));
        var c = Assert.Throws<ArgumentNullException>(() =>
        {
            var allocator = new Allocator();
            converter.EncodeAuto(ref allocator, null);
        });
        var d = Assert.Throws<ArgumentNullException>(() =>
        {
            var allocator = new Allocator();
            converterSecond.EncodeAuto(ref allocator, null);
        });

        var h = Converter.GetMethod(converter, "Encode");
        var i = Converter.GetMethod(converterSecond, "Encode");
        var j = Converter.GetMethod(converter, "EncodeAuto");
        var k = Converter.GetMethod(converterSecond, "EncodeAuto");
        var message = $"Tuple can not be null, type: {typeof(T)}";
        var exceptions = new[] { a, b, c, d };
        var parameters = new[] { h, i, j, k }.Select(x => x.GetParameters()[1]);

        Assert.All(parameters, x => Assert.Equal("item", x.Name));
        Assert.All(exceptions, x => Assert.Equal("item", x.ParamName));
        Assert.All(exceptions, x => Assert.StartsWith(message, x.Message));
    }
}
