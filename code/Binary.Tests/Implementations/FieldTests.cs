namespace Mikodev.Binary.Tests.Implementations;

using System;
using Xunit;

#pragma warning disable IDE0051 // Remove unused private members
#pragma warning disable IDE0044 // Add readonly modifier
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
}
#pragma warning restore IDE0044 // Add readonly modifier
#pragma warning restore IDE0051 // Remove unused private members
