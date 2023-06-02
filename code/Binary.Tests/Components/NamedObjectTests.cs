namespace Mikodev.Binary.Tests.Components;

using Mikodev.Binary.Components;
using Mikodev.Binary.Tests.Internal;
using System;
using System.Collections.Generic;
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
        Assert.Equal($"Sequence contains no element.", h.Message);
        Assert.Equal($"Sequence contains no element.", i.Message);
    }

    [Fact(DisplayName = "Argument Collection Lengths Not Match Test")]
    public void ArgumentCollectionLengthsNotMatch()
    {
        var generator = Generator.CreateAot();
        var a = new AllocatorAction<int>((ref Allocator allocator, int data) => throw new NotSupportedException());
        var b = generator.GetConverter<string>();
        var h = Assert.Throws<ArgumentException>(() => NamedObject.GetNamedObjectConverter(a, null, b, new[] { string.Empty }, new[] { false, true }));
        Assert.Null(h.ParamName);
        Assert.Equal($"Sequence lengths not match.", h.Message);
    }

    [Fact(DisplayName = "Constructor Parameter Structure Equals Not Supported")]
    public void ConstructorParameterStructureEqualsNotSupported()
    {
        _ = Assert.Throws<NotSupportedException>(() => default(NamedObjectConstructorParameter).Equals(null));
    }

    [Fact(DisplayName = "Constructor Parameter Structure Get Hash Code Not Supported")]
    public void ConstructorParameterStructureGetHashCodeNotSupported()
    {
        _ = Assert.Throws<NotSupportedException>(() => default(NamedObjectConstructorParameter).GetHashCode());
    }

    [Fact(DisplayName = "Constructor Parameter Structure To String Default Value")]
    public void ConstructorParameterStructureToStringDefaultValue()
    {
        var result = default(NamedObjectConstructorParameter).ToString();
        var message = "Value Count = 0, Memory Length = 0";
        Assert.Equal(message, result);
    }

    public static IEnumerable<object[]> ConstructorParameterStructureToStringData()
    {
        yield return new[] { new { Id = 1, Name = "First" } };
        yield return new[] { new { Age = 18, Tag = "Good", Pass = true } };
    }

    [Theory(DisplayName = "Constructor Parameter Structure To String Test")]
    [MemberData(nameof(ConstructorParameterStructureToStringData))]
    public void ConstructorParameterStructureToStringTest<T>(T source)
    {
        var message = default(string);
        var generator = Generator.CreateDefault();
        var action = new AllocatorAction<Token?>((ref Allocator allocator, Token? token) => throw new NotSupportedException());
        var constructor = new NamedObjectConstructor<Token?>(parameter =>
        {
            message = parameter.ToString();
            return null;
        });
        var buffer = generator.Encode(source);
        var token = new Token(generator, buffer);
        Assert.NotEmpty(token.Children);
        var names = token.Children.Keys.ToList();
        Assert.NotEmpty(names);
        var converter = NamedObject.GetNamedObjectConverter(action, constructor, generator.GetConverter<string>(), names, Enumerable.Repeat(false, names.Count));
        Assert.Null(message);
        Assert.Null(converter.Decode(buffer));
        Assert.NotNull(message);
        var messageExpected = $"Value Count = {names.Count}, Memory Length = {buffer.Length}";
        Assert.Equal(messageExpected, message);
    }
}
