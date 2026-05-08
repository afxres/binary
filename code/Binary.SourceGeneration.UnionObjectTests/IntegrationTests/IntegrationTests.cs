namespace Mikodev.Binary.SourceGeneration.UnionObjectTests.IntegrationTests;

using Microsoft.FSharp.Core;
using Mikodev.Binary.Attributes;
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

[Union]
public class UnionWithVariousConstructors<A, B, C, D> : IUnion
{
    public object? Value { get; init; }

    public UnionWithVariousConstructors(A value) => Value = value;

    public UnionWithVariousConstructors(in B value) => Value = value;

    public UnionWithVariousConstructors(ref C value) => Value = value;

    public UnionWithVariousConstructors(out D value) => throw new NotSupportedException();

    public UnionWithVariousConstructors(A a, B b) => throw new NotSupportedException();

    public UnionWithVariousConstructors(A a, B b, C c) => throw new NotSupportedException();

    public UnionWithVariousConstructors(A a, B b, C c, D d) => throw new NotSupportedException();

    public override bool Equals(object? obj) => Equals(Value, (obj as UnionWithVariousConstructors<A, B, C, D>)?.Value);

    public override int GetHashCode() => RuntimeHelpers.GetHashCode(Value);
}

[SourceGeneratorContext]
[SourceGeneratorInclude<Pet>]
[SourceGeneratorInclude<Choice<int, string>>]
[SourceGeneratorInclude<Choice<string, int>>]
[SourceGeneratorInclude<Choice<double, string>>]
[SourceGeneratorInclude<Choice<char[], object>>]
[SourceGeneratorInclude<Choice<A, B>>]
[SourceGeneratorInclude<Choice<B, A>>]
[SourceGeneratorInclude<UnionWithVariousConstructors<int, string, double, object>>]
public partial class IntegrationGeneratorContext { }

