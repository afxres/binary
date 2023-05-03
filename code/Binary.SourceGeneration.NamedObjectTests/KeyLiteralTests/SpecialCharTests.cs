namespace Mikodev.Binary.SourceGeneration.NamedObjectTests.KeyLiteralTests;

using Mikodev.Binary.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

[SourceGeneratorContext]
[SourceGeneratorInclude<ClassWithSpecialCharsAsNameKeys>]
public partial class SpecialCharGeneratorContext { }

[NamedObject]
public class ClassWithSpecialCharsAsNameKeys : IEquatable<ClassWithSpecialCharsAsNameKeys?>
{
    [NamedKey("\x0\x10\x13\x4f60\x597d")]
    public int A { get; set; }

    [NamedKey("\x7f")]
    public string? Z { get; set; }

    public bool Equals(ClassWithSpecialCharsAsNameKeys? other) => other is not null && A == other.A && Z == other.Z;

    public override bool Equals(object? obj) => Equals(obj as ClassWithSpecialCharsAsNameKeys);

    public override int GetHashCode() => HashCode.Combine(A, Z);
}

public class SpecialCharTests
{
    [Fact(DisplayName = "Class With Special Chars As Named Keys")]
    public void ClassWithSpecialCharsAsNamedKeysTest()
    {
        var generator = Generator.CreateAotBuilder().AddConverterCreators(SpecialCharGeneratorContext.ConverterCreators.Values).Build();
        var converter = generator.GetConverter<ClassWithSpecialCharsAsNameKeys>();
        var source = new ClassWithSpecialCharsAsNameKeys { A = 1, Z = "Zero" };
        var buffer = converter.Encode(source);

        var generatorSecond = Generator.CreateDefault();
        var converterSecond = generatorSecond.GetConverter<ClassWithSpecialCharsAsNameKeys>();
        var bufferSecond = converterSecond.Encode(source);
        Assert.Equal(buffer, bufferSecond);

        var result = converter.Decode(buffer);
        var resultSecond = converterSecond.Decode(bufferSecond);
        Assert.Equal(source, result);
        Assert.Equal(source, resultSecond);

        var token = new Token(generator, buffer);
        var tokenSecond = new Token(generatorSecond, bufferSecond);
        var expectedKeys = new HashSet<string> { "\0\u0010\u0013你好", "\u007f" };
        Assert.Equal(expectedKeys, token.Children.Keys.ToHashSet());
        Assert.Equal(expectedKeys, tokenSecond.Children.Keys.ToHashSet());
    }
}
