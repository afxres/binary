namespace Mikodev.Binary.Tests.Contexts;

using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

public class GeneratorExtensionsTests
{
    public static IEnumerable<object[]> TestData()
    {
        yield return new object[] { Array.Empty<int>() };
        yield return new object[] { Array.Empty<string>() };
        yield return new object[] { Enumerable.Range(0, 1).ToArray() };
        yield return new object[] { Enumerable.Range(0, 1).Select(x => x.ToString()).ToArray() };
        yield return new object[] { Enumerable.Range(0, 10_000).ToArray() };
        yield return new object[] { Enumerable.Range(0, 10_000).Select(x => x.ToString()).ToArray() };
    }

    [Theory(DisplayName = "Encode Decode Brotli Test")]
    [MemberData(nameof(TestData))]
    public void EncodeDecodeBrotliTest<T>(T item)
    {
        var generator = Generator.CreateDefault();
        var buffer = generator.EncodeBrotli(item);
        var result = generator.DecodeBrotli<T>(buffer);
        var resultAnonymous = generator.DecodeBrotli(buffer, anonymous: item);
        Assert.Equal(item, result);
        Assert.Equal(item, resultAnonymous);
    }
}
