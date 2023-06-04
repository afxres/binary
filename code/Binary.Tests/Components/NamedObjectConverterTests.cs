namespace Mikodev.Binary.Tests.Components;

using Mikodev.Binary;
using Mikodev.Binary.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

public class NamedObjectConverterTests
{
    private sealed class FakeNamedObjectConverter<T> : NamedObjectConverter<T>
    {
        public FakeNamedObjectConverter(Converter<string> converter, IEnumerable<string> names, IEnumerable<bool> optional) : base(converter, names, optional) { }

        public override T Decode(NamedObjectParameter parameter) => throw new NotImplementedException();

        public override void Encode(ref Allocator allocator, T? item) => throw new NotImplementedException();
    }

    [Fact(DisplayName = "Argument Null Test")]
    public void ArgumentNullTest()
    {
        var generator = Generator.CreateAot();
        var b = generator.GetConverter<string>();
        var c = Enumerable.Empty<string>();
        var d = Enumerable.Empty<bool>();
        var i = Assert.Throws<ArgumentNullException>(() => new FakeNamedObjectConverter<object>(null!, c, d));
        var j = Assert.Throws<ArgumentNullException>(() => new FakeNamedObjectConverter<object>(b, null!, d));
        var k = Assert.Throws<ArgumentNullException>(() => new FakeNamedObjectConverter<object>(b, c, null!));
        var constructorInfo = typeof(NamedObjectConverter<object>).GetConstructors().Single();
        var parameterNames = constructorInfo.GetParameters();
        Assert.Equal(parameterNames[0].Name, i.ParamName);
        Assert.Equal(parameterNames[1].Name, j.ParamName);
        Assert.Equal(parameterNames[2].Name, k.ParamName);
    }

    [Fact(DisplayName = "Argument Collection Empty Test")]
    public void ArgumentCollectionEmptyTest()
    {
        var generator = Generator.CreateAot();
        var b = generator.GetConverter<string>();
        var h = Assert.Throws<ArgumentException>(() => new FakeNamedObjectConverter<object>(b, Array.Empty<string>(), new[] { false }));
        var i = Assert.Throws<ArgumentException>(() => new FakeNamedObjectConverter<object>(b, new[] { string.Empty }, Array.Empty<bool>()));
        Assert.Null(h.ParamName);
        Assert.Null(i.ParamName);
        Assert.Equal($"Sequence contains no element.", h.Message);
        Assert.Equal($"Sequence contains no element.", i.Message);
    }

    [Fact(DisplayName = "Argument Collection Lengths Not Match Test")]
    public void ArgumentCollectionLengthsNotMatch()
    {
        var generator = Generator.CreateAot();
        var b = generator.GetConverter<string>();
        var h = Assert.Throws<ArgumentException>(() => new FakeNamedObjectConverter<object>(b, new[] { string.Empty }, new[] { false, true }));
        Assert.Null(h.ParamName);
        Assert.Equal($"Sequence lengths not match.", h.Message);
    }
}
