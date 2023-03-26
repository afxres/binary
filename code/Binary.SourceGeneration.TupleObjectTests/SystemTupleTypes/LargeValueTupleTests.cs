namespace Mikodev.Binary.SourceGeneration.TupleObjectTests.SystemTupleTypes;

using Mikodev.Binary.Attributes;
using System;
using Xunit;

[SourceGeneratorContext]
[SourceGeneratorInclude<(byte, sbyte, ushort, short, uint, int, ulong, long, UInt128, Int128)>]
public partial class LargeValueTupleSourceGeneratorContext { }

public class LargeValueTupleTests
{
    [Fact(DisplayName = "Large Value Tuple")]
    public void LargeValueTuple()
    {
        var creators = LargeValueTupleSourceGeneratorContext.ConverterCreators;
        Assert.Equal(2, creators.Count);
        Assert.Contains(typeof(ValueTuple<long, UInt128, Int128>), creators.Keys);
        Assert.Contains(typeof(ValueTuple<byte, sbyte, ushort, short, uint, int, ulong, ValueTuple<long, UInt128, Int128>>), creators.Keys);

        var builder = Generator.CreateDefaultBuilder();
        foreach (var i in creators)
            _ = builder.AddConverterCreator(i.Value);
        var generator = builder.Build();
        var converter = generator.GetConverter<(byte, sbyte, ushort, short, uint, int, ulong, long, UInt128, Int128)>();
        Assert.Equal(converter.GetType().Assembly, typeof(LargeValueTupleSourceGeneratorContext).Assembly);
        Assert.Equal(62, converter.Length);

        var source = ((byte)1, (sbyte)-2, (ushort)3, (short)-4, 5U, -6, 7UL, -8L, (UInt128)9, (Int128)(-10));
        var buffer = converter.Encode(source);
        var result = converter.Decode(buffer);
        Assert.Equal(source, result);

        var allocator = new Allocator();
        converter.EncodeAuto(ref allocator, source);
        var intent = new ReadOnlySpan<byte>(allocator.ToArray());
        var decode = converter.DecodeAuto(ref intent);
        Assert.Equal(source, decode);
        Assert.Equal(0, intent.Length);
    }
}
