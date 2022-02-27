﻿namespace Mikodev.Binary.Tests.Converters;

using System;
using System.Collections;
using System.Linq;
using Xunit;

public class BitArrayConverterTests
{
    [Fact(DisplayName = "Converter Type Name And Length")]
    public void GetConverter()
    {
        var generator = Generator.CreateDefault();
        var converter = generator.GetConverter<BitArray>();
        Assert.Equal("Mikodev.Binary.Converters.BitArrayConverter", converter.GetType().FullName);
        Assert.Equal(0, converter.Length);
    }

    [Fact(DisplayName = "Encode Decode Random Data")]
    public void BasicTest()
    {
        var random = new Random();
        var generator = Generator.CreateDefault();
        var converter = generator.GetConverter<BitArray>();

        for (var ignore = 0; ignore < 32; ignore++)
        {
            var buffer = new byte[128];
            random.NextBytes(buffer);
            for (var k = 0; k < 1024; k++)
            {
                var source = new BitArray(buffer) { Length = k };
                var encode = converter.Encode(source);
                var target = new ReadOnlySpan<byte>(encode);
                var padding = Converter.Decode(ref target);
                Assert.True(padding is >= 0 and <= 7);
                Assert.Equal((-k) & 7, padding);
                var actual = new byte[(k + 7) >> 3];
                source.CopyTo(actual, 0);
                Assert.True(new ReadOnlySpan<byte>(actual).SequenceEqual(target));

                var result = converter.Decode(encode);
                Assert.Equal(k, result.Count);
                Assert.Equal(source.Cast<bool>(), result.Cast<bool>());
            }
        }
    }
}
