namespace Mikodev.Binary.SourceGeneration.ObjectCrossTests.EnumerableWithAttributeTests;

using Mikodev.Binary.Attributes;
using Mikodev.Binary.SourceGeneration.ObjectCrossTests.DefaultValueTests;
using System;
using System.Collections;
using System.Collections.Generic;
using Xunit;

[SourceGeneratorContext]
[SourceGeneratorInclude<EnumerableTypeAsNamedObject<int>>]
[SourceGeneratorInclude<EnumerableTypeAsNamedObject<string>>]
[SourceGeneratorInclude<EnumerableTypeAsTupleObject<int>>]
[SourceGeneratorInclude<EnumerableTypeAsTupleObject<string>>]
[SourceGeneratorInclude<NonGenericEnumerableTypeAsNamedObject<int>>]
[SourceGeneratorInclude<NonGenericEnumerableTypeAsNamedObject<string>>]
[SourceGeneratorInclude<NonGenericEnumerableTypeAsTupleObject<int>>]
[SourceGeneratorInclude<NonGenericEnumerableTypeAsTupleObject<string>>]
[SourceGeneratorInclude<EnumerableTypeWithConverterAttribute>]
[SourceGeneratorInclude<EnumerableTypeWithConverterCreatorAttribute>]
[SourceGeneratorInclude<NonGenericEnumerableTypeWithConverterAttribute>]
[SourceGeneratorInclude<NonGenericEnumerableTypeWithConverterCreatorAttribute>]
public partial class IntegrationGeneratorContext { }

class FakeConverter<T> : Converter<T>
{
    public override void Encode(ref Allocator allocator, T? item) => throw new NotSupportedException();

    public override T Decode(in ReadOnlySpan<byte> span) => throw new NotSupportedException();
}

class FakeConverterCreator<T> : IConverterCreator
{
    public IConverter? GetConverter(IGeneratorContext context, Type type)
    {
        if (type != typeof(T))
            return null;
        return new FakeConverter<T>();
    }
}

[NamedObject]
class EnumerableTypeAsNamedObject<T> : IEnumerable<T?>, IEquatable<EnumerableTypeAsNamedObject<T>?>
{
    [NamedKey("alpha")]
    public T? Alpha { get; set; }

    [NamedKey("bravo")]
    public T? Bravo { get; set; }

    public bool Equals(EnumerableTypeAsNamedObject<T>? other) =>
        other is not null &&
        EqualityComparer<T?>.Default.Equals(Alpha, other.Alpha) &&
        EqualityComparer<T?>.Default.Equals(Bravo, other.Bravo);

    public override bool Equals(object? obj) => throw new NotSupportedException();

    public override int GetHashCode() => throw new NotSupportedException();

    IEnumerator IEnumerable.GetEnumerator() => throw new NotSupportedException();

    IEnumerator<T?> IEnumerable<T?>.GetEnumerator() => throw new NotSupportedException();
}

[TupleObject]
struct EnumerableTypeAsTupleObject<T> : IEnumerable<T?>, IEquatable<EnumerableTypeAsTupleObject<T>>
{
    [TupleKey(0)]
    public T? A;

    [TupleKey(1)]
    public T? B;

    [TupleKey(2)]
    public T? C;

    public readonly bool Equals(EnumerableTypeAsTupleObject<T> other) =>
        EqualityComparer<T?>.Default.Equals(this.A, other.A) &&
        EqualityComparer<T?>.Default.Equals(this.B, other.B) &&
        EqualityComparer<T?>.Default.Equals(this.C, other.C);

    public override readonly bool Equals(object? obj) => throw new NotSupportedException();

    public override readonly int GetHashCode() => throw new NotSupportedException();

    readonly IEnumerator IEnumerable.GetEnumerator() => throw new NotSupportedException();

    readonly IEnumerator<T?> IEnumerable<T?>.GetEnumerator() => throw new NotSupportedException();
}

[NamedObject]
struct NonGenericEnumerableTypeAsNamedObject<T> : IEnumerable, IEquatable<NonGenericEnumerableTypeAsNamedObject<T>>
{
    [NamedKey("data")]
    public T? Data { get; set; }

    public readonly bool Equals(NonGenericEnumerableTypeAsNamedObject<T> other) => EqualityComparer<T?>.Default.Equals(Data, other.Data);

    public override readonly bool Equals(object? obj) => throw new NotSupportedException();

    public override readonly int GetHashCode() => throw new NotSupportedException();

    readonly IEnumerator IEnumerable.GetEnumerator() => throw new NotSupportedException();
}

[TupleObject]
class NonGenericEnumerableTypeAsTupleObject<T> : IEnumerable, IEquatable<NonGenericEnumerableTypeAsTupleObject<T>?>
{
    [TupleKey(0)]
    public T? Item { get; set; }

    public bool Equals(NonGenericEnumerableTypeAsTupleObject<T>? other) => other is not null && EqualityComparer<T?>.Default.Equals(Item, other.Item);

    public override bool Equals(object? obj) => throw new NotSupportedException();

    public override int GetHashCode() => throw new NotSupportedException();

    IEnumerator IEnumerable.GetEnumerator() => throw new NotSupportedException();
}

[Converter(typeof(FakeConverter<EnumerableTypeWithConverterAttribute>))]
class EnumerableTypeWithConverterAttribute : IEnumerable<Guid>
{
    IEnumerator IEnumerable.GetEnumerator() => throw new NotSupportedException();

