namespace Mikodev.Binary.SourceGeneration.CollectionTests.InlineArrayTests;

using Mikodev.Binary.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Xunit;

#pragma warning disable CA2211 // Non-constant fields should not be visible
[InlineArray(4)]
public struct TestArray4<T>
{
    public T InstanceField;

    public static T? StaticField;

    public static T?[]? StaticArrayField;
}

[InlineArray(10)]
public struct TestArray10<T>
{
    internal T InternalInstanceField;

    internal static T? InternalStaticField;

    internal static T?[]? InternalStaticArrayField;
}

[SourceGeneratorContext]
[SourceGeneratorInclude<TestArray4<int>>]
[SourceGeneratorInclude<TestArray4<string>>]
[SourceGeneratorInclude<TestArray10<int>>]
[SourceGeneratorInclude<TestArray10<string>>]
public partial class IntegrationGeneratorContext { }

public class FakeConverter<T>(int length) : Converter<T>(length)
{
    public override void Encode(ref Allocator allocator, T? item) => throw new NotSupportedException();

    public override T Decode(in ReadOnlySpan<byte> span) => throw new NotSupportedException();
}

public class IntegrationTests
{
    public static IEnumerable<object[]> InlineArrayData()
    {
        var a = new TestArray4<int>();
        var b = Enumerable.Range(0, 4).ToArray();
        new ReadOnlySpan<int>(b).CopyTo(a);

        var c = new TestArray4<string>();
        var d = Enumerable.Range(0, 4).Select(x => x.ToString()).ToArray();
        new ReadOnlySpan<string>(d).CopyTo(c);

        var h = new TestArray10<int>();
        var i = Enumerable.Range(0, 10).ToArray();
        new ReadOnlySpan<int>(i).CopyTo(h);

        var j = new TestArray10<string>();
        var k = Enumerable.Range(0, 10).Select(x => x.ToString()).ToArray();
        new ReadOnlySpan<string>(k).CopyTo(j);

        yield return new object[] { a, b, 16 };
        yield return new object[] { c, d, 0 };
        yield return new object[] { h, i, 40 };
        yield return new object[] { j, k, 0 };
    }

    [Theory(DisplayName = "Integration Test")]
    [MemberData(nameof(InlineArrayData))]
    public void IntegrationTest<T, E>(T item, E[] expected, int converterLength)
    {
        var generator = Generator.CreateAotBuilder()
            .AddConverterCreators(IntegrationGeneratorContext.ConverterCreators.Values)
            .Build();
        var converter = generator.GetConverter<T>();
        var converterType = converter.GetType();
        Assert.Equal(typeof(IntegrationGeneratorContext).Assembly, converterType.Assembly);

        var generatorSecond = Generator.CreateDefault();
        var converterSecond = generatorSecond.GetConverter<T>();
        var converterSecondType = converterSecond.GetType();
        Assert.Equal(typeof(IConverter).Assembly, converterSecondType.Assembly);

        Assert.Equal(converterLength, converter.Length);
        Assert.Equal(converterLength, converterSecond.Length);

        var buffer = converter.Encode(item);
        var bufferSecond = converterSecond.Encode(item);
        var bufferExpected = generatorSecond.Encode(expected);
        Assert.Equal(bufferExpected, buffer);
        Assert.Equal(bufferExpected, bufferSecond);

        var result = converter.Decode(bufferExpected);
        var resultSecond = converterSecond.Decode(bufferExpected);
        var bufferResult = converter.Encode(result);
        var bufferResultSecond = converterSecond.Encode(resultSecond);
        Assert.Equal(bufferExpected, bufferResult);
        Assert.Equal(bufferExpected, bufferResultSecond);
    }

    [Fact(DisplayName = "Converter Length Overflow Test")]
    public void ConverterLengthOverflowTest()
    {
        var a = Generator.CreateAotBuilder().AddConverterCreators(IntegrationGeneratorContext.ConverterCreators.Values).AddConverter(new FakeConverter<int>(100)).Build();
        var b = Generator.CreateDefaultBuilder().AddConverter(new FakeConverter<int>(200)).Build();
        Assert.Equal(400, a.GetConverter<TestArray4<int>>().Length);
        Assert.Equal(800, b.GetConverter<TestArray4<int>>().Length);
        Assert.Equal(1000, a.GetConverter<TestArray10<int>>().Length);
        Assert.Equal(2000, b.GetConverter<TestArray10<int>>().Length);

        var c = Generator.CreateAotBuilder().AddConverterCreators(IntegrationGeneratorContext.ConverterCreators.Values).AddConverter(new FakeConverter<int>(0x2000_0000)).Build();
        var d = Generator.CreateDefaultBuilder().AddConverter(new FakeConverter<int>(0x3000_0000)).Build();
        _ = Assert.Throws<OverflowException>(c.GetConverter<TestArray4<int>>);
        _ = Assert.Throws<OverflowException>(d.GetConverter<TestArray4<int>>);
        _ = Assert.Throws<OverflowException>(c.GetConverter<TestArray10<int>>);
        _ = Assert.Throws<OverflowException>(d.GetConverter<TestArray10<int>>);
    }
}
#pragma warning restore CA2211 // Non-constant fields should not be visible
