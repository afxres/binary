﻿namespace Mikodev.Binary.SourceGeneration.CollectionTests.SupportedTypesTests;

using Mikodev.Binary.Attributes;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Xunit;

[SourceGeneratorContext]
[SourceGeneratorInclude<ImmutableArray<int>>]
[SourceGeneratorInclude<ImmutableArray<string>>]
[SourceGeneratorInclude<List<int>>]
[SourceGeneratorInclude<List<string>>]
[SourceGeneratorInclude<LinkedList<int>>]
[SourceGeneratorInclude<LinkedList<string>>]
[SourceGeneratorInclude<HashSet<int>>]
[SourceGeneratorInclude<HashSet<string>>]
[SourceGeneratorInclude<Dictionary<int, long>>]
[SourceGeneratorInclude<Dictionary<int, string>>]
[SourceGeneratorInclude<IList<int>>]
[SourceGeneratorInclude<IList<string>>]
[SourceGeneratorInclude<ICollection<int>>]
[SourceGeneratorInclude<ICollection<string>>]
[SourceGeneratorInclude<IEnumerable<int>>]
[SourceGeneratorInclude<IEnumerable<string>>]
[SourceGeneratorInclude<IReadOnlyList<int>>]
[SourceGeneratorInclude<IReadOnlyList<string>>]
[SourceGeneratorInclude<IReadOnlyCollection<int>>]
[SourceGeneratorInclude<IReadOnlyCollection<string>>]
[SourceGeneratorInclude<ISet<int>>]
[SourceGeneratorInclude<ISet<string>>]
[SourceGeneratorInclude<IReadOnlySet<int>>]
[SourceGeneratorInclude<IReadOnlySet<string>>]
[SourceGeneratorInclude<IDictionary<int, long>>]
[SourceGeneratorInclude<IDictionary<int, string>>]
[SourceGeneratorInclude<IReadOnlyDictionary<int, long>>]
[SourceGeneratorInclude<IReadOnlyDictionary<int, string>>]
[SourceGeneratorInclude<ImmutableDictionary<short, long>>]
[SourceGeneratorInclude<ImmutableDictionary<string, int>>]
[SourceGeneratorInclude<ImmutableHashSet<int>>]
[SourceGeneratorInclude<ImmutableHashSet<string>>]
[SourceGeneratorInclude<ImmutableList<int>>]
[SourceGeneratorInclude<ImmutableList<string>>]
[SourceGeneratorInclude<ImmutableQueue<int>>]
[SourceGeneratorInclude<ImmutableQueue<string>>]
[SourceGeneratorInclude<ImmutableSortedDictionary<int, long>>]
[SourceGeneratorInclude<ImmutableSortedDictionary<int, string>>]
[SourceGeneratorInclude<ImmutableSortedSet<int>>]
[SourceGeneratorInclude<ImmutableSortedSet<string>>]
[SourceGeneratorInclude<IImmutableDictionary<short, long>>]
[SourceGeneratorInclude<IImmutableDictionary<string, int>>]
[SourceGeneratorInclude<IImmutableList<int>>]
[SourceGeneratorInclude<IImmutableList<string>>]
[SourceGeneratorInclude<IImmutableQueue<int>>]
[SourceGeneratorInclude<IImmutableQueue<string>>]
[SourceGeneratorInclude<IImmutableSet<int>>]
[SourceGeneratorInclude<IImmutableSet<string>>]
public partial class IntegrationGeneratorContext { }

public class IntegrationTests
{
    public static IEnumerable<object[]> EnumerableTypesData()
    {
        yield return new object[] { new List<int> { 1 }, 1, "SpanLikeConverter`1.*List`1.*Int32" };
        yield return new object[] { new List<string> { "2" }, "2", "SpanLikeConverter`1.*List`1.*String" };
        yield return new object[] { new HashSet<int> { 3 }, 3, "SequenceConverter`1.*HashSet`1.*Int32" };
        yield return new object[] { new HashSet<string> { "4" }, "4", "SequenceConverter`1.*HashSet`1.*String" };
        yield return new object[] { new Dictionary<int, long> { { 5, 6 } }, new KeyValuePair<int, long>(5, 6), "SequenceConverter`1.*Dictionary`2.*Int32.*Int64" };
        yield return new object[] { new Dictionary<int, string> { { 7, "8" } }, new KeyValuePair<int, string>(7, "8"), "SequenceConverter`1.*Dictionary`2.*Int32.*String" };
        yield return new object[] { ImmutableArray.Create(1), 1, "SpanLikeConverter`1.*ImmutableArray`1.*Int32" };
        yield return new object[] { ImmutableArray.Create("2"), "2", "SpanLikeConverter`1.*ImmutableArray`1.*String" };
        yield return new object[] { new LinkedList<int>(new[] { 3 }), 3, "LinkedListConverter`1.*Int32" };
        yield return new object[] { new LinkedList<string>(new[] { "4" }), "4", "LinkedListConverter`1.*String" };
    }

