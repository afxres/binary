namespace Mikodev.Binary.SourceGeneration.NamedObjectTests.AllKindMembers;

using Mikodev.Binary.Attributes;
using System.Collections.Generic;
using System.Reflection;
using Xunit;

[SourceGeneratorContext]
[SourceGeneratorInclude<TestType>]
public partial class AllKindMembersSourceGeneratorContext { }

[NamedObject]
public class TestType
{
    [NamedKey("1")]
    public static int PublicStaticField;

    [NamedKey("2")]
    internal static int InternalStaticField;

    [NamedKey("3")]
    public int PublicInstanceField;

    [NamedKey("4")]
    internal string? InternalInstanceField;

    [NamedKey("5")]
    public static string? PublicStaticProperty { get; set; }

    [NamedKey("6")]
    internal static string? InternalStaticProperty { get; set; }

    [NamedKey("7")]
    public int PublicInstanceProperty { get; set; }

    [NamedKey("8")]
    internal string? InternalInstanceProperty { get; set; }

    [NamedKey("9")]
    public int this[int id] => id;

    [NamedKey("10")]
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
        Assert.Equal(converterType.Assembly, typeof(IConverter).Assembly);
        var encodeField = converterType.GetField("encode", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(encodeField);
        var encodeAction = Assert.IsType<AllocatorAction<TestType>>(encodeField.GetValue(converter));
        var encodeTarget = encodeAction.Target;
        Assert.NotNull(encodeTarget);
        Assert.Equal(encodeTarget.GetType().Assembly, typeof(AllKindMembersSourceGeneratorContext).Assembly);

        TestType.InternalStaticField = 5;
        TestType.InternalStaticProperty = "6";
        TestType.PublicStaticField = 7;
        TestType.PublicStaticProperty = "8";

        var source = new TestType { InternalInstanceField = "1", InternalInstanceProperty = "2", PublicInstanceField = 3, PublicInstanceProperty = 4 };
        var buffer = converter.Encode(source);
        var token = new Token(generator, buffer);
        var expectedKeys = new[] { "3", "7" };
        Assert.Equal(new HashSet<string>(expectedKeys), new HashSet<string>(token.Children.Keys));

        var result = converter.Decode(buffer);
        Assert.Null(result.InternalInstanceField);
        Assert.Null(result.InternalInstanceProperty);
        Assert.Equal(3, result.PublicInstanceField);
        Assert.Equal(4, result.PublicInstanceProperty);

        Assert.Equal(5, TestType.InternalStaticField);
        Assert.Equal("6", TestType.InternalStaticProperty);
        Assert.Equal(7, TestType.PublicStaticField);
        Assert.Equal("8", TestType.PublicStaticProperty);
    }
}
