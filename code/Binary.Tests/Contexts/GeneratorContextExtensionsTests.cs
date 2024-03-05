namespace Mikodev.Binary.Tests.Contexts;

using System;
using System.Collections.Generic;
using Xunit;

public class GeneratorContextExtensionsTests
{
    private class FakeConverter<T> : Converter<T>
    {
        public override T Decode(in ReadOnlySpan<byte> span) => throw new NotSupportedException();

        public override void Encode(ref Allocator allocator, T? item) => throw new NotSupportedException();
    }

    private class FakeGeneratorContext : IGeneratorContext
    {
        public List<Type> Records { get; } = [];

        public IConverter GetConverter(Type type)
        {
            var converter = Activator.CreateInstance(typeof(FakeConverter<>).MakeGenericType(type));
            Records.Add(type);
            return (IConverter)converter!;
        }
    }

    [Fact(DisplayName = "Get Converter Test")]
    public void GetConverterTest()
    {
        var context = new FakeGeneratorContext();
        Assert.Empty(context.Records);
        var a = context.GetConverter<int>();
        Assert.Equal(context.Records, [(typeof(int))]);
        _ = Assert.IsType<FakeConverter<int>>(a);
        var b = context.GetConverter<string>();
        Assert.Equal(context.Records, [typeof(int), typeof(string)]);
        _ = Assert.IsType<FakeConverter<string>>(b);
    }

    [Fact(DisplayName = "Get Converter For Anonymous Test")]
    public void GetConverterAnonymousTest()
    {
        var context = new FakeGeneratorContext();
        Assert.Empty(context.Records);
        var x = new { id = 192 };
        var a = context.GetConverter(x);
        Assert.Equal(context.Records, [x.GetType()]);
        Assert.IsType(typeof(FakeConverter<>).MakeGenericType(x.GetType()), a);
        var y = new { key = Guid.Empty, data = "empty" };
        var b = context.GetConverter(y);
        Assert.Equal(context.Records, [x.GetType(), y.GetType()]);
        Assert.IsType(typeof(FakeConverter<>).MakeGenericType(y.GetType()), b);
    }
}
