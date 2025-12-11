namespace Mikodev.Binary.SourceGeneration.CollectionTests.SupportedTypesTests;

using Mikodev.Binary.Attributes;
using System;
using System.Collections.Frozen;
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
[SourceGeneratorInclude<FrozenSet<int>>]
[SourceGeneratorInclude<FrozenSet<string>>]
[SourceGeneratorInclude<FrozenDictionary<int, string>>]
[SourceGeneratorInclude<FrozenDictionary<string, int>>]
public partial class IntegrationGeneratorContext { }

public class IntegrationTests
{
    private static readonly string BimodalArrayBasedStringConverterName = BitConverter.IsLittleEndian ? "ArrayBasedNativeEndianConverter`3" : "ArrayBasedConverter`3";

    private static readonly string BimodalListConverterName = BitConverter.IsLittleEndian ? "ListNativeEndianConverter`1" : "ListConverter`1";

    public static IEnumerable<object[]> EnumerableTypesData()
    {
        yield return [new List<int> { 1 }, 1, BimodalListConverterName + ".*Int32"];
        yield return [new List<string> { "2" }, "2", "ListConverter`1.*String"];
        yield return [new HashSet<int> { 3 }, 3, "SequenceConverter`1.*HashSet`1.*Int32"];
        yield return [new HashSet<string> { "4" }, "4", "SequenceConverter`1.*HashSet`1.*String"];
        yield return [new Dictionary<int, long> { { 5, 6 } }, new KeyValuePair<int, long>(5, 6), "SequenceConverter`1.*Dictionary`2.*Int32.*Int64"];
        yield return [new Dictionary<int, string> { { 7, "8" } }, new KeyValuePair<int, string>(7, "8"), "SequenceConverter`1.*Dictionary`2.*Int32.*String"];
        yield return [ImmutableArray.Create(1), 1, BimodalArrayBasedStringConverterName + ".*ImmutableArrayAdapter`1.*Int32"];
        yield return [ImmutableArray.Create("2"), "2", "ArrayBasedConverter`3.*ImmutableArrayAdapter`1.*String"];
        yield return [new LinkedList<int>([3]), 3, "LinkedListConverter`1.*Int32"];
        yield return [new LinkedList<string>(["4"]), "4", "LinkedListConverter`1.*String"];
    }

    public static IEnumerable<object[]> ImmutableCollectionTypesData()
    {
        yield return [ImmutableDictionary.CreateRange([new KeyValuePair<short, long>(10, 20)]), new KeyValuePair<short, long>(10, 20), "ImmutableDictionary.*Int16.*Int64"];
        yield return [ImmutableDictionary.CreateRange([new KeyValuePair<string, int>("text", 4)]), new KeyValuePair<string, int>("text", 4), "ImmutableDictionary.*String.*Int32"];
        yield return [ImmutableHashSet.Create(1), 1, "ImmutableHashSet.*Int32"];
        yield return [ImmutableHashSet.Create("2"), "2", "ImmutableHashSet.*String"];
        yield return [ImmutableList.Create(3), 3, "ImmutableList.*Int32"];
        yield return [ImmutableList.Create("4"), "4", "ImmutableList.*String"];
        yield return [ImmutableQueue.Create(5), 5, "ImmutableQueue.*Int32"];
        yield return [ImmutableQueue.Create("6"), "6", "ImmutableQueue.*String"];
        yield return [ImmutableSortedDictionary.CreateRange([new KeyValuePair<int, long>(40, 60)]), new KeyValuePair<int, long>(40, 60), "ImmutableSortedDictionary.*Int32.*Int64"];
        yield return [ImmutableSortedDictionary.CreateRange([new KeyValuePair<int, string>(6, "sorted")]), new KeyValuePair<int, string>(6, "sorted"), "ImmutableSortedDictionary.*Int32.*String"];
        yield return [ImmutableSortedSet.Create(7), 7, "ImmutableSortedSet.*Int32"];
        yield return [ImmutableSortedSet.Create("8"), "8", "ImmutableSortedSet.*String"];
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

        // default or null collection test
        var bufferDefault = converter.Encode(default);
        Assert.Empty(bufferDefault);
    }

