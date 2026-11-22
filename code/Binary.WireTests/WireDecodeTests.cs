namespace Mikodev.Binary.WireTests;

using System.Collections.Generic;
using System.Linq;
using Xunit;

// The decode direction: the table bytes are the input, the corpus value is the
// oracle. Every header width is fed to `DecodeAuto`, not just the ones the table
// lists, because a reader that only handles its own width is a split reader.
public class WireDecodeTests
{
    public static IEnumerable<object[]> Rows => WireFixture.Rows.Select(x => new object[] { x.Name });

    private static void AssertDecode(IGenerator generator, WireCase item, WireRow row)
    {
        var bare = WireFixture.ParseHex(row.Bare);
        var bareValue = WireEncoder.Decode(generator, item, bare, auto: false);
        Assert.True(bareValue.Consumed == bare.Length, $"{row.Name} (Decode): consumed {bareValue.Consumed} of {bare.Length}");
        Assert.True(WireCheck.Compare(item.Value, bareValue.Value) is true, $"{row.Name} (Decode): decoded value differs from the corpus value");

        foreach (var form in WireFixture.DecodeForms(row))
        {
            var auto = WireFixture.ParseHex(form);
            var autoValue = WireEncoder.Decode(generator, item, auto, auto: true);
            Assert.True(autoValue.Consumed == auto.Length, $"{row.Name} (DecodeAuto {form}): consumed {autoValue.Consumed} of {auto.Length}");
            Assert.True(WireCheck.Compare(item.Value, autoValue.Value) is true, $"{row.Name} (DecodeAuto {form}): decoded value differs from the corpus value");
        }
    }

    [Theory(DisplayName = "Reflection decode")]
    [MemberData(nameof(Rows))]
    public void ReflectionDecode(string name)
    {
        var row = WireFixture.Rows.Single(x => x.Name == name);
        var item = WireEncoder.Cases[name];
        AssertDecode(WireEncoder.Reflection, item, row);
    }

    [Theory(DisplayName = "Source generator decode")]
    [MemberData(nameof(Rows))]
    public void SourceGeneratorDecode(string name)
    {
        var row = WireFixture.Rows.Single(x => x.Name == name);
        var item = WireEncoder.Cases[name];
        AssertDecode(WireEncoder.Aot, item, row);
    }
}
