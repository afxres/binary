namespace Mikodev.Binary.SourceGeneration.TupleObjectTests.CustomTupleTypes;

using Mikodev.Binary.Attributes;
using System;
using System.Collections.Generic;
using Xunit;

[SourceGeneratorContext]
[SourceGeneratorInclude<TupleObjectWithInternalConstructor>]
[SourceGeneratorInclude<ValueTypeTupleObjectWithoutPublicSetter>]
public partial class EncodeOnlySourceGeneratorContext { }

[TupleObject]
class TupleObjectWithInternalConstructor
{
    [TupleKey(0)]
    public readonly int Item;

    internal TupleObjectWithInternalConstructor(int item)
    {
        this.Item = item;
    }
}

[TupleObject]
struct ValueTypeTupleObjectWithoutPublicSetter
{
    [TupleKey(0)]
    public string? Data { get; internal set; }
}

public class EncodeOnlyTests
{
    public static IEnumerable<object[]> EncodeOnlyData()
    {
        yield return new object[] { new TupleObjectWithInternalConstructor(3) };
        yield return new object[] { new ValueTypeTupleObjectWithoutPublicSetter { Data = "Item" } };
    }

    [Theory(DisplayName = "Encode Only Test")]
    [MemberData(nameof(EncodeOnlyData))]
    public void EncodeOnlyTest<T>(T source)
    {
        var generator = Generator.CreateAotBuilder().AddConverterCreators(EncodeOnlySourceGeneratorContext.ConverterCreators.Values).Build();
        var converter = generator.GetConverter<T>();
        Assert.Equal(converter.GetType().Assembly, typeof(EncodeOnlySourceGeneratorContext).Assembly);

        var generatorSecond = Generator.CreateDefault();
        var converterSecond = generatorSecond.GetConverter<T>();
        Assert.Equal(converterSecond.GetType().Assembly, typeof(IConverter).Assembly);

        var buffer = converter.Encode(source);
        var bufferSecond = converterSecond.Encode(source);
        Assert.Equal(buffer, bufferSecond);

        var a = Assert.Throws<NotSupportedException>(() => converter.Decode(new ReadOnlySpan<byte>(buffer)));
        var b = Assert.Throws<NotSupportedException>(() => converterSecond.Decode(new ReadOnlySpan<byte>(buffer)));
        var c = Assert.Throws<NotSupportedException>(() =>
        {
            var span = new ReadOnlySpan<byte>(buffer);
            _ = converter.DecodeAuto(ref span);
        });
        var d = Assert.Throws<NotSupportedException>(() =>
        {
            var span = new ReadOnlySpan<byte>(bufferSecond);
            _ = converterSecond.DecodeAuto(ref span);
        });
        var message = $"No suitable constructor found, type: {typeof(T)}";
        Assert.Equal(message, a.Message);
        Assert.Equal(message, b.Message);
        Assert.Equal(message, c.Message);
        Assert.Equal(message, d.Message);
    }
}
