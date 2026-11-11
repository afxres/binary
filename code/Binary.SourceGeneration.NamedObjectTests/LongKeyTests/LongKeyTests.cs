namespace Mikodev.Binary.SourceGeneration.NamedObjectTests.LongKeyTests;

using Mikodev.Binary.Attributes;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;

[SourceGeneratorContext]
[SourceGeneratorInclude<ClassWithLongMemberNames>]
[SourceGeneratorInclude<ClassWithLongNamedKeys>]
public partial class LongKeyGeneratorContext { }

/// <summary>
/// No named object attribute, so the member names are the named keys: 88, 88 and 106 bytes. The
/// first two share both their length and their first byte, the third does not share its length.
/// </summary>
public class ClassWithLongMemberNames
{
    public int ThisMemberNameIsIntentionallyVeryLongSoThatTheKeyTableFallsBackToSequenceEqualComparison;

    public int ThatMemberNameIsIntentionallyVeryLongSoThatTheKeyTableFallsBackToSequenceEqualComparison;

    public int AMemberNameThatIsEvenLongerThanTheTwoAboveBecauseItKeepsGoingWithMoreWordsSoThatTheLengthBucketIsNotShared;
}

/// <summary>
/// Keys far beyond the 15 bytes word limit. Every key either shares its length with another member
/// or sits on the length boundary, which is what decides the shape of the generated lookup table.
/// </summary>
[NamedObject]
public class ClassWithLongNamedKeys
{
    // 87 bytes each, same length but a different first byte: two groups inside the same length bucket
    [NamedKey("another_member_name_that_shares_its_length_with_the_following_member_but_not_the_prefix")]
    public int Alpha;

    [NamedKey("zealous_member_name_that_shares_its_length_with_the_previous_member_but_not_the_prefix_")]
    public int Zulu;

    // 15 bytes uses two overlapping Word64 reads, 16 bytes is the first length that needs SequenceEqual
    [NamedKey("abcdefghijklmno")]
    public int Fifteen;

    [NamedKey("abcdefghijklmnop")]
    public int Sixteen;

    // 60 bytes of utf-8 for 21 characters, both keys start with the same character: the table
    // buckets by byte count, and a shared first byte forces a SequenceEqual chain
    [NamedKey("中文文文文文文文文文文文文文文文文文文文文")]
    public int ChineseOne;

    [NamedKey("中字字字字字字字字字字字字字字字字字字字字")]
    public int ChineseTwo;
}

public class LongKeyTests
{
    private static readonly FieldInfo[] MemberNameFields = typeof(ClassWithLongMemberNames).GetFields();

    private static readonly FieldInfo[] NamedKeyFields = typeof(ClassWithLongNamedKeys).GetFields();

    private static void AssertMemberNameFields(ClassWithLongMemberNames expected, ClassWithLongMemberNames actual)
    {
        foreach (var field in MemberNameFields)
            Assert.Equal(field.GetValue(expected), field.GetValue(actual));
    }

    [Fact(DisplayName = "Long Member Names Encode Decode Test")]
    public void LongMemberNamesTest()
    {
        Assert.Equal(3, MemberNameFields.Length);
        var generator = Generator.CreateAotBuilder()
            .AddConverterCreators(LongKeyGeneratorContext.ConverterCreators.Values)
            .Build();
        var generatorSecond = Generator.CreateDefault();
        var converter = generator.GetConverter<ClassWithLongMemberNames>();
        var converterSecond = generatorSecond.GetConverter<ClassWithLongMemberNames>();

        var source = new ClassWithLongMemberNames
        {
            ThisMemberNameIsIntentionallyVeryLongSoThatTheKeyTableFallsBackToSequenceEqualComparison = 1,
            ThatMemberNameIsIntentionallyVeryLongSoThatTheKeyTableFallsBackToSequenceEqualComparison = 2,
            AMemberNameThatIsEvenLongerThanTheTwoAboveBecauseItKeepsGoingWithMoreWordsSoThatTheLengthBucketIsNotShared = 3,
        };
        var buffer = converter.Encode(source);
        var bufferSecond = converterSecond.Encode(source);
        Assert.Equal(buffer, bufferSecond);

        var expectedKeys = new HashSet<string>
        {
            "ThisMemberNameIsIntentionallyVeryLongSoThatTheKeyTableFallsBackToSequenceEqualComparison",
            "ThatMemberNameIsIntentionallyVeryLongSoThatTheKeyTableFallsBackToSequenceEqualComparison",
            "AMemberNameThatIsEvenLongerThanTheTwoAboveBecauseItKeepsGoingWithMoreWordsSoThatTheLengthBucketIsNotShared",
        };
        var token = new Token(generator, buffer);
        var tokenSecond = new Token(generatorSecond, bufferSecond);
        Assert.Equal(expectedKeys, token.Children.Keys.ToHashSet());
        Assert.Equal(expectedKeys, tokenSecond.Children.Keys.ToHashSet());

        var result = converter.Decode(buffer);
        AssertMemberNameFields(source, result);
        Assert.Equal(buffer, converter.Encode(result));
    }

    [Fact(DisplayName = "Long Named Keys Encode Decode Test")]
    public void LongNamedKeysTest()
    {
        Assert.Equal(6, NamedKeyFields.Length);
        var generator = Generator.CreateAotBuilder()
            .AddConverterCreators(LongKeyGeneratorContext.ConverterCreators.Values)
            .Build();
        var generatorSecond = Generator.CreateDefault();
        var converter = generator.GetConverter<ClassWithLongNamedKeys>();
        var converterSecond = generatorSecond.GetConverter<ClassWithLongNamedKeys>();

        var source = new ClassWithLongNamedKeys
        {
            Alpha = 3,
            Zulu = 4,
            Fifteen = 5,
            Sixteen = 6,
            ChineseOne = 7,
            ChineseTwo = 8,
        };
        var buffer = converter.Encode(source);
        var bufferSecond = converterSecond.Encode(source);
        Assert.Equal(buffer, bufferSecond);

        var expectedKeys = new HashSet<string>
        {
            "another_member_name_that_shares_its_length_with_the_following_member_but_not_the_prefix",
            "zealous_member_name_that_shares_its_length_with_the_previous_member_but_not_the_prefix_",
            "abcdefghijklmno",
            "abcdefghijklmnop",
            "中文文文文文文文文文文文文文文文文文文文文",
            "中字字字字字字字字字字字字字字字字字字字字",
        };
        var token = new Token(generator, buffer);
        var tokenSecond = new Token(generatorSecond, bufferSecond);
        Assert.Equal(expectedKeys, token.Children.Keys.ToHashSet());
        Assert.Equal(expectedKeys, tokenSecond.Children.Keys.ToHashSet());

        var result = converter.Decode(buffer);
        foreach (var field in NamedKeyFields)
            Assert.Equal(field.GetValue(source), field.GetValue(result));
        Assert.Equal(buffer, converter.Encode(result));
    }
}
