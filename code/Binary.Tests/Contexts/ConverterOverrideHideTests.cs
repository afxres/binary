namespace Mikodev.Binary.Tests.Contexts;

using Microsoft.FSharp.Core;
using System;
using Xunit;

public class ConverterOverrideHideTests
{
    private class OverrideAllMember<T> : Converter<T>
    {
        public override T Decode(in ReadOnlySpan<byte> span) => throw new NotSupportedException("Fake 'Decode'");

        public override T DecodeAuto(ref ReadOnlySpan<byte> span) => throw new NotSupportedException("Fake 'DecodeAuto'");

        public override T DecodeWithLengthPrefix(ref ReadOnlySpan<byte> span) => throw new NotSupportedException("Fake 'DecodeWithLengthPrefix'");

        public override void Encode(ref Allocator allocator, T? item) => throw new NotSupportedException("Fake 'Encode'");

        public override void EncodeAuto(ref Allocator allocator, T? item) => throw new NotSupportedException("Fake 'EncodeAuto'");

        public override void EncodeWithLengthPrefix(ref Allocator allocator, T? item) => throw new NotSupportedException("Fake 'EncodeWithLengthPrefix'");
    }

    private class OverrideHideAllMember<T> : OverrideAllMember<T>
    {
        public new virtual T Decode(in ReadOnlySpan<byte> span) => throw new NotSupportedException("New 'Decode'");

        public new virtual T DecodeAuto(ref ReadOnlySpan<byte> span) => throw new NotSupportedException("New 'DecodeAuto'");

        public new virtual T DecodeWithLengthPrefix(ref ReadOnlySpan<byte> span) => throw new NotSupportedException("New 'DecodeWithLengthPrefix'");

        public new virtual void Encode(ref Allocator allocator, T item) => throw new NotSupportedException("New 'Encode'");

        public new virtual void EncodeAuto(ref Allocator allocator, T item) => throw new NotSupportedException("New 'EncodeAuto'");

        public new virtual void EncodeWithLengthPrefix(ref Allocator allocator, T item) => throw new NotSupportedException("New 'EncodeWithLengthPrefix'");
    }

    [Fact(DisplayName = "Encode With New 'Encode' Method")]
    public void EncodeWithHiddenEncodeMethod()
    {
        var generator = Generator.CreateDefaultBuilder().AddConverter(new OverrideHideAllMember<int>()).Build();
        var converter = generator.GetConverter<ValueTuple<int>>();
        Assert.StartsWith("TupleObjectConverter`1", converter.GetType().Name);
        var error = Assert.Throws<NotSupportedException>(() =>
        {
            var allocator = new Allocator();
            converter.Encode(ref allocator, default);
        });
        Assert.Equal("Fake 'Encode'", error.Message);
    }

    [Fact(DisplayName = "Encode With New 'EncodeAuto' Method")]
    public void EncodeWithHiddenEncodeAutoMethod()
    {
        var generator = Generator.CreateDefaultBuilder().AddConverter(new OverrideHideAllMember<int>()).Build();
        var converter = generator.GetConverter<ValueTuple<int>>();
        Assert.StartsWith("TupleObjectConverter`1", converter.GetType().Name);
        var error = Assert.Throws<NotSupportedException>(() =>
        {
            var allocator = new Allocator();
            converter.EncodeAuto(ref allocator, default);
        });
        Assert.Equal("Fake 'EncodeAuto'", error.Message);
    }

    [Fact(DisplayName = "Encode With New 'EncodeWithLengthPrefix' Method")]
    public void EncodeWithHiddenEncodeWithLengthPrefixMethod()
    {
        var generator = Generator.CreateDefaultBuilder().AddConverter(new OverrideHideAllMember<int>()).Build();
        var value = new { id = 0 };
        var error = Assert.Throws<NotSupportedException>(() => generator.Encode(value));
        Assert.Equal("Fake 'EncodeWithLengthPrefix'", error.Message);
    }

