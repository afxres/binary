namespace Mikodev.Binary.SourceGeneration.UnionObjectTests.IntegrationTests;

using Mikodev.Binary.Attributes;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Xunit;

// empty type: no instance state at all (no fields or properties, public or non-public)
public readonly record struct BareEmpty;

public sealed class BareEmptyClass { }

public sealed record BareEmptyRecordClass;

public readonly union UnionWithEmptyStruct(BareEmpty, int);

public readonly union UnionWithEmptyClass(BareEmptyClass, int);

public readonly union UnionWithEmptyRecordClass(BareEmptyRecordClass, int);

// the empty case sits last, so the converter argument list ends on a skipped slot
public readonly union UnionWithTrailingEmptyCase(int, BareEmpty);

// control group: an empty type with a hand written zero byte converter
// such a converter is only meaningful inside a union, where the tag delimits the payload;
// it is NOT a valid stand alone converter, see 'ZeroByteConverterIsInvalidOutsideUnionTest'
[Converter(typeof(BareEmptyConverter))]
public readonly record struct CoveredEmpty;

public sealed class BareEmptyConverter : Converter<CoveredEmpty>
{
    public override void Encode(ref Allocator allocator, CoveredEmpty item) { }

    public override CoveredEmpty Decode(in ReadOnlySpan<byte> span) => default;

    public override void EncodeAuto(ref Allocator allocator, CoveredEmpty item) { }

    public override CoveredEmpty DecodeAuto(ref ReadOnlySpan<byte> span) => default;
}

public readonly union UnionWithCoveredEmpty(CoveredEmpty, int);

[SourceGeneratorContext]
[SourceGeneratorInclude<UnionWithEmptyStruct>]
[SourceGeneratorInclude<UnionWithEmptyClass>]
[SourceGeneratorInclude<UnionWithEmptyRecordClass>]
[SourceGeneratorInclude<UnionWithTrailingEmptyCase>]
[SourceGeneratorInclude<UnionWithCoveredEmpty>]
public partial class UnionEmptyCaseGeneratorContext { }

public class UnionEmptyCaseTests
{
    private static (IGenerator Jit, IGenerator Aot) CreateGenerators()
    {
        var jit = Generator.CreateDefault();
        var aot = Generator.CreateAotBuilder()
            .AddConverterCreators(UnionEmptyCaseGeneratorContext.ConverterCreators.Values)
            .Build();
        return (jit, aot);
    }

    // expected wire of an empty case: the tag byte only, auto must not append a length prefix
    private static void AssertEmptyCaseWire<TUnion>(TUnion source, string expectedHex)
    {
        var (jit, aot) = CreateGenerators();
        foreach (var generator in new[] { jit, aot })
        {
            var converter = generator.GetConverter<TUnion>();
            Assert.Equal(0, converter.Length);

            var buffer = converter.Encode(source);
            Assert.Equal(expectedHex, Convert.ToHexString(buffer));

            var autoBuffer = Allocator.Invoke(source, converter.EncodeAuto);
            Assert.Equal(expectedHex, Convert.ToHexString(autoBuffer));

            var body = new ReadOnlySpan<byte>(buffer);
            _ = converter.DecodeAuto(ref body);
            Assert.Equal(0, body.Length);
        }
    }

    [Fact(DisplayName = "Union With Empty Struct Case Test")]
    public void UnionWithEmptyStructCaseTest()
    {
        var source = new UnionWithEmptyStruct(new BareEmpty());
        AssertEmptyCaseWire(source, "00");
        var converter = Generator.CreateDefault().GetConverter<UnionWithEmptyStruct>();
        Assert.Equal(source, converter.Decode(converter.Encode(source)));
    }

