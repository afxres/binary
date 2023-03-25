namespace Mikodev.Binary.SourceGeneration.TupleObjectTests.AllKindMembers;

using Mikodev.Binary.Attributes;
using System;
using Xunit;

[SourceGeneratorContext]
[SourceGeneratorInclude<TestType>]
public partial class AllKindMembersSourceGeneratorContext { }

[TupleObject]
public class TestType
{
    [TupleKey(1)]
    public static int PublicStaticField;

    [TupleKey(2)]
    internal static int InternalStaticField;

    [TupleKey(3)]
    public int PublicInstanceField;

    [TupleKey(4)]
    internal string? InternalInstanceField;

    [TupleKey(5)]
    public static string? PublicStaticProperty { get; set; }

    [TupleKey(6)]
    internal static string? InternalStaticProperty { get; set; }

    [TupleKey(7)]
    public int PublicInstanceProperty { get; set; }

    [TupleKey(8)]
    internal string? InternalInstanceProperty { get; set; }

    [TupleKey(9)]
    public int this[int id] => id;

    [TupleKey(10)]
    internal string this[string id] => id;
}

public class AllKindMembersTests
{
    [Fact(DisplayName = "All Kind Members Test")]
    public void AllKindMembersTest()
    {
        var pair = Assert.Single(AllKindMembersSourceGeneratorContext.ConverterCreators);
        Assert.Equal(typeof(TestType), pair.Key);
        var builder = Generator.CreateDefaultBuilder();
        _ = builder.AddConverterCreator(pair.Value);
        var generator = builder.Build();
        var converter = generator.GetConverter<TestType>();
        var converterType = converter.GetType();
        Assert.Equal(converterType.Assembly, typeof(AllKindMembersSourceGeneratorContext).Assembly);

        TestType.InternalStaticField = 1;
        TestType.InternalStaticProperty = "2";
        TestType.PublicStaticField = 3;
        TestType.PublicStaticProperty = "4";

        var source = new TestType { InternalInstanceField = "5", InternalInstanceProperty = "6", PublicInstanceField = 7, PublicInstanceProperty = 8 };
        var buffer = converter.Encode(source);
        var int32Converter = generator.GetConverter<int>();
        var intent = new ReadOnlySpan<byte>(buffer);
        var a = int32Converter.DecodeAuto(ref intent);
        var b = int32Converter.DecodeAuto(ref intent);
        Assert.Equal(0, intent.Length);
        Assert.Equal(7, a);
        Assert.Equal(8, b);

        var result = converter.Decode(buffer);
        Assert.Null(result.InternalInstanceField);
        Assert.Null(result.InternalInstanceProperty);
        Assert.Equal(7, result.PublicInstanceField);
        Assert.Equal(8, result.PublicInstanceProperty);

        Assert.Equal(1, TestType.InternalStaticField);
        Assert.Equal("2", TestType.InternalStaticProperty);
        Assert.Equal(3, TestType.PublicStaticField);
        Assert.Equal("4", TestType.PublicStaticProperty);
    }
}
