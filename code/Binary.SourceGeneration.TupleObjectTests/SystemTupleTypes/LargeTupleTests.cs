namespace Mikodev.Binary.SourceGeneration.TupleObjectTests.SystemTupleTypes;

using Mikodev.Binary.Attributes;
using System;
using Xunit;

[SourceGeneratorContext]
[SourceGeneratorInclude<Tuple<byte, sbyte, ushort, short, uint, int, ulong, Tuple<long, UInt128, Int128>>>]
public partial class LargeTupleSourceGeneratorContext { }

public class LargeTupleTests
{
    [Fact(DisplayName = "Large Tuple")]
    public void LargeTuple()
    {
        var creators = LargeTupleSourceGeneratorContext.ConverterCreators;
        Assert.Equal(2, creators.Count);
        Assert.Contains(typeof(Tuple<long, UInt128, Int128>), creators.Keys);
        Assert.Contains(typeof(Tuple<byte, sbyte, ushort, short, uint, int, ulong, Tuple<long, UInt128, Int128>>), creators.Keys);

        var builder = Generator.CreateDefaultBuilder();
        foreach (var i in creators)
            _ = builder.AddConverterCreator(i.Value);
        var generator = builder.Build();
        var converter = generator.GetConverter<Tuple<byte, sbyte, ushort, short, uint, int, ulong, Tuple<long, UInt128, Int128>>>();
        Assert.Equal(converter.GetType().Assembly, typeof(LargeTupleSourceGeneratorContext).Assembly);
        Assert.Equal(62, converter.Length);

        var source = new Tuple<byte, sbyte, ushort, short, uint, int, ulong, Tuple<long, UInt128, Int128>>(1, -2, 3, -4, 5U, -6, 7UL, new Tuple<long, UInt128, Int128>(-8L, (UInt128)9, (Int128)(-10)));
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
