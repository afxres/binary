namespace Mikodev.Binary.Tests.Contexts;

using System;
using System.Collections.Generic;
using Xunit;

public class GeneratorBuilderExtensionsTests
{
    private sealed class FakeGeneratorBuilder : IGeneratorBuilder
    {
        public List<IConverter> Converters { get; } = new List<IConverter>();

        public List<IConverterCreator> ConverterCreators { get; } = new List<IConverterCreator>();

        public IGeneratorBuilder AddConverter(IConverter converter)
        {
            Converters.Add(converter);
            return this;
        }

        public IGeneratorBuilder AddConverterCreator(IConverterCreator creator)
        {
            ConverterCreators.Add(creator);
            return this;
        }

        public IGenerator Build() => throw new NotSupportedException();
    }

    private sealed class FakeConverter<T> : Converter<T>
    {
        public override void Encode(ref Allocator allocator, T? item) => throw new NotSupportedException();

        public override T Decode(in ReadOnlySpan<byte> span) => throw new NotSupportedException();
    }

    private sealed class FakeConverterCreator<T> : IConverterCreator
    {
        public IConverter? GetConverter(IGeneratorContext context, Type type) => throw new NotSupportedException();
    }

    [Fact(DisplayName = "Add Converters")]
    public void AddConverters()
    {
        var converters = new IConverter[] { new FakeConverter<int>(), new FakeConverter<string>() };
        var builder = new FakeGeneratorBuilder();
        Assert.Empty(builder.Converters);
        var returned = builder.AddConverters(converters);
        Assert.Equal(2, builder.Converters.Count);
        Assert.True(ReferenceEquals(builder, returned));
    }

    [Fact(DisplayName = "Add Converter Creators")]
    public void AddConverterCreators()
    {
        var creators = new IConverterCreator[] { new FakeConverterCreator<Guid>(), new FakeConverterCreator<Uri>() };
        var builder = new FakeGeneratorBuilder();
        Assert.Empty(builder.ConverterCreators);
        var returned = builder.AddConverterCreators(creators);
        Assert.Equal(2, builder.ConverterCreators.Count);
        Assert.True(ReferenceEquals(builder, returned));
    }

    [Fact(DisplayName = "Add Converters (argument null)")]
    public void AddConvertersArgumentNull()
    {
        var a = Assert.Throws<ArgumentNullException>(() => GeneratorBuilderExtensions.AddConverters(null!, Array.Empty<IConverter>()));
        var b = Assert.Throws<ArgumentNullException>(() => GeneratorBuilderExtensions.AddConverters(new FakeGeneratorBuilder(), null!));
        var method = new Func<IGeneratorBuilder, IEnumerable<IConverter>, IGeneratorBuilder>(GeneratorBuilderExtensions.AddConverters).Method;
        Assert.Equal("builder", a.ParamName);
        Assert.Equal("builder", method.GetParameters()[0].Name);
        Assert.Equal("converters", b.ParamName);
        Assert.Equal("converters", method.GetParameters()[1].Name);
    }

    [Fact(DisplayName = "Add Converter Creators (argument null)")]
    public void AddConverterCreatorsArgumentNull()
    {
        var a = Assert.Throws<ArgumentNullException>(() => GeneratorBuilderExtensions.AddConverterCreators(null!, Array.Empty<IConverterCreator>()));
        var b = Assert.Throws<ArgumentNullException>(() => GeneratorBuilderExtensions.AddConverterCreators(new FakeGeneratorBuilder(), null!));
        var method = new Func<IGeneratorBuilder, IEnumerable<IConverterCreator>, IGeneratorBuilder>(GeneratorBuilderExtensions.AddConverterCreators).Method;
        Assert.Equal("builder", a.ParamName);
        Assert.Equal("builder", method.GetParameters()[0].Name);
        Assert.Equal("creators", b.ParamName);
        Assert.Equal("creators", method.GetParameters()[1].Name);
    }
}