public class IntegrationTests
{
    [Theory(DisplayName = "Union Basic Test")]
    [InlineData(typeof(Pet))]
    [InlineData(typeof(Choice<int, string>))]
    [InlineData(typeof(Choice<char[], object>))]
    [InlineData(typeof(UnionWithVariousConstructors<int, string, double, object>))]
    public void UnionBasicTest(Type type)
    {
        var generator = Generator.CreateDefault();
        var generatorSecond = Generator.CreateAotBuilder()
            .AddConverterCreators(IntegrationGeneratorContext.ConverterCreators.Values)
            .Build();
        var converter = generator.GetConverter(type);
        var converterSecond = generatorSecond.GetConverter(type);
        Assert.Equal(0, converter.Length);
        Assert.Equal(0, converterSecond.Length);
        Assert.Equal("UnionConverter`1", converter.GetType().Name);
        Assert.False(converterSecond.GetType().IsGenericType);
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

    public static TheoryData<UnionWithVariousConstructors<int, string, double, object>, FSharpChoice<int, string>, Type, int> UnionWithVariousConstructorsCommonTestData()
    {
        var data = new TheoryData<UnionWithVariousConstructors<int, string, double, object>, FSharpChoice<int, string>, Type, int>();
        var a = 6;
        var b = "Nice";
        data.Add(a, FSharpChoice<int, string>.NewChoice1Of2(a), typeof(FSharpChoice<int, string>), 0);
        data.Add(b, FSharpChoice<int, string>.NewChoice2Of2(b), typeof(FSharpChoice<int, string>), 1);
        return data;
    }

    [Theory(DisplayName = "Union Encode Decode Test")]
    [MemberData(nameof(PetUnionAndFSharpChoiceTestData))]
    [MemberData(nameof(ChoiceUnionAndFSharpChoiceDoubleStringTestData))]
    [MemberData(nameof(ChoiceUnionAndFSharpChoiceABTestData))]
    [MemberData(nameof(ChoiceUnionAndFSharpChoiceBATestData))]
    [MemberData(nameof(UnionWithVariousConstructorsCommonTestData))]
    public void UnionEncodeDecodeTest<A, B>(A source, B contrast, Type contrastType, int tag)
    {
        var sourceIntent = (source as IUnion)?.Value;
        Assert.NotNull(sourceIntent);
        var generator = Generator.CreateDefaultBuilder().AddFSharpConverterCreators().Build();
        var generatorSecond = Generator.CreateAotBuilder()
            .AddConverterCreators(IntegrationGeneratorContext.ConverterCreators.Values)
            .Build();
        var converter = generator.GetConverter<A>();
        var converterContrast = generator.GetConverter(contrastType);
        var converterSecond = generatorSecond.GetConverter<A>();
        var buffer = converter.Encode(source);
        var bufferContrast = converterContrast.Encode(contrast);
        var bufferSecond = converterSecond.Encode(source);
        Assert.Equal(bufferContrast, buffer);
        Assert.Equal(bufferContrast, bufferSecond);
        var span = new ReadOnlySpan<byte>(buffer);
        var head = Converter.Decode(ref span);
        var intent = generator.Encode(sourceIntent);
        var spanSecond = new ReadOnlySpan<byte>(buffer);
        var headSecond = Converter.Decode(ref spanSecond);
        Assert.Equal(tag, head);
        Assert.Equal(tag, headSecond);
        Assert.Equal(intent, span);
        Assert.Equal(intent, spanSecond);
        var result = converter.Decode(buffer);
        var resultSecond = converterSecond.Decode(buffer);
        Assert.NotNull(result as IUnion);
        Assert.NotNull(resultSecond as IUnion);
        Assert.Equal(source, result);
        Assert.Equal(source, resultSecond);
        Assert.Equal(sourceIntent, ((IUnion)result).Value);
        Assert.Equal(sourceIntent, ((IUnion)resultSecond).Value);
    }

    [Theory(DisplayName = "Union Encode Auto Decode Auto Test")]
    [MemberData(nameof(PetUnionAndFSharpChoiceTestData))]
    [MemberData(nameof(ChoiceUnionAndFSharpChoiceDoubleStringTestData))]
    [MemberData(nameof(ChoiceUnionAndFSharpChoiceABTestData))]
    [MemberData(nameof(ChoiceUnionAndFSharpChoiceBATestData))]
    [MemberData(nameof(UnionWithVariousConstructorsCommonTestData))]
    public void UnionEncodeAutoDecodeAutoTest<A, B>(A source, B contrast, Type contrastType, int tag) where B : class
    {
        var sourceIntent = (source as IUnion)?.Value;
        Assert.NotNull(sourceIntent);
        var generator = Generator.CreateDefaultBuilder().AddFSharpConverterCreators().Build();
        var generatorSecond = Generator.CreateAotBuilder()
            .AddConverterCreators(IntegrationGeneratorContext.ConverterCreators.Values)
            .Build();
        var converter = generator.GetConverter<A>();
        var converterContrast = generator.GetConverter(contrastType);
        var converterSecond = generatorSecond.GetConverter<A>();
        var buffer = Allocator.Invoke(source, converter.EncodeAuto);
        var bufferContrast = Allocator.Invoke((object)contrast, converterContrast.EncodeAuto);
        var bufferSecond = Allocator.Invoke(source, converterSecond.EncodeAuto);
        Assert.Equal(bufferContrast, buffer);
        Assert.Equal(bufferContrast, bufferSecond);
        var span = new ReadOnlySpan<byte>(buffer);
        var head = Converter.Decode(ref span);
        var intent = Allocator.Invoke(sourceIntent, generator.GetConverter(sourceIntent.GetType()).EncodeAuto);
        var spanSecond = new ReadOnlySpan<byte>(buffer);
        var headSecond = Converter.Decode(ref spanSecond);
        Assert.Equal(tag, head);
        Assert.Equal(tag, headSecond);
        Assert.Equal(intent, span);
        Assert.Equal(intent, spanSecond);
        var body = new ReadOnlySpan<byte>(buffer);
        var result = converter.DecodeAuto(ref body);
        var bodySecond = new ReadOnlySpan<byte>(buffer);
        var resultSecond = converterSecond.DecodeAuto(ref bodySecond);
        Assert.NotNull(result as IUnion);
        Assert.NotNull(resultSecond as IUnion);
        Assert.Equal(0, body.Length);
        Assert.Equal(0, bodySecond.Length);
        Assert.Equal(source, result);
        Assert.Equal(source, resultSecond);
        Assert.Equal(sourceIntent, ((IUnion)result).Value);
        Assert.Equal(sourceIntent, ((IUnion)resultSecond).Value);
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
        data.Add(typeof(UnionWithVariousConstructors<int, string, double, object>), [0x02], 2);
        data.Add(typeof(UnionWithVariousConstructors<int, string, double, object>), [0x80, 0, 0, 2], 2);
        data.Add(typeof(UnionWithVariousConstructors<int, string, double, object>), [0x80, 0, 0x04, 0], 1024);
        data.Add(typeof(UnionWithVariousConstructors<int, string, double, object>), [0xFF, 0xFF, 0xFF, 0xFF], int.MaxValue);
        return data;
    }

    [Theory(DisplayName = "Union Deode Decode Auto With Invalid Tag Test")]
    [MemberData(nameof(UnionDecodeDecodeAutoWithInvalidTagTestData))]
    public void UnionDecodeDecodeAutoWithInvalidTagTest(Type type, byte[] buffer, int tag)
    {
        var generator = Generator.CreateDefault();
        var generatorSecond = Generator.CreateAotBuilder()
            .AddConverterCreators(IntegrationGeneratorContext.ConverterCreators.Values)
            .Build();
        var converter = generator.GetConverter(type);
        var converterSecond = generatorSecond.GetConverter(type);
        var error = Assert.Throws<ArgumentException>(() => converter.Decode(buffer));
        var errorAuto = Assert.Throws<ArgumentException>(() =>
        {
            var body = new ReadOnlySpan<byte>(buffer);
            _ = converter.DecodeAuto(ref body);
            Assert.Fail();
        });
        var errorSecond = Assert.Throws<ArgumentException>(() => converterSecond.Decode(buffer));
        var errorAutoSecond = Assert.Throws<ArgumentException>(() =>
        {
            var body = new ReadOnlySpan<byte>(buffer);
            _ = converterSecond.DecodeAuto(ref body);
            Assert.Fail();
        });
        var message = $"Invalid union tag '{tag}', type: {type}";
        Assert.Equal(message, error.Message);
        Assert.Equal(message, errorAuto.Message);
        Assert.Equal(message, errorSecond.Message);
        Assert.Equal(message, errorAutoSecond.Message);
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
        var generatorSecond = Generator.CreateAotBuilder()
            .AddConverterCreators(IntegrationGeneratorContext.ConverterCreators.Values)
            .Build();
        var converter = generator.GetConverter(type);
        var converterSecond = generatorSecond.GetConverter(type);
        var error = Assert.Throws<ArgumentException>(() => converter.Encode(source));
        var errorAuto = Assert.Throws<ArgumentException>(() => Allocator.Invoke(source, converter.EncodeAuto));
        var errorSecond = Assert.Throws<ArgumentException>(() => converterSecond.Encode(source));
        var errorAutoSecond = Assert.Throws<ArgumentException>(() => Allocator.Invoke(source, converterSecond.EncodeAuto));
        var message = $"Invalid or null union value, type: {type}";
        Assert.Null(error.ParamName);
        Assert.Null(errorAuto.ParamName);
        Assert.Null(errorSecond.ParamName);
        Assert.Null(errorAutoSecond.ParamName);
        Assert.Equal(message, error.Message);
        Assert.Equal(message, errorAuto.Message);
        Assert.Equal(message, errorSecond.Message);
        Assert.Equal(message, errorAutoSecond.Message);
    }

    [Theory(DisplayName = "Union Case Type Duplicated Test")]
    [InlineData(typeof(Choice<int, int>))]
    [InlineData(typeof(Choice<string, string>))]
    public void UnionCaseTypeDuplicatedTest(Type type)
    {
        var generator = Generator.CreateDefault();
        var error = Assert.Throws<ArgumentException>(() => generator.GetConverter(type));
        var message = $"Union case type duplicated, type: {type}";
        Assert.Null(error.ParamName);
        Assert.Equal(message, error.Message);
    }
}
