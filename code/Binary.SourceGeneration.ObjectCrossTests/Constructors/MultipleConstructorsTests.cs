namespace Mikodev.Binary.SourceGeneration.ObjectCrossTests.Constructors;

using Mikodev.Binary.Attributes;
using System;
using Xunit;

[SourceGeneratorContext]
[SourceGeneratorInclude<ClassWithMultipleConstructors>]
public partial class MultipleConstructorsGeneratorContext { }

public class ClassWithMultipleConstructors : IEquatable<ClassWithMultipleConstructors?>
{
    public int Key { get; }

    public int Alpha { get; set; }

    public int Bravo { get; set; }

    public int Delta { get; set; }

    public ClassWithMultipleConstructors(int key) => throw new NotSupportedException();

    public ClassWithMultipleConstructors(int alpha, int key) => throw new NotSupportedException();

    public ClassWithMultipleConstructors(int alpha, int bravo, int key) => throw new NotSupportedException();

    public ClassWithMultipleConstructors(int bravo, int key, int alpha, int delta)
    {
        Key = key;
        Alpha = alpha;
        Bravo = bravo;
        Delta = delta;
    }

    public bool Equals(ClassWithMultipleConstructors? other) => other is not null && Key == other.Key && Alpha == other.Alpha && Bravo == other.Bravo && Delta == other.Delta;

    public override bool Equals(object? obj) => Equals(obj as ClassWithMultipleConstructors);

    public override int GetHashCode() => HashCode.Combine(Key, Alpha, Bravo, Delta);
}

public class MultipleConstructorsTests
{
    [Fact(DisplayName = "Class With Multiple Constructors")]
    public void ClassWithMultipleConstructorsTest()
    {
        var generator = Generator.CreateAotBuilder().AddConverterCreators(MultipleConstructorsGeneratorContext.ConverterCreators.Values).Build();
        var converter = generator.GetConverter<ClassWithMultipleConstructors>();
        var source = new ClassWithMultipleConstructors(2, 0, 1, 4);
        var buffer = converter.Encode(source);

        var generatorSecond = Generator.CreateDefault();
        var converterSecond = generatorSecond.GetConverter<ClassWithMultipleConstructors>();
        var bufferSecond = converterSecond.Encode(source);

        var result = converter.Decode(buffer);
        var resultSecond = converterSecond.Decode(bufferSecond);
        Assert.Equal(buffer, bufferSecond);
        Assert.Equal(source, result);
        Assert.Equal(source, resultSecond);
    }
}
