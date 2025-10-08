namespace Mikodev.Binary.SourceGeneration.AttributeTests.MemberConverterAttributeTests;

using Mikodev.Binary.Attributes;
using System;
using System.Text;
using Xunit;

public class UnicodeStringConverter : Converter<string>
{
    public override void Encode(ref Allocator allocator, string? item)
    {
        if (item is null)
            return;
        var encoding = Encoding.Unicode;
        Allocator.Append(ref allocator, encoding.GetMaxByteCount(item.Length), item, (span, data) => encoding.GetBytes(data.AsSpan(), span));
    }

    public override string Decode(in ReadOnlySpan<byte> span)
    {
        return Encoding.Unicode.GetString(span);
    }
}

[SourceGeneratorContext]
[SourceGeneratorInclude<Person>]
public partial class PersonSourceGeneratorContext { }

[TupleObject]
public class Person
{
    [TupleKey(0)]
    [Converter(typeof(UnicodeStringConverter))]
    public string? Name { get; set; }
}

public class TupleObjectMemberWithConverterAttributeTests
{
    [Fact(DisplayName = "Member Converter Attribute Test")]
    public void MemberConverterAttributeTest()
    {
        var pair = Assert.Single(PersonSourceGeneratorContext.ConverterCreators);
        Assert.Equal(typeof(Person), pair.Key);
        var converter = Assert.IsType<Converter<Person>>(pair.Value.GetConverter(null!, typeof(Person)), exactMatch: false);
        Assert.Equal(converter.GetType().Assembly, typeof(PersonSourceGeneratorContext).Assembly);

        for (var i = 0; i < 16; i++)
        {
            var source = new Person { Name = i.ToString() };
            var buffer = converter.Encode(source);
            var result = converter.Decode(buffer);
            Assert.Equal(source.Name, result.Name);
            var expectedBuffer = Encoding.Unicode.GetBytes(source.Name);
            Assert.Equal(expectedBuffer, buffer);
        }
    }
}
