namespace Mikodev.Binary.Tests.Implementations;

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
}
