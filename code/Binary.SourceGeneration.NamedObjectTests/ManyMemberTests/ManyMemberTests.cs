namespace Mikodev.Binary.SourceGeneration.NamedObjectTests.ManyMemberTests;

using Mikodev.Binary.Attributes;
using System.Reflection;
using Xunit;

[SourceGeneratorContext]
[SourceGeneratorInclude<ClassWithManyMembers>]
[SourceGeneratorInclude<ClassWithManyOptionalMembers>]
public partial class ManyMemberGeneratorContext { }

/// <summary>
/// 64 members. The named keys spread over two length buckets and more than twenty first byte
/// groups, so the generated lookup table is emitted and walked at full size.
/// </summary>
[NamedObject]
public class ClassWithManyMembers
{
    [NamedKey("a")]
    public int Value00;

    [NamedKey("b")]
    public int Value01;

    [NamedKey("c")]
    public int Value02;

    [NamedKey("d")]
    public int Value03;

    [NamedKey("e")]
    public int Value04;

    [NamedKey("f")]
    public int Value05;

    [NamedKey("g")]
    public int Value06;

    [NamedKey("h")]
    public int Value07;

    [NamedKey("i")]
    public int Value08;

    [NamedKey("j")]
    public int Value09;

    [NamedKey("k")]
    public int Value10;

    [NamedKey("l")]
    public int Value11;

    [NamedKey("m")]
    public int Value12;

    [NamedKey("n")]
    public int Value13;

    [NamedKey("o")]
    public int Value14;

    [NamedKey("p")]
    public int Value15;

    [NamedKey("q")]
    public int Value16;

    [NamedKey("r")]
    public int Value17;

    [NamedKey("s")]
    public int Value18;

    [NamedKey("t")]
    public int Value19;

    [NamedKey("u")]
    public int Value20;

    [NamedKey("v")]
    public int Value21;

    [NamedKey("w")]
    public int Value22;

    [NamedKey("x")]
    public int Value23;

    [NamedKey("y")]
    public int Value24;

    [NamedKey("z")]
    public int Value25;

    [NamedKey("a26")]
    public int Value26;

    [NamedKey("b27")]
    public int Value27;

    [NamedKey("c28")]
    public int Value28;

    [NamedKey("d29")]
    public int Value29;

    [NamedKey("e30")]
    public int Value30;

    [NamedKey("f31")]
    public int Value31;

    [NamedKey("g32")]
    public int Value32;

    [NamedKey("h33")]
    public int Value33;

    [NamedKey("i34")]
    public int Value34;

    [NamedKey("j35")]
    public int Value35;

    [NamedKey("k36")]
    public int Value36;

    [NamedKey("l37")]
    public int Value37;

    [NamedKey("m38")]
    public int Value38;

    [NamedKey("n39")]
    public int Value39;

    [NamedKey("o40")]
    public int Value40;

    [NamedKey("p41")]
    public int Value41;

    [NamedKey("q42")]
    public int Value42;

    [NamedKey("r43")]
    public int Value43;

    [NamedKey("s44")]
    public int Value44;

    [NamedKey("t45")]
    public int Value45;

    [NamedKey("u46")]
    public int Value46;

    [NamedKey("v47")]
    public int Value47;

    [NamedKey("w48")]
    public int Value48;

    [NamedKey("x49")]
    public int Value49;

    [NamedKey("y50")]
    public int Value50;

    [NamedKey("z51")]
    public int Value51;

    [NamedKey("a52")]
    public int Value52;

    [NamedKey("b53")]
    public int Value53;

    [NamedKey("c54")]
    public int Value54;

    [NamedKey("d55")]
    public int Value55;

    [NamedKey("e56")]
    public int Value56;

    [NamedKey("f57")]
    public int Value57;

    [NamedKey("g58")]
    public int Value58;

    [NamedKey("h59")]
    public int Value59;

    [NamedKey("i60")]
    public int Value60;

    [NamedKey("j61")]
    public int Value61;

    [NamedKey("k62")]
    public int Value62;

    [NamedKey("l63")]
    public int Value63;
}

