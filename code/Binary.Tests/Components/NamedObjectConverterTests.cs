namespace Mikodev.Binary.Tests.Components;

using Mikodev.Binary;
using Mikodev.Binary.Components;
using System;
using System.Collections.Generic;
using Xunit;

public class NamedObjectConverterTests
{
    private sealed class FakeNamedObjectConverter<T>(IEnumerable<IEnumerable<byte>> headers, IEnumerable<string> names, IEnumerable<bool> optional) : NamedObjectConverter<T>(headers, names, optional)
    {
        public override T Decode(NamedObjectParameter parameter) => throw new NotImplementedException();

        public override void Encode(ref Allocator allocator, T? item) => throw new NotImplementedException();
    }

    [Fact(DisplayName = "Argument Null Test")]
    public void ArgumentNullTest()
    {
        var h = Assert.Throws<ArgumentException>(() => new FakeNamedObjectConverter<object>(null!, [], []));
        var i = Assert.Throws<ArgumentException>(() => new FakeNamedObjectConverter<object>([], null!, []));
        var k = Assert.Throws<ArgumentException>(() => new FakeNamedObjectConverter<object>([], [], null!));
        Assert.Null(h.ParamName);
        Assert.Null(i.ParamName);
        Assert.Null(k.ParamName);
        Assert.Equal($"Sequence is null or empty.", h.Message);
        Assert.Equal($"Sequence is null or empty.", i.Message);
        Assert.Equal($"Sequence is null or empty.", k.Message);
    }

    [Fact(DisplayName = "Argument Collection Empty Test")]
    public void ArgumentCollectionEmptyTest()
    {
        var h = Assert.Throws<ArgumentException>(() => new FakeNamedObjectConverter<object>([], [string.Empty], [default]));
        var i = Assert.Throws<ArgumentException>(() => new FakeNamedObjectConverter<object>([[]], [], [default]));
        var k = Assert.Throws<ArgumentException>(() => new FakeNamedObjectConverter<object>([[]], [string.Empty], []));
        Assert.Null(h.ParamName);
        Assert.Null(i.ParamName);
        Assert.Null(k.ParamName);
        Assert.Equal($"Sequence is null or empty.", h.Message);
        Assert.Equal($"Sequence is null or empty.", i.Message);
        Assert.Equal($"Sequence is null or empty.", k.Message);
    }

    [Fact(DisplayName = "Argument Collection Lengths Not Match Test")]
    public void ArgumentCollectionLengthsNotMatch()
    {
        var h = Assert.Throws<ArgumentException>(() => new FakeNamedObjectConverter<object>([[]], [string.Empty], [default, default]));
        var i = Assert.Throws<ArgumentException>(() => new FakeNamedObjectConverter<object>([[]], [string.Empty, string.Empty], [default]));
        var k = Assert.Throws<ArgumentException>(() => new FakeNamedObjectConverter<object>([[], []], [string.Empty], [default]));
        Assert.Null(h.ParamName);
        Assert.Null(i.ParamName);
        Assert.Null(k.ParamName);
        Assert.Equal($"Sequence lengths not match.", h.Message);
        Assert.Equal($"Sequence lengths not match.", i.Message);
        Assert.Equal($"Sequence lengths not match.", k.Message);
    }
}
