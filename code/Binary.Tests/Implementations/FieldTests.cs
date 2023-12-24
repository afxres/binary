namespace Mikodev.Binary.Tests.Implementations;

using System;
using Xunit;

#pragma warning disable IDE0051 // Remove unused private members
#pragma warning disable IDE0044 // Add readonly modifier
#pragma warning disable CS0169  // The field '...' is never used
public class FieldTests
{
    private class TestClass
    {
        public static int PublicStatic;

        private static int PrivateStatic;

        private string? PrivateInstance;

        public int A;

        public string? B;
    }

    private struct TestValue
    {
        public static int PublicStatic;

        private static int PrivateStatic;

        private string? PrivateInstance;

        public int A;

        public string? B;
    }

    [Fact(DisplayName = "Class Type Fields")]
    public void ClassFieldsTests()
    {
        var generator = Generator.CreateDefault();
        var immutable = Random.Shared.Next();
        TestClass.PublicStatic = immutable;

        var source = new TestClass { A = 2002, B = "2301" };
        var buffer = generator.Encode(source);
        var result = generator.Decode<TestClass>(buffer);
        Assert.False(ReferenceEquals(source, result));
        Assert.Equal(source.A, result.A);
        Assert.Equal(source.B, result.B);

        var token = new Token(generator, buffer);
        var dictionary = token.Children;
        Assert.Equal(2, dictionary.Count);
        Assert.Equal(immutable, TestClass.PublicStatic);
    }

    [Fact(DisplayName = "Value Type Fields")]
    public void ValueFieldsTests()
    {
        var generator = Generator.CreateDefault();
        var immutable = Random.Shared.Next();
        TestValue.PublicStatic = immutable;

        var source = new TestValue { A = 2023, B = "January" };
        var buffer = generator.Encode(source);
        var result = generator.Decode<TestValue>(buffer);
        Assert.Equal(source.A, result.A);
        Assert.Equal(source.B, result.B);

        var token = new Token(generator, buffer);
        var dictionary = token.Children;
        Assert.Equal(2, dictionary.Count);
        Assert.Equal(immutable, TestValue.PublicStatic);
    }

    public class ReadWriteFieldType
    {
        public int Field;
    }

    [Fact(DisplayName = "Read Write Field Test")]
    public void ReadWriteFieldTest()
    {
        var generator = Generator.CreateDefault();
        var converter = generator.GetConverter<ReadWriteFieldType>();
        var source = new ReadWriteFieldType { Field = 65535 };
        var buffer = converter.Encode(source);
        var result = converter.Decode(buffer);
        Assert.Equal(source.Field, result.Field);
    }

    public class ReadOnlyFieldPrivateConstructorType
    {
        public readonly int Field;

        private ReadOnlyFieldPrivateConstructorType(int field) => this.Field = field;

        public static ReadOnlyFieldPrivateConstructorType Create(int field) => new ReadOnlyFieldPrivateConstructorType(field);
    }

    [Fact(DisplayName = "Read Only Field Private Constructor")]
    public void ReadOnlyFieldPrivateConstructorTest()
    {
        var generator = Generator.CreateDefault();
        var converter = generator.GetConverter<ReadOnlyFieldPrivateConstructorType>();
        var source = ReadOnlyFieldPrivateConstructorType.Create(1333);
        var buffer = converter.Encode(source);
        var token = new Token(generator, buffer);
        Assert.Equal(new string[] { "Field" }, token.Children.Keys);
        Assert.Equal(source.Field, token["Field"].As<int>());
        var error = Assert.Throws<NotSupportedException>(() => converter.Decode(buffer));
        var message = $"No suitable constructor found, type: {typeof(ReadOnlyFieldPrivateConstructorType)}";
        Assert.Equal(message, error.Message);
    }

    public class ReadOnlyFieldPublicConstructorType(string item)
    {
        public readonly string Item = "Ha" + item;
    }

    [Fact(DisplayName = "Read Only Field Public Constructor")]
    public void ReadOnlyFieldPublicConstructorTest()
    {
        var generator = Generator.CreateDefault();
        var converter = generator.GetConverter<ReadOnlyFieldPublicConstructorType>();
        var source = new ReadOnlyFieldPublicConstructorType("!");
        Assert.Equal("Ha!", source.Item);
        var buffer = converter.Encode(source);
        var result = converter.Decode(buffer);
        Assert.Equal("HaHa!", result.Item);
    }
}
#pragma warning restore CS0169  // The field '...' is never used
#pragma warning restore IDE0044 // Add readonly modifier
#pragma warning restore IDE0051 // Remove unused private members
