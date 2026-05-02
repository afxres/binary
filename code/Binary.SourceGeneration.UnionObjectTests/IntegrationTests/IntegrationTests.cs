namespace Mikodev.Binary.SourceGeneration.UnionObjectTests.IntegrationTests;

using Microsoft.FSharp.Core;
using System;
using System.Runtime.CompilerServices;
using Xunit;

public record Cat(int Id);

public record Dog(string Name);

public readonly union Pet(Cat, Dog);

public record A(int Id);

public record B(int Id, string Name) : A(Id);

[Union]
public class Choice<A, B> : IUnion
{
    public object? Value { get; init; }

    public Choice(A value) => Value = value;

    public Choice(B value) => Value = value;

    public override bool Equals(object? obj) => Equals(Value, (obj as Choice<A, B>)?.Value);

    public override int GetHashCode() => RuntimeHelpers.GetHashCode(Value);
}

public class IntegrationTests
{
    [Theory(DisplayName = "Union Basic Test")]
    [InlineData(typeof(Pet))]
    [InlineData(typeof(Choice<int, string>))]
    public void UnionBasicTest(Type type)
    {
        var generator = Generator.CreateDefault();
        var converter = generator.GetConverter(type);
        Assert.Equal(0, converter.Length);
        Assert.Equal("UnionConverter`1", converter.GetType().Name);
    }

    public static TheoryData<Pet, FSharpChoice<Cat, Dog>, Type, int> PetUnionAndFSharpChoiceTestData()
    {
        var data = new TheoryData<Pet, FSharpChoice<Cat, Dog>, Type, int>();
        var cat = new Cat(0xCCAA);
        var dog = new Dog("Spike");
        data.Add(cat, FSharpChoice<Cat, Dog>.NewChoice1Of2(cat), typeof(FSharpChoice<Cat, Dog>), 0);
        data.Add(dog, FSharpChoice<Cat, Dog>.NewChoice2Of2(dog), typeof(FSharpChoice<Cat, Dog>), 1);
        return data;
    }

    public static TheoryData<Choice<double, string>, FSharpChoice<double, string>, Type, int> ChoiceUnionAndFSharpChoiceDoubleStringTestData()
    {
        var data = new TheoryData<Choice<double, string>, FSharpChoice<double, string>, Type, int>();
        var a = new Choice<double, string>(1.0);
        var b = new Choice<double, string>("Two");
        data.Add(a, FSharpChoice<double, string>.NewChoice1Of2(1.0), typeof(FSharpChoice<double, string>), 0);
        data.Add(b, FSharpChoice<double, string>.NewChoice2Of2("Two"), typeof(FSharpChoice<double, string>), 1);
        return data;
    }

    public static TheoryData<Choice<A, B>, FSharpChoice<A, B>, Type, int> ChoiceUnionAndFSharpChoiceABTestData()
    {
        var data = new TheoryData<Choice<A, B>, FSharpChoice<A, B>, Type, int>();
        var a = new A(4);
        var b = new B(7, "Seven");
        data.Add(a, FSharpChoice<A, B>.NewChoice1Of2(a), typeof(FSharpChoice<A, B>), 0);
        data.Add(b, FSharpChoice<A, B>.NewChoice2Of2(b), typeof(FSharpChoice<A, B>), 1);
        return data;
    }

    public static TheoryData<Choice<B, A>, FSharpChoice<B, A>, Type, int> ChoiceUnionAndFSharpChoiceBATestData()
    {
        var data = new TheoryData<Choice<B, A>, FSharpChoice<B, A>, Type, int>();
        var a = new A(6);
        var b = new B(8, "Eight");
        data.Add(b, FSharpChoice<B, A>.NewChoice1Of2(b), typeof(FSharpChoice<B, A>), 0);
        data.Add(a, FSharpChoice<B, A>.NewChoice2Of2(a), typeof(FSharpChoice<B, A>), 1);
        return data;
    }

    [Theory(DisplayName = "Union Encode Decode Test")]
    [MemberData(nameof(PetUnionAndFSharpChoiceTestData))]
    [MemberData(nameof(ChoiceUnionAndFSharpChoiceDoubleStringTestData))]
    [MemberData(nameof(ChoiceUnionAndFSharpChoiceABTestData))]
    [MemberData(nameof(ChoiceUnionAndFSharpChoiceBATestData))]
    public void UnionEncodeDecodeTest<A, B>(A source, B contrast, Type contrastType, int tag)
    {
        var sourceIntent = (source as IUnion)?.Value;
        Assert.NotNull(sourceIntent);
        var generator = Generator.CreateDefaultBuilder().AddFSharpConverterCreators().Build();
        var converter = generator.GetConverter<A>();
        var converterContrast = generator.GetConverter(contrastType);
        var buffer = converter.Encode(source);
        var bufferContrast = converterContrast.Encode(contrast);
        Assert.Equal(buffer, bufferContrast);
        var span = new ReadOnlySpan<byte>(buffer);
        var head = Converter.Decode(ref span);
        var intent = generator.Encode(sourceIntent);
        Assert.Equal(tag, head);
        Assert.Equal(span, intent);
        var result = converter.Decode(buffer);
        Assert.NotNull(result as IUnion);
        Assert.Equal(source, result);
        Assert.Equal(sourceIntent, ((IUnion)result).Value);
    }

