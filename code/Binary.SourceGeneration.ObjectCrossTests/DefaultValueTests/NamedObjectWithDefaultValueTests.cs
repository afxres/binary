namespace Mikodev.Binary.SourceGeneration.ObjectCrossTests.DefaultValueTests;

using Mikodev.Binary.Attributes;
using System;
using System.Collections.Generic;
using Xunit;

[SourceGeneratorContext]
[SourceGeneratorInclude<ISome>]
[SourceGeneratorInclude<IData>]
[SourceGeneratorInclude<ClassAlpha>]
[SourceGeneratorInclude<ClassBravo>]
[SourceGeneratorInclude<StructureAlpha>]
[SourceGeneratorInclude<StructureBravo>]
public partial class NamedObjectWithDefaultValueSourceGeneratorContext { }

public interface ISome
{
    string? Item { get; set; }
}

public class Some : ISome
{
    public string? Item { get; set; }
}

public interface IData
{
    int Item { get; set; }
}

public class Data : IData
{
    public int Item { get; set; }
}

public class ClassAlpha
{
    public string? Member { get; set; }
}

public class ClassBravo
{
    public int Number;

    public string? Text { get; set; }
}

public readonly struct StructureAlpha(string text)
{
    public readonly string Text = text;
}

public struct StructureBravo
{
    public int Data;
}

public class NamedObjectWithDefaultValueTests
{
    public static IEnumerable<object[]> ValueTypeData()
    {
        yield return [new StructureAlpha("Hello, Roslyn!")];
        yield return [new StructureBravo { Data = 1 }];
    }

    [Theory(DisplayName = "Value Type Named Object Default Value Test")]
    [MemberData(nameof(ValueTypeData))]
    public void ValueTypeNamedObjectDefaultValueTest<T>(T data) where T : struct
    {
        var generator = Generator.CreateAotBuilder().AddConverterCreators(NamedObjectWithDefaultValueSourceGeneratorContext.ConverterCreators.Values).Build();
        var converter = generator.GetConverter<T>();

        var generatorSecond = Generator.CreateDefault();
        var converterSecond = generatorSecond.GetConverter<T>();
        Assert.Equal(converter.GetType().Assembly, typeof(NamedObjectWithDefaultValueSourceGeneratorContext).Assembly);
        Assert.Equal(converterSecond.GetType().Assembly, typeof(IConverter).Assembly);

        var buffer = converter.Encode(data);
        var bufferSecond = converterSecond.Encode(data);
        Assert.Equal(buffer, bufferSecond);

        var bufferDefault = converter.Encode(default);
        var bufferDefaultSecond = converterSecond.Encode(default);
        Assert.NotEmpty(bufferDefault);
        Assert.NotEmpty(bufferDefaultSecond);

        var a = Assert.Throws<ArgumentException>(() => converter.Decode(Array.Empty<byte>()));
        var b = Assert.Throws<ArgumentException>(() => converterSecond.Decode(Array.Empty<byte>()));
        var message = "Not enough bytes or byte sequence invalid.";
        Assert.Null(a.ParamName);
        Assert.Null(b.ParamName);
        Assert.StartsWith(message, a.Message);
        Assert.StartsWith(message, b.Message);
    }

    public static IEnumerable<object[]> ClassData()
    {
        yield return [new ClassAlpha { Member = "Item" }];
        yield return [new ClassBravo { Number = 2, Text = "Data" }];
    }

    [Theory(DisplayName = "Reference Type Named Object Default Value Test")]
    [MemberData(nameof(ClassData))]
    public void ReferenceTypeNamedObjectDefaultValueTest<T>(T data) where T : class
    {
        var generator = Generator.CreateAotBuilder().AddConverterCreators(NamedObjectWithDefaultValueSourceGeneratorContext.ConverterCreators.Values).Build();
        var converter = generator.GetConverter<T>();

        var generatorSecond = Generator.CreateDefault();
        var converterSecond = generatorSecond.GetConverter<T>();
        Assert.Equal(converter.GetType().Assembly, typeof(NamedObjectWithDefaultValueSourceGeneratorContext).Assembly);
        Assert.Equal(converterSecond.GetType().Assembly, typeof(IConverter).Assembly);

        var buffer = converter.Encode(data);
        var bufferSecond = converterSecond.Encode(data);
        Assert.Equal(buffer, bufferSecond);

        var bufferDefault = converter.Encode(default);
        var bufferDefaultSecond = converterSecond.Encode(default);
        Assert.Empty(bufferDefault);
        Assert.Empty(bufferDefaultSecond);

        var resultEmpty = converter.Decode(Array.Empty<byte>());
        var resultEmptySecond = converterSecond.Decode(Array.Empty<byte>());
        Assert.Null(resultEmpty);
        Assert.Null(resultEmptySecond);
    }

    public static IEnumerable<object[]> InterfaceData()
    {
        yield return [typeof(ISome), new Some { Item = "None" }];
        yield return [typeof(IData), new Data { Item = 0x1234 }];
    }

    [Theory(DisplayName = "Interface Type Named Object Default Value Test")]
    [MemberData(nameof(InterfaceData))]
    public void InterfaceTypeNamedObjectDefaultValueTest<T>(Type wanted, T data)
    {
        var method = new Action<object>(ReferenceTypeNamedObjectDefaultValueTest).Method.GetGenericMethodDefinition().MakeGenericMethod(wanted);
        var result = method.Invoke(this, parameters: [data]);
        Assert.Null(result);
    }
}
