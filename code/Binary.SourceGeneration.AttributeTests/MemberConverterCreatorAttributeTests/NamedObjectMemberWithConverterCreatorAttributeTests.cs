namespace Mikodev.Binary.SourceGeneration.AttributeTests.MemberConverterCreatorAttributeTests;

using Mikodev.Binary.Attributes;
using System;
using System.Linq;
using System.Text;
using Xunit;

public class UTF32StringInt32Converter : Converter<int>
{
    public override void Encode(ref Allocator allocator, int item)
    {
        var text = item.ToString();
        var encoding = Encoding.UTF32;
        Allocator.Append(ref allocator, encoding.GetMaxByteCount(text.Length), text, (span, data) => encoding.GetBytes(data.AsSpan(), span));
    }

    public override int Decode(in ReadOnlySpan<byte> span)
    {
        return int.Parse(Encoding.UTF32.GetString(span));
    }
}

public class UTF32StringInt32ConverterCreator : IConverterCreator
{
    IConverter? IConverterCreator.GetConverter(IGeneratorContext context, Type type)
    {
        return new UTF32StringInt32Converter();
    }
}

[SourceGeneratorContext]
[SourceGeneratorInclude<KeyedItem>]
public partial class KeyedItemSourceGeneratorContext { }

[NamedObject]
public class KeyedItem
{
    [NamedKey("id")]
    [ConverterCreator(typeof(UTF32StringInt32ConverterCreator))]
    public int Id { get; set; }
}

public class NamedObjectMemberWithConverterCreatorAttributeTests
{
    [Fact(DisplayName = "Member With Converter Creator Attribute Test")]
    public void MemberWithConverterCreatorAttributeTest()
    {
        var pair = Assert.Single(KeyedItemSourceGeneratorContext.ConverterCreators);
        Assert.Equal(typeof(KeyedItem), pair.Key);

        var generator = Generator.CreateAotBuilder()
            .AddConverterCreator(pair.Value)
            .Build();
        var converter = generator.GetConverter<KeyedItem>();
        var converterType = converter.GetType();
        Assert.Equal(converterType.Assembly, typeof(KeyedItemSourceGeneratorContext).Assembly);

        for (var i = 0; i < 16; i++)
        {
            var source = new KeyedItem { Id = i };
            var buffer = converter.Encode(source);
            var result = converter.Decode(buffer);
            Assert.Equal(source.Id, result.Id);

            var token = new Token(generator, buffer);
            var child = token.Children.Single();
            Assert.Equal("id", child.Key);
            var actual = Encoding.UTF32.GetString(child.Value.Memory.Span);
            Assert.Equal(i.ToString(), actual);
        }
    }
}
