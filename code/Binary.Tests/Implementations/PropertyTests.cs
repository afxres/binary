namespace Mikodev.Binary.Tests.Implementations;

using System;
using Xunit;

#pragma warning disable IDE0051 // Remove unused private members

public class PropertyTests
{
    private class Class
    {
        public static int StaticA { get; set; }

        public static string StaticB { get; set; }

        private static int InvalidC { get; }

        private static string InvalidD { set => throw new NotImplementedException(); }

        public int E { get; set; }

        public string F { get; set; }

        private int InvalidG { get; set; }

        public int this[string i] => throw new NotImplementedException();

        public string this[int i] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }

    private struct Value
    {
        public static int StaticA { get; set; }

        public static string StaticB { get; set; }

        private static int InvalidC { get; }

        private static string InvalidD { set => throw new NotImplementedException(); }

        public int E { get; set; }

        public string F { get; set; }

        private int InvalidG { get; set; }

        public int this[string i] => throw new NotImplementedException();

        public string this[int i] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }

    [Fact(DisplayName = "Class Type Properties")]
    public void ClassTypeProperties()
    {
        var generator = Generator.CreateDefault();
        var random = new Random();
        var a = random.Next();
        var b = random.Next().ToString();
        Class.StaticA = a;
        Class.StaticB = b;

        var source = new Class() { E = 172, F = "internal" };
        var buffer = generator.Encode(source);
        var result = generator.Decode<Class>(buffer);
        Assert.False(ReferenceEquals(source, result));
        Assert.Equal(source.E, result.E);
        Assert.Equal(source.F, result.F);

        var token = new Token(generator, buffer);
        var dictionary = token.Children;
        Assert.Equal(2, dictionary.Count);

        Assert.Equal(a, Class.StaticA);
        Assert.Equal(b, Class.StaticB);
    }

    [Fact(DisplayName = "Value Type Properties")]
    public void ValueTypeProperties()
    {
        var generator = Generator.CreateDefault();
        var random = new Random();
        var a = random.Next();
        var b = random.Next().ToString();
        Value.StaticA = a;
        Value.StaticB = b;

        var source = new Value() { E = 256, F = "protected" };
        var buffer = generator.Encode(source);
        var result = generator.Decode<Value>(buffer);

        Assert.Equal(source.E, result.E);
        Assert.Equal(source.F, result.F);

        var token = new Token(generator, buffer);
        var dictionary = token.Children;
        Assert.Equal(2, dictionary.Count);

        Assert.Equal(a, Value.StaticA);
        Assert.Equal(b, Value.StaticB);
    }
}
