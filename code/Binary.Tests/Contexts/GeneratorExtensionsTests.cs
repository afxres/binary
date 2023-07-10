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
        var bufferObject = generator.EncodeBrotli(item, typeof(T));
        Assert.Equal(buffer, bufferObject);

        var result = generator.DecodeBrotli<T>(buffer);
        var resultAnonymous = generator.DecodeBrotli(buffer, anonymous: item);
        var resultSpan = generator.DecodeBrotli<T>(new ReadOnlySpan<byte>(buffer));
        var resultSpanAnonymous = generator.DecodeBrotli(new ReadOnlySpan<byte>(buffer), anonymous: item);
        var resultObject = generator.DecodeBrotli(buffer, typeof(T));
        var resultObjectSpan = generator.DecodeBrotli(new ReadOnlySpan<byte>(buffer), typeof(T));
        Assert.Equal(item, result);
        Assert.Equal(item, resultAnonymous);
        Assert.Equal(item, resultSpan);
        Assert.Equal(item, resultSpanAnonymous);
        Assert.Equal(item, Assert.IsType<T>(resultObject));
        Assert.Equal(item, Assert.IsType<T>(resultObjectSpan));
    }
}
