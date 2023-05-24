namespace Mikodev.Binary.Tests.Components;

using Mikodev.Binary.Components;
using Mikodev.Binary.Tests.Internal;
using System;
using System.Reflection;
using Xunit;

public class TupleObjectTests
{
    private sealed class HideConverter : IConverter
    {
        public int Length => throw new NotSupportedException();

        public void Encode(ref Allocator allocator, object? item) => throw new NotSupportedException();

        public void EncodeAuto(ref Allocator allocator, object? item) => throw new NotSupportedException();

        public void EncodeWithLengthPrefix(ref Allocator allocator, object? item) => throw new NotSupportedException();

        public byte[] Encode(object? item) => throw new NotSupportedException();

        public object? Decode(in ReadOnlySpan<byte> span) => throw new NotSupportedException();

        public object? DecodeAuto(ref ReadOnlySpan<byte> span) => throw new NotSupportedException();

        public object? DecodeWithLengthPrefix(ref ReadOnlySpan<byte> span) => throw new NotSupportedException();

        public object? Decode(byte[]? buffer) => throw new NotSupportedException();
    }

    private sealed class FakeConverter<T> : Converter<T>
    {
        public FakeConverter(int length) : base(length) { }

        public override void Encode(ref Allocator allocator, T? item) => throw new NotImplementedException();

        public override T Decode(in ReadOnlySpan<byte> span) => throw new NotImplementedException();
    }

    [Fact(DisplayName = "Argument Null Test")]
    public void ArgumentNullTest()
    {
        var methodInfo = ReflectionExtensions.GetMethodNotNull(typeof(TupleObject), "GetTupleObjectLength", BindingFlags.Static | BindingFlags.Public);
        var error = Assert.Throws<ArgumentNullException>(() => TupleObject.GetTupleObjectLength(null!));
        var parameters = methodInfo.GetParameters();
        Assert.Equal(parameters[0].Name, error.ParamName);
    }

    [Fact(DisplayName = "Overflow Test")]
    public void OverflowTest()
    {
        var converter = new FakeConverter<int>(0x4000_0000);
        var error = Assert.Throws<OverflowException>(() => TupleObject.GetTupleObjectLength(new[] { converter, converter }));
        Assert.Equal(new OverflowException().Message, error.Message);
    }

    [Fact(DisplayName = "Sequence Null Test")]
    public void SequenceNullTest()
    {
        var methodInfo = ReflectionExtensions.GetMethodNotNull(typeof(TupleObject), "GetTupleObjectLength", BindingFlags.Static | BindingFlags.Public);
        var error = Assert.Throws<ArgumentException>(() => TupleObject.GetTupleObjectLength(new IConverter[] { null! }));
        var parameters = methodInfo.GetParameters();
        Assert.Equal(parameters[0].Name, error.ParamName);
        Assert.StartsWith("Sequence contains null or invalid element.", error.Message);
    }

    [Fact(DisplayName = "Sequence Invalid Test")]
    public void SequenceInvalidTest()
    {
        var methodInfo = ReflectionExtensions.GetMethodNotNull(typeof(TupleObject), "GetTupleObjectLength", BindingFlags.Static | BindingFlags.Public);
        var error = Assert.Throws<ArgumentException>(() => TupleObject.GetTupleObjectLength(new IConverter[] { new HideConverter() }));
        var parameters = methodInfo.GetParameters();
        Assert.Equal(parameters[0].Name, error.ParamName);
        Assert.StartsWith("Sequence contains null or invalid element.", error.Message);
    }

    [Fact(DisplayName = "Sequence Empty Test")]
    public void SequenceEmptyTest()
    {
        var methodInfo = ReflectionExtensions.GetMethodNotNull(typeof(TupleObject), "GetTupleObjectLength", BindingFlags.Static | BindingFlags.Public);
        var error = Assert.Throws<ArgumentException>(() => TupleObject.GetTupleObjectLength(Array.Empty<IConverter>()));
        var parameters = methodInfo.GetParameters();
        Assert.Equal(parameters[0].Name, error.ParamName);
        Assert.StartsWith("Sequence contains no element.", error.Message);
    }
}
