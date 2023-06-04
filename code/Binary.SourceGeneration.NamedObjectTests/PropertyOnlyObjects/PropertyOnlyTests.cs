namespace Mikodev.Binary.SourceGeneration.NamedObjectTests.PropertyOnlyObjects;

using Mikodev.Binary.Attributes;
using System;
using System.Collections.Generic;
using Xunit;

[SourceGeneratorContext]
[SourceGeneratorInclude<Equipment>]
[SourceGeneratorInclude<ValueItem>]
public partial class PropertyOnlySourceGeneratorContext { }

[NamedObject]
public class Equipment : IEquatable<Equipment?>
{
    [NamedKey("id")]
    public required int Id { get; init; }

    [NamedKey("name")]
    public required string Name { get; init; }

    public bool Equals(Equipment? other) => other is not null && Id == other.Id && Name == other.Name;

    public override bool Equals(object? obj) => Equals(obj as Equipment);

    public override int GetHashCode() => HashCode.Combine(Id, Name);
}

[NamedObject]
public readonly struct ValueItem : IEquatable<ValueItem>
{
    [NamedKey("tag")]
    public required long Tag { get; init; }

    [NamedKey("content")]
    public required string Content { get; init; }

    public bool Equals(ValueItem other) => Tag == other.Tag && Content == other.Content;

    public override bool Equals(object? obj) => obj is ValueItem item && Equals(item);

    public override int GetHashCode() => HashCode.Combine(Tag, Content);
}

public class PropertyOnlyTests
{
    public static IEnumerable<object[]> ClassData()
    {
        yield return new object[] { new Equipment { Id = 886, Name = "十分厚重的双语词典" } };
        yield return new object[] { new Equipment { Id = 686, Name = "棱角分明的百科全书" } };
    }

    public static IEnumerable<object[]> ValueData()
    {
        yield return new object[] { new ValueItem { Tag = 22, Content = "苹果" } };
        yield return new object[] { new ValueItem { Tag = 14, Content = "Player" } };
    }

    [Theory(DisplayName = "Encode Decode Test")]
    [MemberData(nameof(ClassData))]
    [MemberData(nameof(ValueData))]
    public void EncodeDecodeTest<T>(T source)
    {
        var builder = Generator.CreateAotBuilder();
        foreach (var i in PropertyOnlySourceGeneratorContext.ConverterCreators)
            _ = builder.AddConverterCreator(i.Value);
        var generator = builder.Build();
        var converter = generator.GetConverter<T>();
        var converterType = converter.GetType();
        Assert.Equal(converterType.Assembly, typeof(PropertyOnlySourceGeneratorContext).Assembly);

        var generatorSecond = Generator.CreateDefault();
        var converterSecond = generatorSecond.GetConverter<T>();
        Assert.Equal("NamedObjectDelegateConverter`1", converterSecond.GetType().Name);

        var buffer = converter.Encode(source);
        var bufferExpected = converterSecond.Encode(source);
        Assert.Equal(bufferExpected.Length, buffer.Length);

        // swap input byte arrays for decode
        var result = converter.Decode(bufferExpected);
        var resultExpected = converterSecond.Decode(buffer);
        Assert.Equal(source, result);
        Assert.Equal(source, resultExpected);
    }
}
