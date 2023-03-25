namespace Mikodev.Binary.SourceGeneration.AttributeTests.ObjectConverterAttributeTests;

using Mikodev.Binary.Attributes;
using System;
using Xunit;

[SourceGeneratorContext]
[SourceGeneratorInclude<TestClass>]
public partial class TestClassSourceGenerationContext { }

[Converter(typeof(TestClassConverter))]
public class TestClass { }

public class TestClassConverter : Converter<TestClass>
{
    public override void Encode(ref Allocator allocator, TestClass? item) => throw new NotSupportedException();

    public override TestClass Decode(in ReadOnlySpan<byte> span) => throw new NotSupportedException();
}

public class WrappedConverterTests
{
    [Fact(DisplayName = "Get Converter Test")]
    public void GetConverterTest()
    {
        var pair = Assert.Single(TestClassSourceGenerationContext.ConverterCreators);
        Assert.Equal(typeof(TestClass), pair.Key);
        var creator = pair.Value;
        _ = Assert.IsType<TestClassConverter>(creator.GetConverter(null!, typeof(TestClass)));
        // return null if type not match
        Assert.Null(creator.GetConverter(null!, typeof(int)));
        Assert.Null(creator.GetConverter(null!, typeof(string)));
    }
}
