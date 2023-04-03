namespace Mikodev.Binary.Tests.Components;

using Mikodev.Binary.Components;
using Mikodev.Binary.Tests.Internal;
using System;
using System.Linq;
using System.Reflection;
using Xunit;

public class NamedObjectTests
{
    [Fact(DisplayName = "Argument Null Test")]
    public void ArgumentNullTest()
    {
        var generator = Generator.CreateAot();
        var a = new AllocatorAction<int>((ref Allocator allocator, int data) => throw new NotSupportedException());
        var b = generator.GetConverter<string>();
        var c = Enumerable.Empty<string>();
        var d = Enumerable.Empty<bool>();
        var h = Assert.Throws<ArgumentNullException>(() => NamedObject.GetNamedObjectConverter<int>(null!, null, b, c, d));
        var i = Assert.Throws<ArgumentNullException>(() => NamedObject.GetNamedObjectConverter(a, null, null!, c, d));
        var j = Assert.Throws<ArgumentNullException>(() => NamedObject.GetNamedObjectConverter(a, null, b, null!, d));
        var k = Assert.Throws<ArgumentNullException>(() => NamedObject.GetNamedObjectConverter(a, null, b, c, null!));
        var methodInfo = ReflectionExtensions.GetMethodNotNull(typeof(NamedObject), "GetNamedObjectConverter", BindingFlags.Static | BindingFlags.Public);
        var parameterNames = methodInfo.GetParameters();
        Assert.Equal(parameterNames[0].Name, h.ParamName);
        Assert.Equal(parameterNames[2].Name, i.ParamName);
        Assert.Equal(parameterNames[3].Name, j.ParamName);
        Assert.Equal(parameterNames[4].Name, k.ParamName);
    }

    [Fact(DisplayName = "Argument Collection Empty Test")]
    public void ArgumentCollectionEmptyTest()
    {
        var generator = Generator.CreateAot();
        var a = new AllocatorAction<int>((ref Allocator allocator, int data) => throw new NotSupportedException());
        var b = generator.GetConverter<string>();
        var h = Assert.Throws<ArgumentException>(() => NamedObject.GetNamedObjectConverter(a, null, b, Array.Empty<string>(), new[] { false }));
        var i = Assert.Throws<ArgumentException>(() => NamedObject.GetNamedObjectConverter(a, null, b, new[] { string.Empty }, Array.Empty<bool>()));
        Assert.Null(h.ParamName);
        Assert.Null(i.ParamName);
        Assert.Equal($"Collection is empty.", h.Message);
        Assert.Equal($"Collection is empty.", i.Message);
    }

    [Fact(DisplayName = "Argument Collection Lengths Not Match Test")]
    public void ArgumentCollectionLengthsNotMatch()
    {
        var generator = Generator.CreateAot();
        var a = new AllocatorAction<int>((ref Allocator allocator, int data) => throw new NotSupportedException());
        var b = generator.GetConverter<string>();
        var h = Assert.Throws<ArgumentException>(() => NamedObject.GetNamedObjectConverter(a, null, b, new[] { string.Empty }, new[] { false, true }));
        Assert.Null(h.ParamName);
        Assert.Equal($"Collection lengths not match.", h.Message);
    }
}
