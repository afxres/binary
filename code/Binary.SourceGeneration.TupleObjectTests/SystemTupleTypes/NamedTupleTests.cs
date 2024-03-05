namespace Mikodev.Binary.SourceGeneration.TupleObjectTests.SystemTupleTypes;

using Mikodev.Binary.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

[SourceGeneratorContext]
[SourceGeneratorInclude<(int, string)>]
[SourceGeneratorInclude<TypeWithNamedTuple>]
public partial class NamedTupleSourceGeneratorContext { }

public class TypeWithNamedTuple
{
    public (int Id, string Name) Person { get; set; }
}

public class NamedTupleTests
{
    [Fact(DisplayName = "Named Tuple Test")]
    public void NamedTupleTest()
    {
        var dictionary = NamedTupleSourceGeneratorContext.ConverterCreators;
        var expectedTypes = new HashSet<Type>([typeof((int, string)), typeof(TypeWithNamedTuple)]);
        Assert.Equal(expectedTypes, dictionary.Keys.ToHashSet());

        var generator = Generator.CreateAotBuilder().AddConverterCreators(dictionary.Values).Build();
        var converter = generator.GetConverter<TypeWithNamedTuple>();
        var source = new TypeWithNamedTuple { Person = (1, "Tom") };
        var buffer = converter.Encode(source);
        var result = converter.Decode(buffer);
        Assert.Equal(source.Person, result.Person);
    }
}
