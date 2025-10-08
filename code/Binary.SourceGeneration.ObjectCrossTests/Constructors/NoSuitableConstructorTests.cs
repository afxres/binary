namespace Mikodev.Binary.SourceGeneration.ObjectCrossTests.Constructors;

using Mikodev.Binary.Attributes;
using System;
using System.Collections.Generic;
using Xunit;

[SourceGeneratorContext]
[SourceGeneratorInclude<InternalSetterOnlyType>]
[SourceGeneratorInclude<BestConstructorNotPublic>]
public partial class NoSuitableConstructorGeneratorContext { }

public class InternalSetterOnlyType
{
    public int A { get; internal set; }

    public InternalSetterOnlyType() { }
}

public class BestConstructorNotPublic
{
    public int A { get; }

    public BestConstructorNotPublic() { }

    internal BestConstructorNotPublic(int a) => A = a;
}

public class NoSuitableConstructorTests
{
    public static IEnumerable<object[]> NoSuitableConstructorData()
    {
        yield return [new InternalSetterOnlyType { A = 1 }];
        yield return [new BestConstructorNotPublic(2)];
    }

    [Theory(DisplayName = "No Suitable Constructor Test")]
    [MemberData(nameof(NoSuitableConstructorData))]
    public void NoSuitableConstructorTest<T>(T source)
    {
        var generator = Generator.CreateAotBuilder().AddConverterCreators(NoSuitableConstructorGeneratorContext.ConverterCreators.Values).Build();
        var generatorSecond = Generator.CreateDefault();
        var converter = generator.GetConverter<T>();
        var converterSecond = generatorSecond.GetConverter<T>();

        var buffer = converter.Encode(source);
        var bufferSecond = converterSecond.Encode(source);
        Assert.NotEmpty(buffer);
        Assert.Equal(buffer, bufferSecond);

        var error = Assert.Throws<NotSupportedException>(() => converter.Decode(buffer));
        var errorSecond = Assert.Throws<NotSupportedException>(() => converterSecond.Decode(bufferSecond));
        var message = $"No suitable constructor found, type: {typeof(T)}";
        Assert.Equal(message, error.Message);
        Assert.Equal(message, errorSecond.Message);
    }
}
