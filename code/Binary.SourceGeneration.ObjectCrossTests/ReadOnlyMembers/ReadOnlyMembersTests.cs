namespace Mikodev.Binary.SourceGeneration.ObjectCrossTests.ReadOnlyMembers;

using Mikodev.Binary.Attributes;
using System;
using System.Collections.Generic;
using Xunit;

[SourceGeneratorContext]
[SourceGeneratorInclude<ReadOnlyPropertyNamedObject>]
[SourceGeneratorInclude<ReadOnlyPropertyTupleObject>]
[SourceGeneratorInclude<ReadOnlyFieldNamedObject>]
[SourceGeneratorInclude<ReadOnlyFieldTupleObject>]
[SourceGeneratorInclude<PropertyWithPrivateSetterNamedObject>]
[SourceGeneratorInclude<PropertyWithPrivateSetterTupleObject>]
[SourceGeneratorInclude<PropertyWithInternalSetterNamedObject>]
[SourceGeneratorInclude<PropertyWithInternalSetterTupleObject>]
public partial class ReadOnlyMembersGeneratorContext { }

[NamedObject]
public class ReadOnlyPropertyNamedObject
{
    private int id;

    [NamedKey("id")]
    public int Id => this.id;

    public static ReadOnlyPropertyNamedObject Create(int id) => new ReadOnlyPropertyNamedObject { id = id };
}

[TupleObject]
public class ReadOnlyPropertyTupleObject
{
    private string? name;

    [TupleKey(0)]
    public string? Name => this.name;

    public static ReadOnlyPropertyTupleObject Create(string? name) => new ReadOnlyPropertyTupleObject { name = name };
}

[NamedObject]
public class ReadOnlyFieldNamedObject
{
    [NamedKey("a")]
    public readonly string? Name;

    private ReadOnlyFieldNamedObject(string? name) => this.Name = name;

    public static ReadOnlyFieldNamedObject Create(string? name) => new ReadOnlyFieldNamedObject(name);
}

[TupleObject]
public class ReadOnlyFieldTupleObject
{
    [TupleKey(0)]
    public readonly int Id;

    private ReadOnlyFieldTupleObject(int id) => this.Id = id;

    public static ReadOnlyFieldTupleObject Create(int id) => new ReadOnlyFieldTupleObject(id);
}

[NamedObject]
public class PropertyWithPrivateSetterNamedObject
{
    [NamedKey("private")]
    public string? Name { get; private set; }

    public static PropertyWithPrivateSetterNamedObject Create(string? name) => new PropertyWithPrivateSetterNamedObject { Name = name };
}

[TupleObject]
public class PropertyWithPrivateSetterTupleObject
{
    [TupleKey(0)]
    public int Id { get; private set; }

    public static PropertyWithPrivateSetterTupleObject Create(int id) => new PropertyWithPrivateSetterTupleObject { Id = id };
}

[NamedObject]
public class PropertyWithInternalSetterNamedObject
{
    [NamedKey("internal")]
    public int Age { get; internal set; }

    public static PropertyWithInternalSetterNamedObject Create(int age) => new PropertyWithInternalSetterNamedObject { Age = age };
}

[TupleObject]
public class PropertyWithInternalSetterTupleObject
{
    [TupleKey(0)]
    public string? Tags { get; internal set; }

    public static PropertyWithInternalSetterTupleObject Create(string? tags) => new PropertyWithInternalSetterTupleObject { Tags = tags };
}

public class ReadOnlyMembersTests
{
    public static IEnumerable<object[]> ReadOnlyPropertyData()
    {
        yield return new object[] { ReadOnlyPropertyNamedObject.Create(86) };
        yield return new object[] { ReadOnlyPropertyTupleObject.Create("name") };
    }

    public static IEnumerable<object[]> ReadOnlyFieldData()
    {
        yield return new object[] { ReadOnlyFieldNamedObject.Create("someone") };
        yield return new object[] { ReadOnlyFieldTupleObject.Create(42) };
    }

    public static IEnumerable<object[]> PropertyWithNonPublicSetterData()
    {
        yield return new object[] { PropertyWithPrivateSetterNamedObject.Create("private-name") };
        yield return new object[] { PropertyWithPrivateSetterTupleObject.Create(34) };
        yield return new object[] { PropertyWithInternalSetterNamedObject.Create(-273) };
        yield return new object[] { PropertyWithInternalSetterTupleObject.Create("internal-tags") };
    }

    [Theory(DisplayName = "Read Only Members Test")]
    [MemberData(nameof(ReadOnlyPropertyData))]
    [MemberData(nameof(ReadOnlyFieldData))]
    [MemberData(nameof(PropertyWithNonPublicSetterData))]
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
