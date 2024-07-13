namespace Mikodev.Binary.Tests.Contexts;

using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

public class AllocatorStringTests
{
    private delegate int GetByteCountDelegate(ReadOnlySpan<char> chars);

    private delegate int GetBytesDelegate(ReadOnlySpan<char> chars, Span<byte> bytes);

    private class FakeEncoding : Encoding
    {
        public required Func<int, int> GetMaxByteCountCallback { get; init; }

        public required GetByteCountDelegate GetByteCountCallback { get; init; }

        public required GetBytesDelegate GetBytesCallback { get; init; }

        public override int GetByteCount(char[] chars, int index, int count) => throw new NotSupportedException();

        public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex) => throw new NotSupportedException();

        public override int GetCharCount(byte[] bytes, int index, int count) => throw new NotSupportedException();

        public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex) => throw new NotSupportedException();

        public override int GetMaxCharCount(int byteCount) => throw new NotSupportedException();

        public override int GetMaxByteCount(int charCount) => GetMaxByteCountCallback.Invoke(charCount);

        public override int GetByteCount(ReadOnlySpan<char> chars) => GetByteCountCallback.Invoke(chars);

        public override int GetBytes(ReadOnlySpan<char> chars, Span<byte> bytes) => GetBytesCallback.Invoke(chars, bytes);
    }

    public static IEnumerable<object[]> StringData()
    {
        yield return new object[] { string.Empty };
        yield return new object[] { "Alpha" };
        yield return new object[] { "一二三四" };
    }

    [Theory(DisplayName = "Append String UTF8 Encoding")]
    [MemberData(nameof(StringData))]
    public void AppendStringUTF8Encoding(string text)
    {
        var encoding = Encoding.UTF8;
        var allocator = new Allocator();
        Assert.Equal(0, allocator.Length);
        Assert.Equal(0, allocator.Capacity);
        Allocator.Append(ref allocator, text, encoding);
        var expected = encoding.GetBytes(text);
        Assert.Equal(expected, allocator.ToArray());
    }

    [Theory(DisplayName = "Append String With Length Prefix UTF8 Encoding")]
    [MemberData(nameof(StringData))]
    public void AppendStringWithLengthPrefixUTF8Encoding(string text)
    {
        var encoding = Encoding.UTF8;
        var allocator = new Allocator();
        Assert.Equal(0, allocator.Length);
        Assert.Equal(0, allocator.Capacity);
        Allocator.AppendWithLengthPrefix(ref allocator, text, encoding);
        var expected = encoding.GetBytes(text);
        var buffer = allocator.AsSpan();
        var result = Converter.DecodeWithLengthPrefix(ref buffer);
        Assert.Equal(0, buffer.Length);
        Assert.Equal(expected, result.ToArray());
    }

    [Theory(DisplayName = "Append String UTF8 Encoding Medium Length Test")]
    [InlineData(72, 96, 96)]
    [InlineData(72, 0, 256)]
    [InlineData(72, 1024, 1024)]
    public void AppendStringUTF8EncodingMediumLengthTest(int stringLength, int allocatorInitialCapacity, int allocatorFinalCapacity)
    {
        var encoding = Encoding.UTF8;
        var text = new string('a', stringLength);
        var allocator = new Allocator(new Span<byte>(new byte[allocatorInitialCapacity]));
        Assert.True(encoding.GetByteCount(text) < 128);
        Assert.True(encoding.GetMaxByteCount(text.Length) > 128);
        Allocator.Append(ref allocator, text, encoding);
        Assert.Equal(stringLength, allocator.Length);
        Assert.Equal(allocatorFinalCapacity, allocator.Capacity);
    }

    [Theory(DisplayName = "Append String With Length Prefix UTF8 Encoding Medium Length Test")]
    [InlineData(48, 80, 80, 1)]
    [InlineData(48, 0, 256, 1)]
    [InlineData(48, 1024, 1024, 4)]
    public void AppendStringWithLengthPrefixUTF8EncodingMediumLengthTest(int stringLength, int allocatorInitialCapacity, int allocatorFinalCapacity, int prefixLength)
    {
        var encoding = Encoding.UTF8;
        var text = new string('a', stringLength);
        var allocator = new Allocator(new Span<byte>(new byte[allocatorInitialCapacity]));
        Assert.True(encoding.GetByteCount(text) < 128);
        Assert.True(encoding.GetMaxByteCount(text.Length) > 128);
        Allocator.AppendWithLengthPrefix(ref allocator, text, encoding);
        var buffer = allocator.AsSpan();
        var actualIntentLength = Converter.Decode(buffer, out var actualPrefixLength);
        Assert.Equal(stringLength, actualIntentLength);
        Assert.Equal(prefixLength, actualPrefixLength);
        Assert.Equal(stringLength + prefixLength, allocator.Length);
        Assert.Equal(allocatorFinalCapacity, allocator.Capacity);
    }

    [Theory(DisplayName = "Append String Fake Encoding Invalid 'GetBytes()' Return Test")]
    [InlineData(1024, 256, -1, -1)]
    [InlineData(1024, 256, -1, 257)]
    [InlineData(256, 256, -1, -1)]
    [InlineData(256, 256, -1, 257)]
    [InlineData(0, 128, 0, 1)]
    [InlineData(0, 128, 0, -1)]
    public void AppendStringFakeEncodingInvalidGetBytesReturnTest(int allocatorInitialCapacity, int getMaxByteCountReturn, int getByteCountReturn, int getBytesReturn)
    {
        var encoding = new FakeEncoding
        {
            GetMaxByteCountCallback = _ => getMaxByteCountReturn,
            GetByteCountCallback = getByteCountReturn is -1 ? (_ => throw new NotSupportedException()) : (_ => getByteCountReturn),
            GetBytesCallback = (_, _) => getBytesReturn,
        };
        var error = Assert.Throws<InvalidOperationException>(() =>
        {
            var allocator = new Allocator(new Span<byte>(new byte[allocatorInitialCapacity]));
            Assert.Equal(0, allocator.Length);
            Assert.Equal(allocatorInitialCapacity, allocator.Capacity);
            Allocator.Append(ref allocator, string.Empty, encoding);
        });
        Assert.Equal("Invalid return value.", error.Message);
    }

    [Theory(DisplayName = "Append String With Length Prefix Fake Encoding Invalid 'GetBytes()' Return Test")]
    [InlineData(196, 192, -1, -1)]
    [InlineData(196, 192, -1, 193)]
    [InlineData(192, 192, 192, -1)]
    [InlineData(192, 192, 192, 193)]
    [InlineData(0, 1, 0, 1)]
    [InlineData(0, 1, 0, -1)]
    public void AppendStringWithLengthPrefixInvalidGetBytesReturnTest(int allocatorInitialCapacity, int getMaxByteCountReturn, int getByteCountReturn, int getBytesReturn)
    {
        var encoding = new FakeEncoding
        {
            GetMaxByteCountCallback = _ => getMaxByteCountReturn,
            GetByteCountCallback = getByteCountReturn is -1 ? (_ => throw new NotSupportedException()) : (_ => getByteCountReturn),
            GetBytesCallback = (_, _) => getBytesReturn,
        };
        var error = Assert.Throws<InvalidOperationException>(() =>
        {
            var allocator = new Allocator(new Span<byte>(new byte[allocatorInitialCapacity]));
            Assert.Equal(0, allocator.Length);
            Assert.Equal(allocatorInitialCapacity, allocator.Capacity);
            Allocator.AppendWithLengthPrefix(ref allocator, string.Empty, encoding);
        });
        Assert.Equal("Invalid return value.", error.Message);
    }

    [Theory(DisplayName = "Append String Fake Encoding Test")]
    [InlineData(80, 80, 80, 80, -1, 80)]
    [InlineData(80, 80, 20, 80, -1, 20)]
    [InlineData(80, 80, 20, 81, 20, 20)]
    [InlineData(80, 160, 81, 81, 81, 81)]
    [InlineData(80, 160, 81, 320, 81, 81)]
    public void AppendStringFakeEncodingTest(int allocatorInitialCapacity, int allocatorFinalCapacity, int allocatorFinalLength, int getMaxByteCountReturn, int getByteCountReturn, int getBytesReturn)
    {
        var encoding = new FakeEncoding
        {
            GetMaxByteCountCallback = _ => getMaxByteCountReturn,
            GetByteCountCallback = getByteCountReturn is -1 ? (_ => throw new NotSupportedException()) : (_ => getByteCountReturn),
            GetBytesCallback = (_, _) => getBytesReturn,
        };
        var allocator = new Allocator(new Span<byte>(new byte[allocatorInitialCapacity]));
        Assert.Equal(0, allocator.Length);
        Assert.Equal(allocatorInitialCapacity, allocator.Capacity);
        Allocator.Append(ref allocator, string.Empty, encoding);
        Assert.Equal(allocatorFinalLength, allocator.Length);
        Assert.Equal(allocatorFinalCapacity, allocator.Capacity);
    }

    [Theory(DisplayName = "Append String With Length Prefix Fake Encoding Test")]
    [InlineData(120, 120, 117, 116, -1, 116, 1)]
    [InlineData(120, 120, 101, 116, -1, 100, 1)]
    [InlineData(120, 120, 101, 117, 100, 100, 1)]
    [InlineData(120, 240, 118, 117, 117, 117, 1)]
    [InlineData(120, 240, 118, 240, 117, 117, 1)]
    [InlineData(120, 240, 134, 240, 130, 130, 4)]
    [InlineData(120, 240, 134, 480, 130, 130, 4)]
    public void AppendStringWithLengthPrefixFakeEncodingTest(int allocatorInitialCapacity, int allocatorFinalCapacity, int allocatorFinalLength, int getMaxByteCountReturn, int getByteCountReturn, int getBytesReturn, int prefixLength)
    {
        var encoding = new FakeEncoding
        {
            GetMaxByteCountCallback = _ => getMaxByteCountReturn,
            GetByteCountCallback = getByteCountReturn is -1 ? (_ => throw new NotSupportedException()) : (_ => getByteCountReturn),
            GetBytesCallback = (_, _) => getBytesReturn,
        };
        var allocator = new Allocator(new Span<byte>(new byte[allocatorInitialCapacity]));
        Assert.Equal(0, allocator.Length);
        Assert.Equal(allocatorInitialCapacity, allocator.Capacity);
        Allocator.AppendWithLengthPrefix(ref allocator, string.Empty, encoding);
        Assert.Equal(allocatorFinalLength, allocator.Length);
        Assert.Equal(allocatorFinalCapacity, allocator.Capacity);
        var buffer = allocator.AsSpan();
        var length = Converter.Decode(buffer, out var bytesRead);
        Assert.Equal(getBytesReturn, length);
        Assert.Equal(prefixLength, bytesRead);
    }

    [Theory(DisplayName = "Append String Fake Encoding Invalid Return Value Test")]
    [InlineData(-1)]
    [InlineData(-4)]
    [InlineData(-5)]
    [InlineData(int.MinValue)]
    public void AppendStringFakeEncodingInvalidReturnValueTest(int byteCount)
    {
        var a = Assert.Throws<NotSupportedException>(() =>
        {
            var encoding = new FakeEncoding
            {
                GetMaxByteCountCallback = _ => byteCount,
                GetByteCountCallback = _ => throw new NotSupportedException("Tag A"),
                GetBytesCallback = (_, _) => throw new NotSupportedException(),
            };
            var allocator = new Allocator(new Span<byte>(new byte[3]));
            Assert.Equal(3, allocator.Capacity);
            Allocator.Append(ref allocator, string.Empty, encoding);
        });
        Assert.Equal("Tag A", a.Message);

        var b = Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            var encoding = new FakeEncoding
            {
                GetMaxByteCountCallback = _ => 1024,
                GetByteCountCallback = _ => byteCount,
                GetBytesCallback = (_, _) => throw new NotSupportedException(),
            };
            var allocator = new Allocator(new Span<byte>(new byte[3]));
            Assert.Equal(3, allocator.Capacity);
            Allocator.Append(ref allocator, string.Empty, encoding);
        });
        Assert.Equal("length", b.ParamName);
    }

    [Theory(DisplayName = "Append String With Length Prefix Fake Encoding Invalid Return Value Test")]
    [InlineData(-5)]
    [InlineData(int.MinValue)]
    public void AppendStringWithLengthPrefixFakeEncodingInvalidReturnValueTest(int byteCount)
    {
        var a = Assert.Throws<NotSupportedException>(() =>
        {
            var encoding = new FakeEncoding
            {
                GetMaxByteCountCallback = _ => byteCount,
                GetByteCountCallback = _ => throw new NotSupportedException("Tag A"),
                GetBytesCallback = (_, _) => throw new NotSupportedException(),
            };
            var allocator = new Allocator(new Span<byte>(new byte[3]));
            Assert.Equal(3, allocator.Capacity);
            Allocator.AppendWithLengthPrefix(ref allocator, string.Empty, encoding);
        });
        Assert.Equal("Tag A", a.Message);

        var b = Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            var encoding = new FakeEncoding
            {
                GetMaxByteCountCallback = _ => 1024,
                GetByteCountCallback = _ => byteCount,
                GetBytesCallback = (_, _) => throw new NotSupportedException(),
            };
            var allocator = new Allocator(new Span<byte>(new byte[3]));
            Assert.Equal(3, allocator.Capacity);
            Allocator.AppendWithLengthPrefix(ref allocator, string.Empty, encoding);
        });
        Assert.Equal("length", b.ParamName);
    }

    [Theory(DisplayName = "Append String With Length Prefix Fake Encoding Special Invalid Return Value Test")]
    [InlineData(-1)]
    [InlineData(-2)]
    [InlineData(-3)]
    [InlineData(-4)]
    public void AppendStringWithLengthPrefixFakeEncodingSpecialInvalidReturnValueTest(int byteCount)
    {
        _ = Assert.Throws<InvalidOperationException>(() =>
        {
            var encoding = new FakeEncoding
            {
                GetMaxByteCountCallback = _ => byteCount,
                GetByteCountCallback = _ => throw new NotSupportedException(),
                GetBytesCallback = (_, _) => throw new NotSupportedException(),
            };
            var allocator = new Allocator(new Span<byte>(new byte[3]));
            Assert.Equal(3, allocator.Capacity);
            Allocator.AppendWithLengthPrefix(ref allocator, string.Empty, encoding);
        });

        _ = Assert.Throws<InvalidOperationException>(() =>
        {
            var encoding = new FakeEncoding
            {
                GetMaxByteCountCallback = _ => 1024,
                GetByteCountCallback = _ => byteCount,
                GetBytesCallback = (_, _) => throw new NotSupportedException(),
            };
            var allocator = new Allocator(new Span<byte>(new byte[3]));
            Assert.Equal(3, allocator.Capacity);
            Allocator.AppendWithLengthPrefix(ref allocator, string.Empty, encoding);
        });
    }
}
