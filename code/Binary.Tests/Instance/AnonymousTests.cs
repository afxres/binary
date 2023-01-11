namespace Mikodev.Binary.Tests.Instance;

using System;
using System.Collections.Generic;
using Xunit;

public class AnonymousTests
{
    private readonly IGenerator generator = Generator.CreateDefault();

    [Fact(DisplayName = "Get Converter For Anonymous Type")]
    public void GetConverter()
    {
        var a = new { name = "Tom", age = 20 };
        var converter = this.generator.GetConverter(a);
        Assert.NotNull(converter);
        Assert.Equal(0, converter.Length);
    }

    [Fact(DisplayName = "Get Converter For Empty Anonymous Type")]
    public void Empty()
    {
        var a = new { };
        var error = Assert.Throws<ArgumentException>(() => this.generator.GetConverter(a));
        var message = $"No available member found, type: {a.GetType()}";
        Assert.Equal(message, error.Message);
    }

    [Fact(DisplayName = "Encode Then Decode")]
    public void Convert()
    {
        var a = new { id = "some", data = new { name = "Bob" } };
        var bytes = this.generator.Encode(a);
        Assert.NotEmpty(bytes);
        var value = this.generator.Decode(bytes, a);
        Assert.False(ReferenceEquals(a, value));
        Assert.Equal(a, value);
    }

    [Fact(DisplayName = "Decode Empty Bytes")]
    public void ValueFromEmptyBytes()
    {
        var bytes = Array.Empty<byte>();
        var value = this.generator.Decode(bytes, new { key = default(string) });
        Assert.Null(value);
    }

    [Fact(DisplayName = "Encode Null Value")]
    public void NullValue()
    {
        static T? DefaultOf<T>(T _) => default;

        var a = DefaultOf(new { key = default(string), value = default(int) });
        Assert.Null(a);
        var bytes = this.generator.Encode(a);
        Assert.Empty(bytes);
    }

    [Fact(DisplayName = "Token As Value")]
    public void TokenAs()
    {
        var a = new { guid = Guid.NewGuid(), inner = new { name = "Pro C# ...", price = 51.2 } };
        var bytes = this.generator.Encode(a);
        var token = new Token(this.generator, bytes);
        var value = token.As(a);
        Assert.False(ReferenceEquals(a, value));
        Assert.Equal(a, value);

        var inner = token["inner"].As(a.inner);
        Assert.False(ReferenceEquals(a.inner, inner));
        Assert.Equal(a.inner, inner);
    }

    [Fact(DisplayName = "No Suitable Constructor (case sensitive)")]
    public void CaseSensitive()
    {
        var a = new { name = "Tex", Name = 0 };
        var bytes = this.generator.Encode(a);
        var bravo = this.generator.Encode(new SortedDictionary<string, object> { ["name"] = "Tex", ["Name"] = 0 });
        Assert.Equal(bravo, bytes);
        var error = Assert.Throws<NotSupportedException>(() => this.generator.Decode(bytes, anonymous: a));
        var message = $"No suitable constructor found, type: {a.GetType()}";
        Assert.Equal(message, error.Message);
    }
}
