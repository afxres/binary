namespace Mikodev.Binary.SourceGeneration.CollectionTests.PredefinedCollectionTests;

using Mikodev.Binary.Attributes;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Xunit;

[SourceGeneratorContext]
[SourceGeneratorInclude<ArraySegment<int>>]
[SourceGeneratorInclude<ArraySegment<string>>]
[SourceGeneratorInclude<ImmutableArray<int>>]
[SourceGeneratorInclude<ImmutableArray<string>>]
[SourceGeneratorInclude<List<int>>]
[SourceGeneratorInclude<List<string>>]
[SourceGeneratorInclude<Memory<int>>]
[SourceGeneratorInclude<Memory<string>>]
[SourceGeneratorInclude<ReadOnlyMemory<int>>]
[SourceGeneratorInclude<ReadOnlyMemory<string>>]
[SourceGeneratorInclude<LinkedList<int>>]
[SourceGeneratorInclude<LinkedList<string>>]
[SourceGeneratorInclude<HashSet<int>>]
[SourceGeneratorInclude<HashSet<string>>]
[SourceGeneratorInclude<PriorityQueue<int, string>>]
[SourceGeneratorInclude<PriorityQueue<string, int>>]
[SourceGeneratorInclude<Dictionary<int, string>>]
[SourceGeneratorInclude<Dictionary<string, int>>]
public partial class BasicSourceGeneratorContext { }

public class BasicTests
{
    public static IEnumerable<object[]> ListData()
    {
        var a = Enumerable.Range(0, 20);
        var b = Enumerable.Range(0, 100).Select(x => x.ToString());
        yield return new object[] { a.ToList(), a, "SpanLikeConverter`1.*List`1.*Int32" };
        yield return new object[] { b.ToList(), b, "SpanLikeConverter`1.*List`1.*String" };
    }

    public static IEnumerable<object[]> ImmutableArrayData()
    {
        var a = Enumerable.Range(10, 10);
        var b = Enumerable.Range(-3, 3).Select(x => x.ToString());
        yield return new object[] { a.ToImmutableArray(), a, "SpanLikeConverter`1.*ImmutableArray`1.*Int32" };
        yield return new object[] { b.ToImmutableArray(), b, "SpanLikeConverter`1.*ImmutableArray`1.*String" };
    }

    [Theory(DisplayName = "Encode Decode Test")]
    [MemberData(nameof(ListData))]
    [MemberData(nameof(ImmutableArrayData))]
    public void EncodeDecode<T, E>(T source, IEnumerable<E> expected, string pattern) where T : IEnumerable<E>
    {
        var builder = Generator.CreateAotBuilder();
        foreach (var i in BasicSourceGeneratorContext.ConverterCreators)
            _ = builder.AddConverterCreator(i.Value);
        var generator = builder.Build();
        var converter = generator.GetConverter<T>();
        var converterType = converter.GetType();
        Assert.True(converterType.Assembly == typeof(IConverter).Assembly);
        Assert.Matches(pattern, converterType.FullName);

        var generatorSecond = Generator.CreateDefault();
        var buffer = converter.Encode(source);
        var bufferSecond = generatorSecond.Encode(expected.ToArray());
        Assert.Equal(bufferSecond, buffer);

        var result = converter.Decode(bufferSecond);
        var resultSecond = generatorSecond.Decode<E[]>(buffer);
        Assert.Equal(expected, result);
        Assert.Equal(expected, resultSecond);
    }
}