    public static IEnumerable<object[]> CollectionInterfaceData()
    {
        var a = new List<int> { 1 };
        var b = new List<string> { "2" };
        yield return [typeof(IList<int>), a, 1, ".*IList.*Int32"];
        yield return [typeof(IList<string>), b, "2", ".*IList.*String"];
        yield return [typeof(ICollection<int>), a, 1, ".*ICollection.*Int32"];
        yield return [typeof(ICollection<string>), b, "2", ".*ICollection.*String"];
        yield return [typeof(IEnumerable<int>), a, 1, ".*IEnumerable.*Int32"];
        yield return [typeof(IEnumerable<string>), b, "2", ".*IEnumerable.*String"];
        yield return [typeof(IReadOnlyList<int>), a, 1, ".*IReadOnlyList.*Int32"];
        yield return [typeof(IReadOnlyList<string>), b, "2", ".*IReadOnlyList.*String"];
        yield return [typeof(IReadOnlyCollection<int>), a, 1, ".*IReadOnlyCollection.*Int32"];
        yield return [typeof(IReadOnlyCollection<string>), b, "2", ".*IReadOnlyCollection.*String"];

        var c = new HashSet<int> { 3 };
        var d = new HashSet<string> { "4" };
        yield return [typeof(ISet<int>), c, 3, ".*ISet.*Int32"];
        yield return [typeof(ISet<string>), d, "4", ".*ISet.*String"];
        yield return [typeof(IReadOnlySet<int>), c, 3, ".*IReadOnlySet.*Int32"];
        yield return [typeof(IReadOnlySet<string>), d, "4", ".*IReadOnlySet.*String"];

        var e = new Dictionary<int, long> { { 5, 6 } };
        var f = new Dictionary<int, string> { { 7, "8" } };
        yield return [typeof(IDictionary<int, long>), e, new KeyValuePair<int, long>(5, 6), ".*IDictionary.*Int32.*Int64"];
        yield return [typeof(IDictionary<int, string>), f, new KeyValuePair<int, string>(7, "8"), ".*IDictionary.*Int32.*String"];
        yield return [typeof(IReadOnlyDictionary<int, long>), e, new KeyValuePair<int, long>(5, 6), ".*IReadOnlyDictionary.*Int32.*Int64"];
        yield return [typeof(IReadOnlyDictionary<int, string>), f, new KeyValuePair<int, string>(7, "8"), ".*IReadOnlyDictionary.*Int32.*String"];
    }

    public static IEnumerable<object[]> ImmutableCollectionInterfaceData()
    {
        var a = ImmutableDictionary.CreateRange([new KeyValuePair<short, long>(10, 20)]);
        var b = ImmutableDictionary.CreateRange([new KeyValuePair<string, int>("text", 4)]);
        yield return [typeof(IImmutableDictionary<short, long>), a, new KeyValuePair<short, long>(10, 20), ".*IImmutableDictionary.*Int16.*Int64"];
        yield return [typeof(IImmutableDictionary<string, int>), b, new KeyValuePair<string, int>("text", 4), ".*IImmutableDictionary.*String.*Int32"];

        var c = ImmutableList.Create(1);
        var d = ImmutableList.Create("2");
        yield return [typeof(IImmutableList<int>), c, 1, ".*IImmutableList.*Int32"];
        yield return [typeof(IImmutableList<string>), d, "2", ".*IImmutableList.*String"];

        var e = ImmutableQueue.Create(3);
        var f = ImmutableQueue.Create("4");
        yield return [typeof(IImmutableQueue<int>), e, 3, ".*IImmutableQueue.*Int32"];
        yield return [typeof(IImmutableQueue<string>), f, "4", ".*IImmutableQueue.*String"];

        var g = ImmutableHashSet.Create(5);
        var h = ImmutableHashSet.Create("6");
        yield return [typeof(IImmutableSet<int>), g, 5, ".*IImmutableSet.*Int32"];
        yield return [typeof(IImmutableSet<string>), h, "6", ".*IImmutableSet.*String"];
    }

    public static IEnumerable<object[]> FrozenCollectionAbstractClassData()
    {
        var a = FrozenDictionary.ToFrozenDictionary([new KeyValuePair<int, string>(1, "2")]);
        var b = FrozenDictionary.ToFrozenDictionary([new KeyValuePair<string, int>("3", 4)]);
        yield return [typeof(FrozenDictionary<int, string>), a, new KeyValuePair<int, string>(1, "2"), ".*FrozenDictionary.*Int32.*String"];
        yield return [typeof(FrozenDictionary<string, int>), b, new KeyValuePair<string, int>("3", 4), ".*FrozenDictionary.*String.*Int32"];

        var c = FrozenSet.ToFrozenSet([5]);
        var d = FrozenSet.ToFrozenSet(["6"]);
        yield return [typeof(FrozenSet<int>), c, 5, ".*FrozenSet.*Int32"];
        yield return [typeof(FrozenSet<string>), d, "6", ".*FrozenSet.*String"];
    }

    [Theory(DisplayName = "Collection Interface Or Abstract Class Encode Decode Test")]
    [MemberData(nameof(CollectionInterfaceData))]
    [MemberData(nameof(ImmutableCollectionInterfaceData))]
    [MemberData(nameof(FrozenCollectionAbstractClassData))]
    public void CollectionInterfaceOrAbstractClassEncodeDecodeTest<T, E>(Type wantedType, T source, E element, string pattern) where T : IEnumerable<E>
    {
        Assert.True(wantedType.IsAbstract || wantedType.IsInterface);
        Assert.StartsWith("System.Collections", wantedType.Namespace);
        var method = new Action<IEnumerable<object>, object, string>(EncodeDecodeTest).Method;
        var target = method.GetGenericMethodDefinition().MakeGenericMethod([wantedType, typeof(E)]);
        var result = target.Invoke(this, [source, element, pattern]);
        Assert.Null(result);
    }
}
