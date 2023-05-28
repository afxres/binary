namespace Mikodev.Binary.Tests.Contexts;

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

public class ConverterStreamTests
{
    public static IEnumerable<object[]> NumberData()
    {
        yield return new object[] { 0, 1 };
        yield return new object[] { 63, 1 };
        yield return new object[] { 127, 1 };
        yield return new object[] { 128, 4 };
        yield return new object[] { 65537, 4 };
        yield return new object[] { 0x12345678, 4 };
        yield return new object[] { int.MaxValue, 4 };
    }

    [Theory(DisplayName = "Encode Decode Test")]
    [MemberData(nameof(NumberData))]
    public void EncodeDecodeTest(int number, int length)
    {
        var stream = new MemoryStream();
        Converter.Encode(stream, number);
        Assert.Equal(length, stream.Length);
        stream.Position = 0;
        var result = Converter.Decode(stream);
        Assert.Equal(number, result);
        Assert.Equal(length, stream.Position);
    }

    [Theory(DisplayName = "Encode Decode Async Test")]
    [MemberData(nameof(NumberData))]
    public async Task EncodeDecodeTestAsync(int number, int length)
    {
        var stream = new MemoryStream();
        await Converter.EncodeAsync(stream, number);
        Assert.Equal(length, stream.Length);
        stream.Position = 0;
        var result = await Converter.DecodeAsync(stream);
        Assert.Equal(number, result);
        Assert.Equal(length, stream.Position);
    }

    public static IEnumerable<object[]> NumberInvalidData()
    {
        yield return new object[] { -1 };
        yield return new object[] { int.MinValue };
    }

    [Theory(DisplayName = "Encode Invalid Number")]
    [MemberData(nameof(NumberInvalidData))]
    public void EncodeInvalidNumberTest(int number)
    {
        var stream = new MemoryStream();
        var error = Assert.Throws<ArgumentOutOfRangeException>(() => Converter.Encode(stream, number));
        Assert.Equal(0, stream.Length);
        Assert.Equal(0, stream.Capacity);
        var method = new Action<Stream, int>(Converter.Encode).Method;
        var parameters = method.GetParameters();
        Assert.Equal("number", error.ParamName);
        Assert.Equal("number", parameters[1].Name);
        Assert.StartsWith("Argument number must be greater than or equal to zero!", error.Message);
    }

    [Theory(DisplayName = "Encode Invalid Number Async")]
    [MemberData(nameof(NumberInvalidData))]
    public async Task EncodeInvalidNumberTestAsync(int number)
    {
        var stream = new MemoryStream();
        var task = Converter.EncodeAsync(stream, number).AsTask();
        var error = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => task);
        Assert.Equal(0, stream.Length);
        Assert.Equal(0, stream.Capacity);
        var method = new Func<Stream, int, CancellationToken, ValueTask>(Converter.EncodeAsync).Method;
        var parameters = method.GetParameters();
        Assert.Equal("number", error.ParamName);
        Assert.Equal("number", parameters[1].Name);
        Assert.StartsWith("Argument number must be greater than or equal to zero!", error.Message);
    }

    private sealed class TestStream : Stream
    {
        public override void Flush() => throw new NotImplementedException();

        public override int Read(byte[] buffer, int offset, int count) => throw new NotImplementedException();

        public override long Seek(long offset, SeekOrigin origin) => throw new NotImplementedException();

        public override void SetLength(long value) => throw new NotImplementedException();

        public override void Write(byte[] buffer, int offset, int count) => throw new NotImplementedException();

        public override bool CanRead => throw new NotImplementedException();

        public override bool CanSeek => throw new NotImplementedException();

        public override bool CanWrite => throw new NotImplementedException();

        public override long Length => throw new NotImplementedException();

        public override long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            Assert.True(cancellationToken.CanBeCanceled);
            cancellationToken.ThrowIfCancellationRequested();
            throw new NotSupportedException("Message 01.");
        }

        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            Assert.True(cancellationToken.CanBeCanceled);
            cancellationToken.ThrowIfCancellationRequested();
            throw new NotSupportedException("Message 02.");
        }
    }

    [Fact(DisplayName = "Encode With Cancellation Async")]
    public async Task EncodeWithCancellationTestAsync()
    {
        var stream = new TestStream();
        var source = new CancellationTokenSource();
        var task = Converter.EncodeAsync(stream, 0, source.Token).AsTask();
        var error = await Assert.ThrowsAsync<NotSupportedException>(() => task);
        Assert.Equal("Message 01.", error.Message);

        source.Cancel();
        var taskSecond = Converter.EncodeAsync(stream, 0, source.Token).AsTask();
        var exception = await Assert.ThrowsAsync<OperationCanceledException>(() => taskSecond);
        Assert.Equal(source.Token, exception.CancellationToken);
    }

    [Fact(DisplayName = "Decode With Cancellation Async")]
    public async Task DecodeWithCancellationTestAsync()
    {
        var stream = new TestStream();
        var source = new CancellationTokenSource();
        var task = Converter.DecodeAsync(stream, source.Token).AsTask();
        var error = await Assert.ThrowsAsync<NotSupportedException>(() => task);
        Assert.Equal("Message 02.", error.Message);

        source.Cancel();
        var taskSecond = Converter.DecodeAsync(stream, source.Token).AsTask();
        var exception = await Assert.ThrowsAsync<OperationCanceledException>(() => taskSecond);
        Assert.Equal(source.Token, exception.CancellationToken);
    }

    public static IEnumerable<object[]> NotEnoughBytesData()
    {
        yield return new object[] { Array.Empty<byte>() };
        yield return new object[] { new byte[] { 0x80 } };
        yield return new object[] { new byte[] { 0x80, 0 } };
        yield return new object[] { new byte[] { 0x80, 0, 1 } };
    }

    [Theory(DisplayName = "Decode Without Enough Bytes")]
    [MemberData(nameof(NotEnoughBytesData))]
    public void DecodeWithoutEnoughBytesTest(byte[] bytes)
    {
        var stream = new MemoryStream(bytes);
        _ = Assert.Throws<EndOfStreamException>(() => Converter.Decode(stream));
    }

    [Theory(DisplayName = "Decode Without Enough Bytes Async")]
    [MemberData(nameof(NotEnoughBytesData))]
    public async Task DecodeWithoutEnoughBytesTestAsync(byte[] bytes)
    {
        var stream = new MemoryStream(bytes);
        var task = Converter.DecodeAsync(stream).AsTask();
        _ = await Assert.ThrowsAsync<EndOfStreamException>(() => task);
    }
}
