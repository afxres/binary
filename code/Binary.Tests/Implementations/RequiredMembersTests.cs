namespace Mikodev.Binary.Tests.Implementations;

using Mikodev.Binary.Attributes;
using System;
using System.Linq;
using Xunit;

public class RequiredMembersTests
{
    private class SimplePartialRequiredObject
    {
        public required int Id { get; init; }

        public string? Name { get; init; }
    }

    [Fact(DisplayName = "Partial Required Object Encode")]
    public void PartialRequiredEncode()
    {
        var a = new SimplePartialRequiredObject { Id = 1 };
        var b = new SimplePartialRequiredObject { Id = 2, Name = "Two" };
        var generator = Generator.CreateDefault();
        var ba = generator.Encode(a);
        var bb = generator.Encode(b);
        var ta = new Token(generator, ba);
        var tb = new Token(generator, bb);
        Assert.Equal(new[] { "Id" }.ToHashSet(), ta.Children.Keys.ToHashSet());
        Assert.Equal(new[] { "Id", "Name" }.ToHashSet(), tb.Children.Keys.ToHashSet());
        Assert.Equal(1, ta["Id"].As<int>());
        Assert.Equal(2, tb["Id"].As<int>());
        Assert.Equal("Two", tb["Name"].As<string>());
    }

    [Fact(DisplayName = "Partial Required Object Decode")]
    public void PartialRequiredDecode()
    {
        var a = new { Id = 3 };
        var b = new { Id = 4, Name = "Four" };
        var c = new { Id = 5, Other = "Ignore" };
        var d = new { Id = 6, Name = "Six", Ignore = "Always" };
        var generator = Generator.CreateDefault();
        var ra = generator.Decode<SimplePartialRequiredObject>(generator.Encode(a));
        var rb = generator.Decode<SimplePartialRequiredObject>(generator.Encode(b));
        var rc = generator.Decode<SimplePartialRequiredObject>(generator.Encode(c));
        var rd = generator.Decode<SimplePartialRequiredObject>(generator.Encode(d));
        Assert.Equal(3, ra.Id);
        Assert.Null(ra.Name);
        Assert.Equal(4, rb.Id);
        Assert.Equal("Four", rb.Name);
        Assert.Equal(5, rc.Id);
        Assert.Null(rc.Name);
        Assert.Equal(6, rd.Id);
        Assert.Equal("Six", rd.Name);
    }

    [NamedObject]
    private class MissingNamedKeyOnRequiredObject
    {
        public required int Id { get; init; }
    }

    [TupleObject]
    private class MissingTupleKeyOnRequiredObject
    {
        public required long Flag { get; init; }
    }

    [Fact(DisplayName = "Missing Named Key For Required Member")]
    public void MissingNamedKey()
    {
        var generator = Generator.CreateDefault();
        var error = Assert.Throws<ArgumentException>(generator.GetConverter<MissingNamedKeyOnRequiredObject>);
        var message = $"Require 'NamedKeyAttribute' for required member, member name: Id, type: {typeof(MissingNamedKeyOnRequiredObject)}";
        Assert.Equal(message, error.Message);
    }

    [Fact(DisplayName = "Missing Tuple Key For Required Member")]
    public void MissingTupleKey()
    {
        var generator = Generator.CreateDefault();
        var error = Assert.Throws<ArgumentException>(generator.GetConverter<MissingTupleKeyOnRequiredObject>);
        var message = $"Require 'TupleKeyAttribute' for required member, member name: Flag, type: {typeof(MissingTupleKeyOnRequiredObject)}";
        Assert.Equal(message, error.Message);
    }

    private class DataType
    {
        public required int RequiredId { get; init; }

        public string? OptionalName { get; init; }

        public Guid OptionalGuid { get; init; }

        public required string RequiredMessage { get; init; }
    }

    [Fact(DisplayName = "Decode Missing Optional Member")]
    public void DecodeMissingOptionalMember()
    {
        var generator = Generator.CreateDefault();
        var converter = generator.GetConverter<DataType>();
        var id = Guid.Parse("1a57a6fd-61cd-4faf-81b5-cb97bb8fa5dd");
        var a = converter.Decode(generator.Encode(new { RequiredId = 1024, RequiredMessage = "Hello", OptionalGuid = id }));
        Assert.Equal(1024, a.RequiredId);
        Assert.Equal("Hello", a.RequiredMessage);
        Assert.Null(a.OptionalName);
        Assert.Equal(id, a.OptionalGuid);

        var b = converter.Decode(generator.Encode(new { OptionalName = "Bravo", RequiredMessage = "Nice", RequiredId = 65535 }));
        Assert.Equal(65535, b.RequiredId);
        Assert.Equal("Bravo", b.OptionalName);
        Assert.Equal("Nice", b.RequiredMessage);
        Assert.Equal(default, b.OptionalGuid);
    }

    [Fact(DisplayName = "Decode Missing Required Member")]
    public void DecodeMissingRequiredMember()
    {
        var generator = Generator.CreateDefault();
        var converter = generator.GetConverter<DataType>();
        var a = Assert.Throws<ArgumentException>(() => _ = converter.Decode(generator.Encode(new { RequiredMessage = "0.0" })));
        var m = $"Named key 'RequiredId' does not exist, type: {typeof(DataType)}";
        Assert.Null(a.ParamName);
        Assert.Equal(m, a.Message);

        var b = Assert.Throws<ArgumentException>(() => _ = converter.Decode(generator.Encode(new { RequiredId = 768 })));
        var n = $"Named key 'RequiredMessage' does not exist, type: {typeof(DataType)}";
        Assert.Null(b.ParamName);
        Assert.Equal(n, b.Message);
    }
}
