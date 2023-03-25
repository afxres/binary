namespace Mikodev.Binary.SourceGeneration.AttributeTests.ObjectConverterCreatorAttributesTests;

using Mikodev.Binary.Attributes;
using System;
using Xunit;

[SourceGeneratorContext]
[SourceGeneratorInclude<TestType>]
public partial class TestTypeSourceGeneratorContext { }

[ConverterCreator(typeof(TestTypeConverterCreator))]
public class TestType { }

public class TestTypeConverter : Converter<TestType>
{
    public override void Encode(ref Allocator allocator, TestType? item) => throw new NotSupportedException();

    public override TestType Decode(in ReadOnlySpan<byte> span) => throw new NotSupportedException();
}

public class TestTypeConverterCreator : IConverterCreator
{
    IConverter? IConverterCreator.GetConverter(IGeneratorContext context, Type type)
    {
        return new TestTypeConverter();
    }
}

public class WrappedConverterCreatorTests
{
    [Fact(DisplayName = "Wrapped Converter Creator Test")]
    public void WrappedConverterCreatorTest()
    {
        var pair = Assert.Single(TestTypeSourceGeneratorContext.ConverterCreators);
        Assert.Equal(typeof(TestType), pair.Key);
        var creator = pair.Value;
        _ = Assert.IsType<TestTypeConverter>(creator.GetConverter(null!, typeof(TestType)));
        // return null if type not match
        Assert.Null(creator.GetConverter(null!, typeof(int)));
        Assert.Null(creator.GetConverter(null!, typeof(string)));
    }
}