    [Theory(DisplayName = "Union Encode Auto Decode Auto Test")]
    [MemberData(nameof(PetUnionAndFSharpChoiceTestData))]
    [MemberData(nameof(ChoiceUnionAndFSharpChoiceDoubleStringTestData))]
    [MemberData(nameof(ChoiceUnionAndFSharpChoiceABTestData))]
    [MemberData(nameof(ChoiceUnionAndFSharpChoiceBATestData))]
    public void UnionEncodeAutoDecodeAutoTest<A, B>(A source, B contrast, Type contrastType, int tag) where B : class
    {
        var sourceIntent = (source as IUnion)?.Value;
        Assert.NotNull(sourceIntent);
        var generator = Generator.CreateDefaultBuilder().AddFSharpConverterCreators().Build();
        var converter = generator.GetConverter<A>();
        var converterContrast = generator.GetConverter(contrastType);
        var buffer = Allocator.Invoke(source, converter.EncodeAuto);
        var bufferContrast = Allocator.Invoke((object)contrast, converterContrast.EncodeAuto);
        Assert.Equal(buffer, bufferContrast);
        var span = new ReadOnlySpan<byte>(buffer);
        var head = Converter.Decode(ref span);
        var intent = Allocator.Invoke(sourceIntent, generator.GetConverter(sourceIntent.GetType()).EncodeAuto);
        Assert.Equal(tag, head);
        Assert.Equal(span, intent);
        var body = new ReadOnlySpan<byte>(buffer);
        var result = converter.DecodeAuto(ref body);
        Assert.NotNull(result as IUnion);
        Assert.Equal(0, body.Length);
        Assert.Equal(source, result);
        Assert.Equal(sourceIntent, ((IUnion)result).Value);
    }

    public static TheoryData<Type, byte[], int> UnionDecodeDecodeAutoWithInvalidTagTestData()
    {
        var data = new TheoryData<Type, byte[], int>();
        data.Add(typeof(Pet), [0x02], 2);
        data.Add(typeof(Pet), [0x80, 0, 0, 2], 2);
        data.Add(typeof(Pet), [0x80, 0, 0x04, 0], 1024);
        data.Add(typeof(Pet), [0xFF, 0xFF, 0xFF, 0xFF], int.MaxValue);
        data.Add(typeof(Choice<string, int>), [0x02], 2);
        data.Add(typeof(Choice<string, int>), [0x80, 0, 0, 2], 2);
        data.Add(typeof(Choice<string, int>), [0x80, 0, 0x04, 0], 1024);
        data.Add(typeof(Choice<string, int>), [0xFF, 0xFF, 0xFF, 0xFF], int.MaxValue);
        return data;
    }

    [Theory(DisplayName = "Union Deode Decode Auto With Invalid Tag Test")]
    [MemberData(nameof(UnionDecodeDecodeAutoWithInvalidTagTestData))]
    public void UnionDecodeDecodeAutoWithInvalidTagTest(Type type, byte[] buffer, int tag)
    {
        var generator = Generator.CreateDefault();
        var converter = generator.GetConverter(type);
        var a = Assert.Throws<ArgumentException>(() => converter.Decode(buffer));
        var b = Assert.Throws<ArgumentException>(() =>
        {
            var body = new ReadOnlySpan<byte>(buffer);
            _ = converter.DecodeAuto(ref body);
            Assert.Fail();
        });
        var message = $"Invalid union tag '{tag}', type: {type}";
        Assert.Equal(message, a.Message);
        Assert.Equal(message, b.Message);
    }

    public static TheoryData<object?, Type> UnionWithNullOrNullValueTestData()
    {
        var data = new TheoryData<object?, Type>();
        data.Add(null, typeof(Choice<int, string>));
        data.Add(new Choice<int, string?>(default(string)), typeof(Choice<int, string>));
        data.Add(default(Pet), typeof(Pet));
        return data;
    }

    [Theory(DisplayName = "Union With Null Or Null Value Test")]
    [MemberData(nameof(UnionWithNullOrNullValueTestData))]
    public void UnionWithNullOrNullValueTest(object? source, Type type)
    {
        var generator = Generator.CreateDefault();
        var converter = generator.GetConverter(type);
        var error = Assert.Throws<ArgumentException>(() => converter.Encode(source));
        var errorAuto = Assert.Throws<ArgumentException>(() => Allocator.Invoke(source, generator.GetConverter(type).EncodeAuto));
        var message = $"Invalid or null union value, type: {type}";
        Assert.Null(error.ParamName);
        Assert.Null(errorAuto.ParamName);
        Assert.Equal(message, error.Message);
        Assert.Equal(message, errorAuto.Message);
    }
}
