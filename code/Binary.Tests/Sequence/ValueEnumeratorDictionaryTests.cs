namespace Mikodev.Binary.Tests.Sequence;

using Mikodev.Binary.Tests.Contexts;
using Mikodev.Binary.Tests.Sequence.Models;
using System.Collections.Generic;
using System.Linq;
using Xunit;

public class ValueEnumeratorDictionaryTests
{
    public static IEnumerable<object[]> KeyValuePairData()
    {
        var source = Enumerable.Range(0, 100).ToList();
        yield return new object[] { source.Select(x => KeyValuePair.Create(x, x)).ToList() };
        yield return new object[] { source.Select(x => KeyValuePair.Create(x, x.ToString())).ToList() };
        yield return new object[] { source.Select(x => KeyValuePair.Create(x.ToString(), x)).ToList() };
        yield return new object[] { source.Select(x => KeyValuePair.Create(x.ToString(), x.ToString())).ToList() };
    }

    [Theory(DisplayName = "Value Enumerator Dictionary Test")]
    [MemberData(nameof(KeyValuePairData))]
    public void EncodeDecode<K, V>(List<KeyValuePair<K, V>> values) where K : notnull
    {
        var generator = Generator.CreateDefault();
        var converter = generator.GetConverter<Dictionary<K, V>>();
        var source = new Dictionary<K, V>(values);
        var buffer = converter.Encode(source);
        var result = converter.Decode(buffer);
        Assert.Equal(source, result);
        ConverterTests.TestVariableEncodeDecodeMethods(converter, source);
    }

    [Theory(DisplayName = "Value Enumerator Custom Dictionary Test")]
    [MemberData(nameof(KeyValuePairData))]
    public void EncodeDecodeCustomDictionary<K, V>(List<KeyValuePair<K, V>> values) where K : notnull
    {
        var generator = Generator.CreateDefault();
        var converter = generator.GetConverter<CustomValueEnumeratorDictionary<K, V>>();
        var source = new CustomValueEnumeratorDictionary<K, V>(values);
        Assert.Equal(0, source.CurrentCallCount);
        Assert.Equal(0, source.MoveNextCallCount);
        var buffer = converter.Encode(source);
        var result = converter.Decode(buffer);
        Assert.Equal(source.Items.Count, source.CurrentCallCount);
        Assert.Equal(source.Items.Count + 1, source.MoveNextCallCount);
        Assert.Equal(source.Items, result.Items);
    }
}
