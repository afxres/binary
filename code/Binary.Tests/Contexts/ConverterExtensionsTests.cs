namespace Mikodev.Binary.Tests.Contexts;

using Mikodev.Binary.Attributes;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using Xunit;

public class ConverterExtensionsTests
{
    [NamedObject]
    private record TestNamedObject([property: NamedKey("id")] int Id, [property: NamedKey("text")] string Text);

    [TupleObject]
    private record TestTupleObject([property: TupleKey(0)] int Id, [property: TupleKey(1)] string Text);

    public static IEnumerable<object[]> TestNamedObjectData()
    {
        static List<TestNamedObject> Create(int count) => Enumerable.Range(0, count).Select(x => new TestNamedObject(x, x.ToString())).ToList();

        yield return new object[] { new List<TestNamedObject>() };
        yield return new object[] { Create(1) };
        yield return new object[] { Create(1_000) };
        yield return new object[] { Create(100_000) };
    }

    public static IEnumerable<object[]> TestTupleObjectData()
    {
        static List<TestTupleObject> Create(int count) => Enumerable.Range(0, count).Select(x => new TestTupleObject(x, x.ToString())).ToList();

        yield return new object[] { new List<TestTupleObject>() };
        yield return new object[] { Create(1) };
        yield return new object[] { Create(1_000) };
        yield return new object[] { Create(100_000) };
    }

    [Theory(DisplayName = "Encode Decode Brotli Test")]
    [MemberData(nameof(TestNamedObjectData))]
    [MemberData(nameof(TestTupleObjectData))]
    public void EncodeDecodeBrotliTest<T>(T data)
    {
        var generator = Generator.CreateDefault();
        var converter = generator.GetConverter<T>();
        var buffer = converter.EncodeBrotli(data);
        var result = converter.DecodeBrotli(buffer);
        Assert.Equal(data, result);

        var bufferOrigin = converter.Encode(data);
        var bufferZipped = new byte[buffer.Length];
        var status = BrotliEncoder.TryCompress(bufferOrigin, bufferZipped, out var bytesWritten, quality: 1, window: 22);
        Assert.True(status);
        Assert.Equal(buffer.Length, bytesWritten);
        Assert.Equal(buffer, bufferZipped);

        var bufferResult = new byte[bufferOrigin.Length];
        var statusResult = BrotliDecoder.TryDecompress(bufferZipped, bufferResult, out var bytesWrittenResult);
        Assert.True(statusResult);
        Assert.Equal(bufferOrigin.Length, bytesWrittenResult);
        Assert.Equal(bufferOrigin, bufferResult);
    }
}
