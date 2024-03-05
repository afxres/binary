namespace Mikodev.Binary.SourceGeneration.NamedObjectTests.PlainObjectTests;

using Mikodev.Binary.Attributes;
using System.Collections.Generic;
using System.Linq;
using Xunit;

[SourceGeneratorContext]
[SourceGeneratorInclude<SimpleObject>]
public partial class MixedMembersSourceGeneratorContext { }

#pragma warning disable IDE0051 // Remove unused private members
#pragma warning disable IDE0044 // Add readonly modifier
#pragma warning disable CA2211 // Non-constant fields should not be visible
#pragma warning disable CS0169 // The field '...' is never used
public class SimpleObject
{
    private const int PrivateConstant = 1;

    internal const long InternalConstant = 2;

    public const string PublicConstant = "3";

    private int PrivateInstanceField;

    internal long InternalInstanceField;

    public string? PublicInstanceField;

    private static int PrivateStaticField;

    internal static long InternalStaticField;

    public static string? PublicStaticField;

    private int PrivateInstanceProperty { get; set; }

    internal long InternalInstanceProperty { get; set; }

    public string? PublicInstanceProperty { get; set; }

    private static int PrivateStaticProperty { get; set; }

    internal static long InternalStaticProperty { get; set; }

    public static string? PublicStaticProperty { get; set; }

    private int this[int key] => key;

    internal long this[long key] => key;

    public string? this[string? key] => key;
}
#pragma warning restore CS0169 // The field '...' is never used
#pragma warning restore CA2211 // Non-constant fields should not be visible
#pragma warning restore IDE0044 // Add readonly modifier
#pragma warning restore IDE0051 // Remove unused private members

public class MixedMembersTests
{
    [Fact(DisplayName = "Plain Object Encode Decode Test")]
    public void PlainObjectEncodeDecodeTest()
    {
        var generator = Generator.CreateAotBuilder()
            .AddConverterCreators(MixedMembersSourceGeneratorContext.ConverterCreators.Values)
            .Build();
        var generatorSecond = Generator.CreateDefault();

        SimpleObject.InternalStaticField = 1L;
        SimpleObject.InternalStaticProperty = 2L;
        SimpleObject.PublicStaticField = "3";
        SimpleObject.PublicStaticProperty = "4";

        var converter = generator.GetConverter<SimpleObject>();
        var converterSecond = generatorSecond.GetConverter<SimpleObject>();
        var source = new SimpleObject
        {
            InternalInstanceField = -3L,
            InternalInstanceProperty = 6L,
            PublicInstanceField = "100",
            PublicInstanceProperty = "-3",
        };
        var buffer = converter.Encode(source);
        var bufferSecond = converterSecond.Encode(source);
        var token = new Token(generator, buffer);
        var tokenSecond = new Token(generatorSecond, bufferSecond);
        var keys = new HashSet<string>(["PublicInstanceField", "PublicInstanceProperty"]);
        Assert.Equal(keys, token.Children.Keys.ToHashSet());
        Assert.Equal(keys, tokenSecond.Children.Keys.ToHashSet());

        var result = converter.Decode(buffer);
        Assert.Equal(source.PublicInstanceField, result.PublicInstanceField);
        Assert.Equal(source.PublicInstanceProperty, result.PublicInstanceProperty);
        Assert.Equal(0L, result.InternalInstanceField);
        Assert.Equal(0L, result.InternalInstanceProperty);

        var resultSecond = converterSecond.Decode(bufferSecond);
        Assert.Equal(source.PublicInstanceField, resultSecond.PublicInstanceField);
        Assert.Equal(source.PublicInstanceProperty, resultSecond.PublicInstanceProperty);
        Assert.Equal(0L, resultSecond.InternalInstanceField);
        Assert.Equal(0L, resultSecond.InternalInstanceProperty);

        Assert.Equal(1L, SimpleObject.InternalStaticField);
        Assert.Equal(2L, SimpleObject.InternalStaticProperty);
        Assert.Equal("3", SimpleObject.PublicStaticField);
        Assert.Equal("4", SimpleObject.PublicStaticProperty);
    }
}