    public static IEnumerable<object[]> ImmutableCollectionTypesData()
    {
        yield return new object[] { ImmutableDictionary.CreateRange(new[] { new KeyValuePair<short, long>(10, 20) }), new KeyValuePair<short, long>(10, 20), "SequenceConverter`1.*ImmutableDictionary`2.*Int16.*Int64" };
        yield return new object[] { ImmutableDictionary.CreateRange(new[] { new KeyValuePair<string, int>("text", 4) }), new KeyValuePair<string, int>("text", 4), "SequenceConverter`1.*ImmutableDictionary`2.*String.*Int32" };
        yield return new object[] { ImmutableHashSet.Create(1), 1, "SequenceConverter`1.*ImmutableHashSet`1.*Int32" };
        yield return new object[] { ImmutableHashSet.Create("2"), "2", "SequenceConverter`1.*ImmutableHashSet`1.*String" };
        yield return new object[] { ImmutableList.Create(3), 3, "SequenceConverter`1.*ImmutableList`1.*Int32" };
        yield return new object[] { ImmutableList.Create("4"), "4", "SequenceConverter`1.*ImmutableList`1.*String" };
        yield return new object[] { ImmutableQueue.Create(5), 5, "SequenceConverter`1.*ImmutableQueue`1.*Int32" };
        yield return new object[] { ImmutableQueue.Create("6"), "6", "SequenceConverter`1.*ImmutableQueue`1.*String" };
        yield return new object[] { ImmutableSortedDictionary.CreateRange(new[] { new KeyValuePair<int, long>(40, 60) }), new KeyValuePair<int, long>(40, 60), "SequenceConverter`1.*ImmutableSortedDictionary`2.*Int32.*Int64" };
        yield return new object[] { ImmutableSortedDictionary.CreateRange(new[] { new KeyValuePair<int, string>(6, "sorted") }), new KeyValuePair<int, string>(6, "sorted"), "SequenceConverter`1.*ImmutableSortedDictionary`2.*Int32.*String" };
        yield return new object[] { ImmutableSortedSet.Create(7), 7, "SequenceConverter`1.*ImmutableSortedSet`1.*Int32" };
        yield return new object[] { ImmutableSortedSet.Create("8"), "8", "SequenceConverter`1.*ImmutableSortedSet`1.*String" };
    }

    [Theory(DisplayName = "Encode Decode Test")]
    [MemberData(nameof(EnumerableTypesData))]
    [MemberData(nameof(ImmutableCollectionTypesData))]
    public void EncodeDecodeTest<T, E>(T source, E element, string pattern) where T : IEnumerable<E>
    {
        var generator = Generator.CreateAotBuilder()
            .AddConverterCreators(IntegrationGeneratorContext.ConverterCreators.Values)
            .Build();
        var converter = generator.GetConverter<T>();
        var converterType = converter.GetType();
        Assert.True(converterType.Assembly == typeof(IConverter).Assembly);
        Assert.Matches(pattern, converterType.FullName);

        // encode test
        var generatorSecond = Generator.CreateDefault();
        var buffer = converter.Encode(source);
        var bufferSecond = generatorSecond.Encode(source);
        Assert.Equal(bufferSecond, buffer);

        // decode test
        var result = converter.Decode(bufferSecond);
        var resultSecond = generatorSecond.Decode<T>(buffer);
        Assert.Equal(element, Assert.Single(result));
        Assert.Equal(element, Assert.Single(resultSecond));

        // empty collection test
        var resultEmpty = converter.Decode(Array.Empty<byte>());
        Assert.Empty(resultEmpty);
        var bufferEmpty = converter.Encode(resultEmpty);
        Assert.Empty(bufferEmpty);

        // actual implementation test
        var sourceType = source.GetType();
        var resultType = result.GetType();
        var resultEmptyType = resultEmpty.GetType();
        Assert.Equal(sourceType, resultType);
        Assert.Equal(sourceType, resultEmptyType);
    }