/// <summary>
/// Four required members and eight optional ones: the missing member check keeps a required slot
/// and the optional table both non trivial.
/// </summary>
[NamedObject]
public class ClassWithManyOptionalMembers
{
    [NamedKey("id")]
    public required int Id;

    [NamedKey("seq")]
    public required int Seq;

    [NamedKey("tag")]
    public required int Tag;

    [NamedKey("ver")]
    public required int Ver;

    [NamedKey("note")]
    public int Note;

    [NamedKey("extra")]
    public int Extra;

    [NamedKey("flag")]
    public int Flag;

    [NamedKey("rank")]
    public int Rank;

    [NamedKey("memo")]
    public int Memo;

    [NamedKey("size")]
    public int Size;

    [NamedKey("ratio")]
    public int Ratio;

    [NamedKey("label")]
    public int Label;
}

public class ManyMemberTests
{
    private static readonly FieldInfo[] Fields = typeof(ClassWithManyMembers).GetFields();

    [Fact(DisplayName = "Many Members Encode Decode Test")]
    public void ManyMembersTest()
    {
        Assert.Equal(64, Fields.Length);
        var generator = Generator.CreateAotBuilder()
            .AddConverterCreators(ManyMemberGeneratorContext.ConverterCreators.Values)
            .Build();
        var generatorSecond = Generator.CreateDefault();
        var converter = generator.GetConverter<ClassWithManyMembers>();
        var converterSecond = generatorSecond.GetConverter<ClassWithManyMembers>();

        var source = new ClassWithManyMembers();
        for (var i = 0; i < Fields.Length; i++)
            Fields[i].SetValue(source, i + 1);

        var buffer = converter.Encode(source);
        var bufferSecond = converterSecond.Encode(source);
        Assert.Equal(buffer, bufferSecond);

        var token = new Token(generator, buffer);
        Assert.Equal(Fields.Length, token.Children.Count);

        var result = converter.Decode(buffer);
        foreach (var field in Fields)
            Assert.Equal(field.GetValue(source), field.GetValue(result));
        Assert.Equal(buffer, converter.Encode(result));
    }

    [Fact(DisplayName = "Many Optional Members Encode Decode Test")]
    public void ManyOptionalMembersTest()
    {
        var generator = Generator.CreateAotBuilder()
            .AddConverterCreators(ManyMemberGeneratorContext.ConverterCreators.Values)
            .Build();
        var generatorSecond = Generator.CreateDefault();
        var converter = generator.GetConverter<ClassWithManyOptionalMembers>();
        var converterSecond = generatorSecond.GetConverter<ClassWithManyOptionalMembers>();

        // every optional member keeps its default value, so only the required ones reach the wire
        var source = new ClassWithManyOptionalMembers { Id = 1, Seq = 2, Tag = 3, Ver = 4 };
        var buffer = converter.Encode(source);
        Assert.Equal(buffer, converterSecond.Encode(source));
        Assert.Equal(4, new Token(generator, buffer).Children.Count);

        var result = converter.Decode(buffer);
        foreach (var field in typeof(ClassWithManyOptionalMembers).GetFields())
            Assert.Equal(field.GetValue(source), field.GetValue(result));

        // now with every member set
        var full = new ClassWithManyOptionalMembers
        {
            Id = 5,
            Seq = 6,
            Tag = 7,
            Ver = 8,
            Note = 9,
            Extra = 10,
            Flag = 11,
            Rank = 12,
            Memo = 13,
            Size = 14,
            Ratio = 15,
            Label = 16,
        };
        var fullBuffer = converter.Encode(full);
        Assert.Equal(fullBuffer, converterSecond.Encode(full));
        Assert.Equal(12, new Token(generator, fullBuffer).Children.Count);

        var fullResult = converter.Decode(fullBuffer);
        foreach (var field in typeof(ClassWithManyOptionalMembers).GetFields())
            Assert.Equal(field.GetValue(full), field.GetValue(fullResult));
        Assert.Equal(fullBuffer, converter.Encode(fullResult));
    }
}
