namespace Mikodev.Binary.SourceGeneration.ObjectCrossTests.ReadOnlyMembers;

using Mikodev.Binary.Attributes;
using System;
using System.Collections.Generic;
using Xunit;

[SourceGeneratorContext]
[SourceGeneratorInclude<ReadOnlyPropertyNamedObject>]
[SourceGeneratorInclude<ReadOnlyPropertyTupleObject>]
public partial class ReadOnlyMembersGeneratorContext { }

[NamedObject]
public class ReadOnlyPropertyNamedObject
{
    private int id;

    [NamedKey("id")]
    public int Id => this.id;

    public void SetId(int id) => this.id = id;
}

[TupleObject]
public class ReadOnlyPropertyTupleObject
{
    private string? name;

    [TupleKey(0)]
    public string? Name => this.name;

    public void SetName(string name) => this.name = name;
}

public class ReadOnlyMembersTests
{
    public static IEnumerable<object[]> ReadOnlyPropertyData()
    {
        var a = new ReadOnlyPropertyNamedObject();
        a.SetId(86);
        var b = new ReadOnlyPropertyTupleObject();
        b.SetName("name");
        yield return new object[] { a };
        yield return new object[] { b };
    }

    [Theory(DisplayName = "Read Only Members Test")]
    [MemberData(nameof(ReadOnlyPropertyData))]
    public void ReadOnlyMembersTest<T>(T source)
    {
        var builder = Generator.CreateAotBuilder();
        foreach (var i in ReadOnlyMembersGeneratorContext.ConverterCreators)
            _ = builder.AddConverterCreator(i.Value);
        var generator = builder.Build();
        var converter = generator.GetConverter<T>();
        var buffer = converter.Encode(source);
        Assert.NotEmpty(buffer);

        var error = Assert.Throws<NotSupportedException>(() => converter.Decode(buffer));
        var message = $"No suitable constructor found, type: {typeof(T)}";
        Assert.Equal(message, error.Message);
    }
}
