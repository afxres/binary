namespace Mikodev.Binary.Tests.Sequence;

using Mikodev.Binary.Tests.Sequence.Models;
using System.Collections.Generic;
using System.Linq;
using Xunit;

public class ByRefLikeEnumeratorCollectionTests
{
    public static IEnumerable<object[]> TestData()
    {
        var source = Enumerable.Range(0, 100).ToList();
        yield return [source];
        yield return [source.Select(x => x.ToString()).ToList()];
    }

    [Theory(DisplayName = "Stack Only Value Enumerator Custom Collection Test")]
    [MemberData(nameof(TestData))]
    public void EncodeDecodeCustomCollection<T>(List<T> values)
    {
        var generator = Generator.CreateDefault();
        var converter = generator.GetConverter<CustomByRefLikeEnumeratorCollection<T>>();
        var source = new CustomByRefLikeEnumeratorCollection<T>(values);
        Assert.Equal(0, source.CurrentCallCount);
        Assert.Equal(0, source.MoveNextCallCount);
        var buffer = converter.Encode(source);
        var result = converter.Decode(buffer);
        Assert.Equal(source.Items.Count, source.CurrentCallCount);
        Assert.Equal(source.Items.Count + 1, source.MoveNextCallCount);
        Assert.Equal(source.Items, result.Items);
    }
}
