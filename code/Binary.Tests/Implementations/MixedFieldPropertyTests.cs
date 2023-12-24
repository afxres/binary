namespace Mikodev.Binary.Tests.Implementations;

using Mikodev.Binary.Attributes;
using System;
using System.Linq;
using System.Net;
using Xunit;

public class MixedFieldPropertyTests
{
    public class MixedSimpleClass
    {
        public int FiledA;

        public string? PropertyB { get; set; }

        public double PropertyInitOnlyC { get; init; }
    }

    [Fact(DisplayName = "Mixed Simple Class")]
    public void MixedSimpleClassTest()
    {
        var generator = Generator.CreateDefault();
        var converter = generator.GetConverter<MixedSimpleClass>();
        Assert.Matches("NamedObjectDelegateConverter`1.*MixedSimpleClass", converter.GetType().FullName);

        var source = new MixedSimpleClass { FiledA = 14, PropertyB = "Network", PropertyInitOnlyC = double.E };
        var buffer = converter.Encode(source);
        var result = converter.Decode(buffer);
        Assert.False(ReferenceEquals(source, result));
        Assert.Equal(source.FiledA, result.FiledA);
        Assert.Equal(source.PropertyB, result.PropertyB);
        Assert.Equal(source.PropertyInitOnlyC, result.PropertyInitOnlyC);

        var token = new Token(generator, buffer);
        var keys = new[] { nameof(MixedSimpleClass.FiledA), nameof(MixedSimpleClass.PropertyB), nameof(MixedSimpleClass.PropertyInitOnlyC) };
        Assert.Equal([.. keys], token.Children.Keys.ToHashSet());
    }

    [NamedObject]
    public struct MixedNamedObject
    {
        [NamedKey("id")]
        public int Id;

        public float IgnoredField;

        [NamedKey("name")]
        public string? Name { get; set; }

        public IPAddress? IgnoredProperty { get; set; }
    }

    [Fact(DisplayName = "Mixed Named Object")]
    public void MixedNamedObjectTest()
    {
        var generator = Generator.CreateDefault();
        var converter = generator.GetConverter<MixedNamedObject>();
        Assert.Matches("NamedObjectDelegateConverter`1.*MixedNamedObject", converter.GetType().FullName);

        var source = new MixedNamedObject { Id = 24, Name = "拼音", IgnoredField = float.Pi, IgnoredProperty = IPAddress.Loopback };
        var buffer = converter.Encode(source);
        var result = converter.Decode(buffer);
        Assert.Equal(source.Id, result.Id);
        Assert.Equal(source.Name, result.Name);
        Assert.Equal(0F, result.IgnoredField);
        Assert.Null(result.IgnoredProperty);

        var token = new Token(generator, buffer);
        var keys = new[] { "id", "name" };
        Assert.Equal([.. keys], token.Children.Keys.ToHashSet());
    }

    [TupleObject]
    public class MixedTupleObject
    {
        [TupleKey(0)]
        public int Id;

        public float IgnoredField;

        [TupleKey(1)]
        public string? Name { get; set; }

        public IPAddress? IgnoredProperty { get; set; }
    }

    [Fact(DisplayName = "Mixed Tuple Object")]
    public void MixedTupleObjectTest()
    {
        var generator = Generator.CreateDefault();
        var converter = generator.GetConverter<MixedTupleObject>();
        Assert.Matches("TupleObjectDelegateConverter`1.*MixedTupleObject", converter.GetType().FullName);

        var source = new MixedTupleObject { Id = 30, Name = "input", IgnoredField = float.Epsilon, IgnoredProperty = IPAddress.IPv6Loopback };
        var buffer = converter.Encode(source);
        var result = converter.Decode(buffer);
        Assert.False(ReferenceEquals(source, result));
        Assert.Equal(source.Id, result.Id);
        Assert.Equal(source.Name, result.Name);
        Assert.Equal(0F, result.IgnoredField);
        Assert.Null(result.IgnoredProperty);

        var compat = generator.Decode<(int, string)>(buffer);
        Assert.Equal((30, "input"), compat);
    }

    public class MixedConstructedClass
    {
        public int Field;

        public string? FieldSetViaConstructor;

        public double Property { get; set; }

        public IPAddress? PropertySetViaConstructor { get; set; }

        public IPAddress? PropertySetViaConstructorReadOnly { get; }

        public MixedConstructedClass() => throw new InvalidOperationException();

        public MixedConstructedClass(IPAddress propertySetViaConstructor) => throw new InvalidOperationException();

        public MixedConstructedClass(string fieldSetViaConstructor, IPAddress propertySetViaConstructor, IPAddress propertySetViaConstructorReadOnly)
        {
            this.FieldSetViaConstructor = fieldSetViaConstructor;
            this.PropertySetViaConstructor = propertySetViaConstructor;
            this.PropertySetViaConstructorReadOnly = propertySetViaConstructorReadOnly;
        }
    }

    [Fact(DisplayName = "Mixed Constructed Class")]
    public void MixedConstructedClassTest()
    {
        var generator = Generator.CreateDefault();
        var converter = generator.GetConverter<MixedConstructedClass>();
        Assert.Matches("NamedObjectDelegateConverter`1.*MixedConstructedClass", converter.GetType().FullName);

        var source = new MixedConstructedClass("fsvc", IPAddress.Loopback, IPAddress.IPv6Loopback) { Field = 54, Property = double.Tau };
        var buffer = converter.Encode(source);
        var result = converter.Decode(buffer);
        Assert.Equal(source.Field, result.Field);
        Assert.Equal(source.FieldSetViaConstructor, result.FieldSetViaConstructor);
        Assert.Equal(source.Property, result.Property);
        Assert.Equal(source.PropertySetViaConstructor, result.PropertySetViaConstructor);
        Assert.Equal(source.PropertySetViaConstructorReadOnly, result.PropertySetViaConstructorReadOnly);

        _ = Assert.Throws<InvalidOperationException>(() => new MixedConstructedClass());
        _ = Assert.Throws<InvalidOperationException>(() => new MixedConstructedClass(IPAddress.Broadcast));
    }
}
