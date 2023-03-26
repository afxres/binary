namespace Mikodev.Binary.SourceGeneration.TupleObjectTests.CustomTupleTypes;

using Mikodev.Binary;
using Mikodev.Binary.Attributes;
using System;
using System.Collections.Generic;
using Xunit;

[SourceGeneratorContext]
[SourceGeneratorInclude<Person>]
[SourceGeneratorInclude<Student>]
[SourceGeneratorInclude<ValuePerson>]
public partial class MemberOrderSourceGeneratorContext { }

[TupleObject]
public class Person : IEquatable<Person?>
{
    [TupleKey(0)]
    public required int Id { get; init; }

    [TupleKey(1)]
    public required string Name { get; init; }

    public bool Equals(Person? other) => other is not null && Id == other.Id && Name == other.Name;

    public override bool Equals(object? obj) => Equals(obj as Person);

    public override int GetHashCode() => HashCode.Combine(Id, Name);
}

[TupleObject]
public struct ValuePerson : IEquatable<ValuePerson>
{
    [TupleKey(0)]
    public int Id;

    [TupleKey(1)]
    public string? Name;

    public readonly bool Equals(ValuePerson other) => this.Id == other.Id && this.Name == other.Name;

    public override readonly bool Equals(object? obj) => obj is ValuePerson person && Equals(person);

    public override readonly int GetHashCode() => HashCode.Combine(this.Id, this.Name);
}

[TupleObject]
public class Student : IEquatable<Student?>
{
    [TupleKey(2)]
    public required string Id { get; init; }

    [TupleKey(0)]
    public required int Age { get; init; }

    [TupleKey(1)]
    public required string Name { get; init; }

    [TupleKey(3)]
    public required string Class { get; init; }

    public bool Equals(Student? other) => other is not null && Id == other.Id && Age == other.Age && Name == other.Name && Class == other.Class;

    public override bool Equals(object? obj) => Equals(obj as Student);

    public override int GetHashCode() => HashCode.Combine(Id, Age, Name, Class);
}

public class MemberOrderTests
{
    public static IEnumerable<object[]> OrderedMembersData()
    {
        yield return new object[] { new Person { Id = 100, Name = "Nice!" }, (100, "Nice!") };
        yield return new object[] { new Person { Id = 1024, Name = "nAmE" }, (1024, "nAmE") };
        yield return new object[] { new ValuePerson { Id = 286, Name = "Classical" }, (286, "Classical") };
        yield return new object[] { new ValuePerson { Id = 686, Name = "History" }, (686, "History") };
    }

    public static IEnumerable<object[]> UnorderedMembersData()
    {
        yield return new object[] { new Student { Age = 20, Class = "C-2", Id = "A8402", Name = "Tom" }, (20, "Tom", "A8402", "C-2") };
        yield return new object[] { new Student { Age = 18, Class = "A-1", Id = "S3031", Name = "Jerry" }, (18, "Jerry", "S3031", "A-1") };
    }

    [Theory(DisplayName = "Member Order Test")]
    [MemberData(nameof(OrderedMembersData))]
    [MemberData(nameof(UnorderedMembersData))]
    public void MemberOrderTest<T, R>(T source, R sample)
    {
        var builder = Generator.CreateDefaultBuilder();
        foreach (var pair in MemberOrderSourceGeneratorContext.ConverterCreators)
            _ = builder.AddConverterCreator(pair.Value);
        var generator = builder.Build();
        var converter = generator.GetConverter<T>();
        Assert.Equal(converter.GetType().Assembly, typeof(MemberOrderSourceGeneratorContext).Assembly);
        Assert.Equal(0, converter.Length);

        var converterSample = generator.GetConverter<R>();
        Assert.Equal("TupleObjectConverter`1", converterSample.GetType().Name);
        Assert.Equal(typeof(IConverter).Assembly, converterSample.GetType().Assembly);

        var a = converter.Encode(source);
        var b = converterSample.Encode(sample);
        Assert.Equal(a, b);

        var c = converterSample.Decode(a);
        var d = converter.Decode(b);
        Assert.Equal(source, d);
        Assert.Equal(sample, c);

        var h = Allocator.Invoke(source, converter.EncodeAuto);
        var i = Allocator.Invoke(sample, converterSample.EncodeAuto);
        Assert.Equal(h, i);

        var j = new ReadOnlySpan<byte>(h);
        var k = new ReadOnlySpan<byte>(i);
        var l = converterSample.DecodeAuto(ref j);
        var m = converter.DecodeAuto(ref k);

        Assert.Equal(0, j.Length);
        Assert.Equal(0, k.Length);
        Assert.Equal(source, m);
        Assert.Equal(sample, l);
    }
}
