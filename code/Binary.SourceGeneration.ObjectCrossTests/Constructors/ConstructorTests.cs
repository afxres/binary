namespace Mikodev.Binary.SourceGeneration.ObjectCrossTests.Constructors;

using Mikodev.Binary.Attributes;
using System;
using System.Collections.Generic;
using Xunit;

[SourceGeneratorContext]
[SourceGeneratorInclude<TestNamedObject>]
[SourceGeneratorInclude<TestTupleObject>]
[SourceGeneratorInclude<TestPlainObject>]
public partial class ConstructorGeneratorContext { }

#pragma warning disable IDE0051 // Remove unused private members
[NamedObject]
public class TestNamedObject : IEquatable<TestNamedObject?>
{
    [NamedKey("main-key")]
    public int Id { get; }

    [NamedKey("data-name")]
    public string? Name { get; set; }

    [NamedKey("data-role")]
    public string? Role { get; set; }

    public TestNamedObject(string name, string role) => throw new NotSupportedException();

    public TestNamedObject(int id)
    {
        Id = id;
    }

    private TestNamedObject(int id, string name, string role) => throw new NotSupportedException();

    internal TestNamedObject(string name, string role, int id) => throw new NotSupportedException();

    public bool Equals(TestNamedObject? other) => other is not null && Id == other.Id && Name == other.Name && Role == other.Role;

    public override bool Equals(object? obj) => Equals(obj as TestNamedObject);

    public override int GetHashCode() => HashCode.Combine(Id, Name, Role);
}

[TupleObject]
public class TestTupleObject : IEquatable<TestTupleObject?>
{
    [TupleKey(1)]
    public long Key { get; }

    [TupleKey(2)]
    public int Priority { get; internal set; }

    [TupleKey(0)]
    public string? Message { get; set; }

    public TestTupleObject(long key, string message) => throw new NotSupportedException();

    public TestTupleObject(string message, Guid key, ulong priority) => throw new NotSupportedException();

    public TestTupleObject(int priority, long key)
    {
        Key = key;
        Priority = priority;
    }

    public TestTupleObject(long key) => throw new NotSupportedException();

    private TestTupleObject(long key, int priority, string message) => throw new NotSupportedException();

    internal TestTupleObject(string message, int priority, long key) => throw new NotSupportedException();

    public bool Equals(TestTupleObject? other) => other is not null && Key == other.Key && Priority == other.Priority && Message == other.Message;

    public override bool Equals(object? obj) => Equals(obj as TestTupleObject);

    public override int GetHashCode() => HashCode.Combine(Key, Priority, Message);
}

public class TestPlainObject : IEquatable<TestPlainObject?>
{
    public readonly short Identity;

    public string? Content { get; set; }

    public Uri HelpLink { get; set; }

    private TestPlainObject(string? content, short identity, Uri helpLink) => throw new NotSupportedException();

    internal TestPlainObject(short identity, string? content, Uri helpLink) => throw new NotSupportedException();

    public TestPlainObject(string? content, Uri helpLink) => throw new NotSupportedException();

    public TestPlainObject(Uri helpLink, short identity)
    {
        this.Identity = identity;
        HelpLink = helpLink;
    }

    public TestPlainObject(short identity, string? content, Uri helpLink, Guid whatIsThis) => throw new NotSupportedException();

    public bool Equals(TestPlainObject? other) => other is not null && this.Identity == other.Identity && Content == other.Content && EqualityComparer<Uri>.Default.Equals(HelpLink, other.HelpLink);

    public override bool Equals(object? obj) => Equals(obj as TestPlainObject);

    public override int GetHashCode() => HashCode.Combine(this.Identity, Content, HelpLink);
}
#pragma warning restore IDE0051 // Remove unused private members

public class ConstructorTests
{
    public static IEnumerable<object[]> ConstructorData()
    {
        yield return [new TestNamedObject(3) { Name = "Zero", Role = "None" }];
        yield return [new TestNamedObject(10) { Name = "Alpha", Role = "Administrator" }];
        yield return [new TestTupleObject(255, 1L) { Message = "Error" }];
        yield return [new TestTupleObject(127, 3L) { Message = "Warning" }];
        yield return [new TestPlainObject(new Uri("https://github.com/"), 4096) { Content = "Nice!" }];
        yield return [new TestPlainObject(new Uri("https://www.bing.com/"), 6666) { Content = "Bravo!" }];
    }

    [Theory(DisplayName = "Constructor Test")]
    [MemberData(nameof(ConstructorData))]
    public void ConstructorTest<T>(T source)
    {
        var generator = Generator.CreateAotBuilder()
            .AddConverterCreators(ConstructorGeneratorContext.ConverterCreators.Values)
            .Build();
        var converter = generator.GetConverter<T>();
        var buffer = converter.Encode(source);
        Assert.NotEmpty(buffer);

        var generatorSecond = Generator.CreateDefault();
        var converterSecond = generatorSecond.GetConverter<T>();
        var bufferSecond = converterSecond.Encode(source);

        var result = converterSecond.Decode(buffer);
        var resultSecond = converter.Decode(bufferSecond);
        Assert.Equal(source, result);
        Assert.Equal(source, resultSecond);
    }
}
