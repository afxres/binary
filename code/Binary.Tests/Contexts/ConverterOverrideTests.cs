namespace Mikodev.Binary.Tests.Contexts;

using System;
using Xunit;

public class ConverterOverrideTests
{
    private class OnlyOverrideEncodeWithLengthPrefix<T> : Converter<T>
    {
        public OnlyOverrideEncodeWithLengthPrefix() { }

        public OnlyOverrideEncodeWithLengthPrefix(int length) : base(length) { }

        public override T Decode(in ReadOnlySpan<byte> span) => throw new NotSupportedException();

        public override void Encode(ref Allocator allocator, T? item) => throw new NotSupportedException();

        public override void EncodeWithLengthPrefix(ref Allocator allocator, T? item) => throw new NotSupportedException();
    }

    [Fact(DisplayName = "Only Override 'EncodeWithLengthPrefix'")]
    public void OverrideEncodeWithLengthPrefixMethod()
    {
        var alpha = Assert.Throws<InvalidOperationException>(() => new OnlyOverrideEncodeWithLengthPrefix<int>());
        var bravo = Assert.Throws<InvalidOperationException>(() => new OnlyOverrideEncodeWithLengthPrefix<int>(4));
        var message = $"Method 'EncodeAuto' should be override if method 'EncodeWithLengthPrefix' is overridden, type: {typeof(OnlyOverrideEncodeWithLengthPrefix<int>)}";
        Assert.Equal(message, alpha.Message);
        Assert.Equal(message, bravo.Message);
    }

    private class OnlyOverrideDecodeWithLengthPrefix<T> : Converter<T>
    {
        public OnlyOverrideDecodeWithLengthPrefix() { }

        public OnlyOverrideDecodeWithLengthPrefix(int length) : base(length) { }

        public override T Decode(in ReadOnlySpan<byte> span) => throw new NotSupportedException();

        public override T DecodeWithLengthPrefix(ref ReadOnlySpan<byte> span) => throw new NotSupportedException();

        public override void Encode(ref Allocator allocator, T? item) => throw new NotSupportedException();
    }

    [Fact(DisplayName = "Only Override 'DecodeWithLengthPrefix'")]
    public void OverrideDecodeWithLengthPrefixMethod()
    {
        var alpha = Assert.Throws<InvalidOperationException>(() => new OnlyOverrideDecodeWithLengthPrefix<int>());
        var bravo = Assert.Throws<InvalidOperationException>(() => new OnlyOverrideDecodeWithLengthPrefix<int>(4));
        var message = $"Method 'DecodeAuto' should be override if method 'DecodeWithLengthPrefix' is overridden, type: {typeof(OnlyOverrideDecodeWithLengthPrefix<int>)}";
        Assert.Equal(message, alpha.Message);
        Assert.Equal(message, bravo.Message);
    }
}
