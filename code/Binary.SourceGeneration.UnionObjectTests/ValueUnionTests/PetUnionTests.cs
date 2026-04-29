namespace Mikodev.Binary.SourceGeneration.UnionObjectTests.ValueUnionTests;

using System;
using Xunit;

public record Cat(int Id);

public record Dog(string Name);

public readonly union Pet(Cat, Dog);

public class PetUnionTests
{
    [Fact(DisplayName = "Pet Union Converter Basic Test")]
    public void PetUnionBasicTest()
    {
        var generator = Generator.CreateDefault();
        var converter = generator.GetConverter<Pet>();
        Assert.Equal(0, converter.Length);
        Assert.Equal("UnionConverter`1", converter.GetType().Name);
    }

    public static TheoryData<Pet, int> PetUnionTestData()
    {
        var data = new TheoryData<Pet, int>();
        var cat = new Cat(0xCCAA);
        var dog = new Dog("Spike");
        data.Add(cat, 0);
        data.Add(dog, 1);
        return data;
    }

    [Theory(DisplayName = "Pet Union Encode Decode Test")]
    [MemberData(nameof(PetUnionTestData))]
    public void PetUnionEncodeDecodeTest(Pet source, int tag)
    {
        var generator = Generator.CreateDefault();
        var converter = generator.GetConverter<Pet>();
        var buffer = converter.Encode(source);
        var span = new ReadOnlySpan<byte>(buffer);
        var head = Converter.Decode(ref span);
        var intent = generator.Encode(source.Value);
        Assert.Equal(tag, head);
        Assert.Equal(span, intent);
        var result = converter.Decode(buffer);
        Assert.Equal(source, result);
        Assert.Equal(source.Value, result.Value);
    }

    [Theory(DisplayName = "Pet Union Encode Decode Test")]
    [MemberData(nameof(PetUnionTestData))]
    public void PetUnionEncodeAutoDecodeAutoTest(Pet source, int tag)
    {
        var generator = Generator.CreateDefault();
        var converter = generator.GetConverter<Pet>();
        var buffer = Allocator.Invoke(source, converter.EncodeAuto);
        var span = new ReadOnlySpan<byte>(buffer);
        var head = Converter.Decode(ref span);
        var intent = Allocator.Invoke(source.Value, generator.GetConverter(source.Value).EncodeAuto);
        Assert.Equal(tag, head);
        Assert.Equal(span, intent);
        var body = new ReadOnlySpan<byte>(buffer);
        var result = converter.DecodeAuto(ref body);
        Assert.Equal(0, body.Length);
        Assert.Equal(source, result);
        Assert.Equal(source.Value, result.Value);
    }

    [Theory(DisplayName = "Pet Union Deode Decode Auto With Invalid Tag")]
    [InlineData([new byte[] { 0x02 }, 2])]
    [InlineData([new byte[] { 0x80, 0, 0, 2 }, 2])]
    [InlineData([new byte[] { 0x80, 0, 0x04, 0 }, 1024])]
    [InlineData([new byte[] { 0xFF, 0xFF, 0xFF, 0xFF }, int.MaxValue])]
    public void PetUnionDecodeDecodeAutoWithInvalidTag(byte[] buffer, int tag)
    {
        var generator = Generator.CreateDefault();
        var converter = generator.GetConverter<Pet>();
        var a = Assert.Throws<ArgumentException>(() => converter.Decode(buffer));
        var b = Assert.Throws<ArgumentException>(() =>
        {
            var body = new ReadOnlySpan<byte>(buffer);
            _ = converter.DecodeAuto(ref body);
            Assert.Fail();
        });
        var message = $"Invalid union tag '{tag}', type: {typeof(Pet)}";
        Assert.Equal(message, a.Message);
        Assert.Equal(message, b.Message);
    }
}