    [Fact(DisplayName = "Union With Empty Class Case Test")]
    public void UnionWithEmptyClassCaseTest()
    {
        var source = new UnionWithEmptyClass(new BareEmptyClass());
        AssertEmptyCaseWire(source, "00");
        var converter = Generator.CreateDefault().GetConverter<UnionWithEmptyClass>();
        var result = converter.Decode(converter.Encode(source));
        _ = Assert.IsType<BareEmptyClass>(((IUnion)result).Value);
    }

    [Fact(DisplayName = "Union With Empty Record Class Case Test")]
    public void UnionWithEmptyRecordClassCaseTest()
    {
        // a record class synthesizes 'EqualityContract', which must not be mistaken for state,
        // otherwise the reflection and source generation paths disagree on the wire format
        var source = new UnionWithEmptyRecordClass(new BareEmptyRecordClass());
        AssertEmptyCaseWire(source, "00");
        var converter = Generator.CreateDefault().GetConverter<UnionWithEmptyRecordClass>();
        Assert.Equal(source, converter.Decode(converter.Encode(source)));
    }

    [Fact(DisplayName = "Union With Empty Struct Case And Non-Empty Case Test")]
    public void UnionWithEmptyStructCaseAndNonEmptyCaseTest()
    {
        var (jit, aot) = CreateGenerators();
        var source = new UnionWithEmptyStruct(0x2A);
        foreach (var generator in new[] { jit, aot })
        {
            var converter = generator.GetConverter<UnionWithEmptyStruct>();
            var buffer = converter.Encode(source);
            Assert.Equal("012A000000", Convert.ToHexString(buffer));
            var autoBuffer = Allocator.Invoke(source, converter.EncodeAuto);
            Assert.Equal("012A000000", Convert.ToHexString(autoBuffer));
            Assert.Equal(source, converter.Decode(buffer));
        }
    }

    [Fact(DisplayName = "Union With Trailing Empty Case Test")]
    public void UnionWithTrailingEmptyCaseTest()
    {
        // the empty case is the last one, the generated converter type must not end
        // its parameter list with a separator because that case is skipped
        AssertEmptyCaseWire(new UnionWithTrailingEmptyCase(new BareEmpty()), "01");
        var (jit, aot) = CreateGenerators();
        var source = new UnionWithTrailingEmptyCase(0x2A);
        foreach (var generator in new[] { jit, aot })
        {
            var converter = generator.GetConverter<UnionWithTrailingEmptyCase>();
            Assert.Equal("002A000000", Convert.ToHexString(converter.Encode(source)));
            Assert.Equal(source, converter.Decode(converter.Encode(source)));
        }
    }

    [Fact(DisplayName = "Union With Covered Empty Case Test")]
    public void UnionWithCoveredEmptyCaseTest()
    {
        var source = new UnionWithCoveredEmpty(default(CoveredEmpty));
        AssertEmptyCaseWire(source, "00");
        var converter = Generator.CreateDefault().GetConverter<UnionWithCoveredEmpty>();
        Assert.Equal(source, converter.Decode(converter.Encode(source)));
    }

    // a zero byte converter always reports 'Length == 0', which means variable length:
    // 'EncodeAuto' is therefore expected to write a length prefix, but the converter writes nothing,
    // so the container cannot delimit elements any more; the element count is silently lost
    [Fact(DisplayName = "Zero Byte Converter Is Invalid Outside Union Test")]
    public void ZeroByteConverterIsInvalidOutsideUnionTest()
    {
        var converter = Generator.CreateDefault().GetConverter<List<CoveredEmpty>>();
        var items = new List<CoveredEmpty> { default, default, default };
        var buffer = Allocator.Invoke(items, converter.EncodeAuto);
        Assert.Equal("00", Convert.ToHexString(buffer));
        var span = new ReadOnlySpan<byte>(buffer);
        Assert.Empty(converter.DecodeAuto(ref span));
        Assert.Equal(0, span.Length);
        // decoding a non empty payload never advances the cursor either,
        // 'SpanLikeMethods::GetList' loops until the span is drained and ends with OutOfMemoryException
    }
}