    [Fact(DisplayName = "Decode With New 'Decode' Method")]
    public void DecodeWithHiddenDecodeMethod()
    {
        var generator = Generator.CreateDefaultBuilder().AddConverter(new OverrideHideAllMember<int>()).Build();
        var converter = generator.GetConverter<ValueTuple<int>>();
        Assert.StartsWith("TupleObjectConverter`1", converter.GetType().Name);
        var error = Assert.Throws<NotSupportedException>(() =>
        {
            var span = new ReadOnlySpan<byte>();
            _ = converter.Decode(in span);
        });
        Assert.Equal("Fake 'Decode'", error.Message);
    }

    [Fact(DisplayName = "Decode With New 'DecodeAuto' Method")]
    public void DecodeWithHiddenDecodeAutoMethod()
    {
        var generator = Generator.CreateDefaultBuilder().AddConverter(new OverrideHideAllMember<int>()).Build();
        var converter = generator.GetConverter<ValueTuple<int>>();
        Assert.StartsWith("TupleObjectConverter`1", converter.GetType().Name);
        var error = Assert.Throws<NotSupportedException>(() =>
        {
            var span = new ReadOnlySpan<byte>();
            _ = converter.DecodeAuto(ref span);
        });
        Assert.Equal("Fake 'DecodeAuto'", error.Message);
    }

    [Fact(DisplayName = "Encode With New 'Encode' Method For Union")]
    public void EncodeWithHiddenEncodeMethodForUnion()
    {
        var generator = Generator.CreateDefaultBuilder().AddFSharpConverterCreators().AddConverter(new OverrideHideAllMember<int>()).Build();
        var converter = generator.GetConverter<FSharpOption<int>>();
        Assert.StartsWith("UnionConverter`1", converter.GetType().Name);
        var error = Assert.Throws<NotSupportedException>(() =>
        {
            var allocator = new Allocator();
            converter.Encode(ref allocator, FSharpOption<int>.Some(0));
        });
        Assert.Equal("Fake 'Encode'", error.Message);
    }

    [Fact(DisplayName = "Encode With New 'EncodeAuto' Method For Union")]
    public void EncodeWithHiddenEncodeAutoMethodForUnion()
    {
        var generator = Generator.CreateDefaultBuilder().AddFSharpConverterCreators().AddConverter(new OverrideHideAllMember<int>()).Build();
        var converter = generator.GetConverter<FSharpOption<int>>();
        Assert.StartsWith("UnionConverter`1", converter.GetType().Name);
        var error = Assert.Throws<NotSupportedException>(() =>
        {
            var allocator = new Allocator();
            converter.EncodeAuto(ref allocator, FSharpOption<int>.Some(0));
        });
        Assert.Equal("Fake 'EncodeAuto'", error.Message);
    }

    [Fact(DisplayName = "Decode With New 'Decode' Method For Union")]
    public void DecodeWithHiddenDecodeMethodForUnion()
    {
        var generator = Generator.CreateDefaultBuilder().AddFSharpConverterCreators().AddConverter(new OverrideHideAllMember<int>()).Build();
        var converter = generator.GetConverter<FSharpOption<int>>();
        Assert.StartsWith("UnionConverter`1", converter.GetType().Name);
        var error = Assert.Throws<NotSupportedException>(() =>
        {
            var span = new ReadOnlySpan<byte>(new byte[] { FSharpOption<int>.Tags.Some });
            _ = converter.Decode(in span);
        });
        Assert.Equal("Fake 'Decode'", error.Message);
    }

    [Fact(DisplayName = "Decode With New 'DecodeAuto' Method For Union")]
    public void DecodeWithHiddenDecodeAutoMethodForUnion()
    {
        var generator = Generator.CreateDefaultBuilder().AddFSharpConverterCreators().AddConverter(new OverrideHideAllMember<int>()).Build();
        var converter = generator.GetConverter<FSharpOption<int>>();
        Assert.StartsWith("UnionConverter`1", converter.GetType().Name);
        var error = Assert.Throws<NotSupportedException>(() =>
        {
            var span = new ReadOnlySpan<byte>(new byte[] { FSharpOption<int>.Tags.Some });
            _ = converter.DecodeAuto(ref span);
        });
        Assert.Equal("Fake 'DecodeAuto'", error.Message);
    }
}