    IEnumerator<Guid> IEnumerable<Guid>.GetEnumerator() => throw new NotSupportedException();
}

[ConverterCreator(typeof(FakeConverterCreator<EnumerableTypeWithConverterCreatorAttribute>))]
struct EnumerableTypeWithConverterCreatorAttribute : IEnumerable<decimal>
{
    readonly IEnumerator IEnumerable.GetEnumerator() => throw new NotSupportedException();

    readonly IEnumerator<decimal> IEnumerable<decimal>.GetEnumerator() => throw new NotSupportedException();
}

[Converter(typeof(FakeConverter<NonGenericEnumerableTypeWithConverterAttribute>))]
struct NonGenericEnumerableTypeWithConverterAttribute : IEnumerable
{
    readonly IEnumerator IEnumerable.GetEnumerator() => throw new NotSupportedException();
}

[ConverterCreator(typeof(FakeConverterCreator<NonGenericEnumerableTypeWithConverterCreatorAttribute>))]
class NonGenericEnumerableTypeWithConverterCreatorAttribute : IEnumerable
{
    public IEnumerator GetEnumerator() => throw new NotSupportedException();
}

public class IntegrationTests
{
    public static IEnumerable<object[]> EnumerableTypeAsNamedObjectData()
    {
        var a = new EnumerableTypeAsNamedObject<int> { Alpha = 1, Bravo = 2 };
        var b = new EnumerableTypeAsNamedObject<string> { Alpha = "three", Bravo = "four" };
        yield return [a, new { alpha = 1, bravo = 2 }];
        yield return [b, new { alpha = "three", bravo = "four" }];
    }

    public static IEnumerable<object[]> EnumerableTypeAsTupleObjectData()
    {
        var a = new EnumerableTypeAsTupleObject<int> { A = 1, B = 2, C = 3 };
        var b = new EnumerableTypeAsTupleObject<string> { A = "four", B = "five", C = "six" };
        yield return [a, (1, 2, 3)];
        yield return [b, ("four", "five", "six")];
    }

    public static IEnumerable<object[]> NonGenericEnumerableTypeAsNamedObjectData()
    {
        var a = new NonGenericEnumerableTypeAsNamedObject<int> { Data = 1 };
        var b = new NonGenericEnumerableTypeAsNamedObject<string> { Data = "two" };
        yield return [a, new { data = 1 }];
        yield return [b, new { data = "two" }];
    }

    public static IEnumerable<object[]> NonGenericEnumerableTypeAsTupleObjectData()
    {
        var a = new NonGenericEnumerableTypeAsTupleObject<int> { Item = 3 };
        var b = new NonGenericEnumerableTypeAsTupleObject<string> { Item = "four" };
        yield return [a, ValueTuple.Create(3)];
        yield return [b, ValueTuple.Create("four")];
    }

    [Theory(DisplayName = "Encode Decode Test")]
    [MemberData(nameof(EnumerableTypeAsNamedObjectData))]
    [MemberData(nameof(EnumerableTypeAsTupleObjectData))]
    [MemberData(nameof(NonGenericEnumerableTypeAsNamedObjectData))]
    [MemberData(nameof(NonGenericEnumerableTypeAsTupleObjectData))]
    public void EncodeDecodeTest<T, A>(T data, A anonymous)
    {
        Assert.NotNull(data);
        var generator = Generator.CreateAotBuilder().AddConverterCreators(IntegrationGeneratorContext.ConverterCreators.Values).Build();
        var converter = generator.GetConverter<T>();

        var generatorSecond = Generator.CreateDefault();
        var converterSecond = generatorSecond.GetConverter<T>();
        Assert.Equal(converter.GetType().Assembly, typeof(NamedObjectWithDefaultValueSourceGeneratorContext).Assembly);
        Assert.Equal(converterSecond.GetType().Assembly, typeof(IConverter).Assembly);

        var converterAnonymous = generatorSecond.GetConverter<A>();
        var buffer = converter.Encode(data);
        var bufferSecond = converterSecond.Encode(data);
        var bufferAnonymous = converterAnonymous.Encode(anonymous);
        Assert.Equal(bufferAnonymous, buffer);
        Assert.Equal(bufferAnonymous, bufferSecond);

        var result = converter.Decode(bufferSecond);
        var resultSecond = converterSecond.Decode(buffer);
        Assert.True(Assert.IsType<IEquatable<T>>(data, exactMatch: false).Equals(result));
        Assert.True(Assert.IsType<IEquatable<T>>(data, exactMatch: false).Equals(resultSecond));
    }

    [Theory]
    [InlineData(typeof(EnumerableTypeWithConverterAttribute))]
    [InlineData(typeof(NonGenericEnumerableTypeWithConverterAttribute))]
    [InlineData(typeof(EnumerableTypeWithConverterCreatorAttribute))]
    [InlineData(typeof(NonGenericEnumerableTypeWithConverterCreatorAttribute))]
    public void GetConverterTest(Type type)
    {
        var generator = Generator.CreateAotBuilder().AddConverterCreators(IntegrationGeneratorContext.ConverterCreators.Values).Build();
        var converter = generator.GetConverter(type);

        var generatorSecond = Generator.CreateDefault();
        var converterSecond = generatorSecond.GetConverter(type);

        var expectedConverterType = typeof(FakeConverter<>).MakeGenericType(type);
        Assert.Equal(expectedConverterType, converter.GetType());
        Assert.Equal(expectedConverterType, converterSecond.GetType());
    }
}