    public static IEnumerable<object[]> CollectionInterfaceData()
    {
        var a = new List<int> { 1 };
        var b = new List<string> { "2" };
        yield return new object[] { typeof(IList<int>), a, 1, "SequenceConverter`1.*IList`1.*Int32" };
        yield return new object[] { typeof(IList<string>), b, "2", "SequenceConverter`1.*IList`1.*String" };
        yield return new object[] { typeof(ICollection<int>), a, 1, "SequenceConverter`1.*ICollection`1.*Int32" };
        yield return new object[] { typeof(ICollection<string>), b, "2", "SequenceConverter`1.*ICollection`1.*String" };
        yield return new object[] { typeof(IEnumerable<int>), a, 1, "SequenceConverter`1.*IEnumerable`1.*Int32" };
        yield return new object[] { typeof(IEnumerable<string>), b, "2", "SequenceConverter`1.*IEnumerable`1.*String" };
        yield return new object[] { typeof(IReadOnlyList<int>), a, 1, "SequenceConverter`1.*IReadOnlyList`1.*Int32" };
        yield return new object[] { typeof(IReadOnlyList<string>), b, "2", "SequenceConverter`1.*IReadOnlyList`1.*String" };
        yield return new object[] { typeof(IReadOnlyCollection<int>), a, 1, "SequenceConverter`1.*IReadOnlyCollection`1.*Int32" };
        yield return new object[] { typeof(IReadOnlyCollection<string>), b, "2", "SequenceConverter`1.*IReadOnlyCollection`1.*String" };

        var c = new HashSet<int> { 3 };
        var d = new HashSet<string> { "4" };
        yield return new object[] { typeof(ISet<int>), c, 3, "SequenceConverter`1.*ISet`1.*Int32" };
        yield return new object[] { typeof(ISet<string>), d, "4", "SequenceConverter`1.*ISet`1.*String" };
        yield return new object[] { typeof(IReadOnlySet<int>), c, 3, "SequenceConverter`1.*IReadOnlySet`1.*Int32" };
        yield return new object[] { typeof(IReadOnlySet<string>), d, "4", "SequenceConverter`1.*IReadOnlySet`1.*String" };

        var e = new Dictionary<int, long> { { 5, 6 } };
        var f = new Dictionary<int, string> { { 7, "8" } };
        yield return new object[] { typeof(IDictionary<int, long>), e, new KeyValuePair<int, long>(5, 6), "SequenceConverter`1.*IDictionary`2.*Int32.*Int64" };
        yield return new object[] { typeof(IDictionary<int, string>), f, new KeyValuePair<int, string>(7, "8"), "SequenceConverter`1.*IDictionary`2.*Int32.*String" };
        yield return new object[] { typeof(IReadOnlyDictionary<int, long>), e, new KeyValuePair<int, long>(5, 6), "SequenceConverter`1.*IReadOnlyDictionary`2.*Int32.*Int64" };
        yield return new object[] { typeof(IReadOnlyDictionary<int, string>), f, new KeyValuePair<int, string>(7, "8"), "SequenceConverter`1.*IReadOnlyDictionary`2.*Int32.*String" };
    }

    public static IEnumerable<object[]> ImmutableCollectionInterfaceData()
    {
        var a = ImmutableDictionary.CreateRange(new[] { new KeyValuePair<short, long>(10, 20) });
        var b = ImmutableDictionary.CreateRange(new[] { new KeyValuePair<string, int>("text", 4) });
        yield return new object[] { typeof(IImmutableDictionary<short, long>), a, new KeyValuePair<short, long>(10, 20), "SequenceConverter`1.*IImmutableDictionary`2.*Int16.*Int64" };
        yield return new object[] { typeof(IImmutableDictionary<string, int>), b, new KeyValuePair<string, int>("text", 4), "SequenceConverter`1.*IImmutableDictionary`2.*String.*Int32" };

        var c = ImmutableList.Create(1);
        var d = ImmutableList.Create("2");
        yield return new object[] { typeof(IImmutableList<int>), c, 1, "SequenceConverter`1.*IImmutableList`1.*Int32" };
        yield return new object[] { typeof(IImmutableList<string>), d, "2", "SequenceConverter`1.*IImmutableList`1.*String" };

        var e = ImmutableQueue.Create(3);
        var f = ImmutableQueue.Create("4");
        yield return new object[] { typeof(IImmutableQueue<int>), e, 3, "SequenceConverter`1.*IImmutableQueue`1.*Int32" };
        yield return new object[] { typeof(IImmutableQueue<string>), f, "4", "SequenceConverter`1.*IImmutableQueue`1.*String" };

        var g = ImmutableHashSet.Create(5);
        var h = ImmutableHashSet.Create("6");
        yield return new object[] { typeof(IImmutableSet<int>), g, 5, "SequenceConverter`1.*IImmutableSet`1.*Int32" };
        yield return new object[] { typeof(IImmutableSet<string>), h, "6", "SequenceConverter`1.*IImmutableSet`1.*String" };
    }

    [Theory(DisplayName = "Collection Interface Encode Decode Test")]
    [MemberData(nameof(CollectionInterfaceData))]
    [MemberData(nameof(ImmutableCollectionInterfaceData))]
    public void CollectionInterfaceEncodeDecodeTest<T, E>(Type interfaceType, T source, E element, string pattern) where T : IEnumerable<E>
    {
        Assert.True(interfaceType.IsInterface);
        Assert.StartsWith("System.Collections", interfaceType.Namespace);
        var method = new Action<IEnumerable<object>, object, string>(EncodeDecodeTest).Method;
        var target = method.GetGenericMethodDefinition().MakeGenericMethod(new Type[] { interfaceType, typeof(E) });
        var result = target.Invoke(this, new object?[] { source, element, pattern });
        Assert.Null(result);
    }
}
