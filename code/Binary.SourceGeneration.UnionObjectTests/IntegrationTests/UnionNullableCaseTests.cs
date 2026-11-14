namespace Mikodev.Binary.SourceGeneration.UnionObjectTests.IntegrationTests;

using Mikodev.Binary.Attributes;
using System;
using Xunit;

public readonly union UnionWithNullableValueCase(int?, string);

// 'int' and 'int?' share one boxed representation, so the two cases can never be told
// apart, both the source generator and the reflection based creator reject this type
public readonly union UnionWithAmbiguousCase(int, int?);

[SourceGeneratorContext]
[SourceGeneratorInclude<UnionWithNullableValueCase>]
public partial class UnionNullableCaseGeneratorContext { }

public class UnionNullableCaseTests
{
    private static (IGenerator Jit, IGenerator Aot) CreateGenerators()
    {
        var jit = Generator.CreateDefault();
        var aot = Generator.CreateAotBuilder()
            .AddConverterCreators(UnionNullableCaseGeneratorContext.ConverterCreators.Values)
            .Build();
        return (jit, aot);
    }

    public static TheoryData<UnionWithNullableValueCase, string, string> UnionWithNullableValueCaseTestData()
    {
        var data = new TheoryData<UnionWithNullableValueCase, string, string>();
        // a nullable value type is not a legal pattern type, the generated encoder
        // must match the underlying type while the payload keeps the nullable converter
        data.Add((int?)9, "000109000000", "000109000000");
        data.Add("text", "0174657874", "010474657874");
        return data;
    }

    [Theory(DisplayName = "Union With Nullable Value Case Test")]
    [MemberData(nameof(UnionWithNullableValueCaseTestData))]
    public void UnionWithNullableValueCaseTest(UnionWithNullableValueCase source, string encodeHex, string encodeAutoHex)
    {
        var (jit, aot) = CreateGenerators();
        foreach (var generator in new[] { jit, aot })
        {
            var converter = generator.GetConverter<UnionWithNullableValueCase>();
            var buffer = converter.Encode(source);
            Assert.Equal(encodeHex, Convert.ToHexString(buffer));
            Assert.Equal(source, converter.Decode(buffer));

            var autoBuffer = Allocator.Invoke(source, converter.EncodeAuto);
            Assert.Equal(encodeAutoHex, Convert.ToHexString(autoBuffer));
            var body = new ReadOnlySpan<byte>(autoBuffer);
            Assert.Equal(source, converter.DecodeAuto(ref body));
            Assert.Equal(0, body.Length);
        }
    }

    // 'Value' is null for both an uninitialized union and a nullable case holding null,
    // so encoding null stays an error here, exactly like a nullable reference case
    [Fact(DisplayName = "Union With Nullable Value Case And Null Value Test")]
    public void UnionWithNullableValueCaseAndNullValueTest()
    {
        var (jit, aot) = CreateGenerators();
        UnionWithNullableValueCase source = (int?)null;
        var message = $"Invalid or null union value, type: {typeof(UnionWithNullableValueCase)}";
        foreach (var generator in new[] { jit, aot })
        {
            var converter = generator.GetConverter<UnionWithNullableValueCase>();
            var error = Assert.Throws<ArgumentException>(() => converter.Encode(source));
            Assert.Equal(message, error.Message);

            var autoError = Assert.Throws<ArgumentException>(() => Allocator.Invoke(source, converter.EncodeAuto));
            Assert.Equal(message, autoError.Message);
        }
    }

    [Fact(DisplayName = "Union With Ambiguous Case Type Test")]
    public void UnionWithAmbiguousCaseTypeTest()
    {
        var type = typeof(UnionWithAmbiguousCase);
        var generator = Generator.CreateDefault();
        var error = Assert.Throws<ArgumentException>(() => generator.GetConverter(type));
        Assert.Null(error.ParamName);
        Assert.Equal($"Union case type is ambiguous, type: {type}", error.Message);
    }
}
