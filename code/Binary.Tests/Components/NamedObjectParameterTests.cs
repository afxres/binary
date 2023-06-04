namespace Mikodev.Binary.Tests.Components;

using Mikodev.Binary.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

public class NamedObjectParameterTests
{
    [Fact(DisplayName = "Constructor Parameter Structure Equals Not Supported")]
    public void ConstructorParameterStructureEqualsNotSupported()
    {
        _ = Assert.Throws<NotSupportedException>(() => default(NamedObjectParameter).Equals(null));
    }

    [Fact(DisplayName = "Constructor Parameter Structure Get Hash Code Not Supported")]
    public void ConstructorParameterStructureGetHashCodeNotSupported()
    {
        _ = Assert.Throws<NotSupportedException>(() => default(NamedObjectParameter).GetHashCode());
    }

    [Fact(DisplayName = "Constructor Parameter Structure To String Default Value")]
    public void ConstructorParameterStructureToStringDefaultValue()
    {
        var result = default(NamedObjectParameter).ToString();
        var message = "Value Count = 0, Memory Length = 0";
        Assert.Equal(message, result);
    }

    private delegate T FakeNamedObjectDecodeDelegate<T>(scoped NamedObjectParameter parameter);

    private sealed class FakeNamedObjectConverter<T> : NamedObjectConverter<T>
    {
        public FakeNamedObjectDecodeDelegate<T> DecodeDelegate { get; }

        public FakeNamedObjectConverter(FakeNamedObjectDecodeDelegate<T> decodeDelegate, Converter<string> converter, IEnumerable<string> names, IEnumerable<bool> optional) : base(converter, names, optional)
        {
            DecodeDelegate = decodeDelegate;
        }

        public override T Decode(scoped NamedObjectParameter parameter)
        {
            return DecodeDelegate.Invoke(parameter);
        }

        public override void Encode(ref Allocator allocator, T? item)
        {
            throw new NotSupportedException();
        }
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
        var constructor = new FakeNamedObjectDecodeDelegate<object?>(parameter =>
        {
            message = parameter.ToString();
            return null;
        });
        var buffer = generator.Encode(source);
        var token = new Token(generator, buffer);
        Assert.NotEmpty(token.Children);
        var names = token.Children.Keys.ToList();
        Assert.NotEmpty(names);
        var converter = new FakeNamedObjectConverter<object?>(constructor, generator.GetConverter<string>(), names, Enumerable.Repeat(false, names.Count));
        Assert.Null(message);
        Assert.Null(converter.Decode(buffer));
        Assert.NotNull(message);
        var messageExpected = $"Value Count = {names.Count}, Memory Length = {buffer.Length}";
        Assert.Equal(messageExpected, message);
    }
}
