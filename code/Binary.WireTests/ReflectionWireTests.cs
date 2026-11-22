namespace Mikodev.Binary.WireTests;

using System.Collections.Generic;
using System.Linq;
using Xunit;

public class ReflectionWireTests
{
    public static IEnumerable<object[]> Rows => WireFixture.Rows.Select(x => new object[] { x.Name });

    [Theory(DisplayName = "Reflection wire")]
    [MemberData(nameof(Rows))]
    public void ReflectionWire(string name)
    {
        var row = WireFixture.Rows.Single(x => x.Name == name);
        var item = WireEncoder.Cases[name];

        var bare = WireFixture.ToHex(WireEncoder.Encode(WireEncoder.Reflection, item, auto: false));
        Assert.Equal(row.Bare, bare);
    }

    [Theory(DisplayName = "Reflection wire (auto)")]
    [MemberData(nameof(Rows))]
    public void ReflectionWireAuto(string name)
    {
        var row = WireFixture.Rows.Single(x => x.Name == name);
        var item = WireEncoder.Cases[name];

        var failure = WireCheck.AutoMismatch(WireEncoder.Reflection, item, row);
        Assert.True(failure is null, failure);
    }

    [Fact(DisplayName = "Reflection converter length agrees with the payload width")]
    public void ReflectionLengthMatchesPayload()
    {
        // `Length == 0` means "variable", not "zero bytes".
        foreach (var (name, item) in WireEncoder.Cases)
        {
            var converter = WireEncoder.Reflection.GetConverter(item.Type);
            if (converter.Length is 0)
                continue;

            var bare = WireFixture.ToHex(WireEncoder.Encode(WireEncoder.Reflection, item, auto: false));
            var auto = WireFixture.ToHex(WireEncoder.Encode(WireEncoder.Reflection, item, auto: true));
            Assert.True(bare.Length == converter.Length * 2, $"{name}: {bare.Length / 2} bytes, Length {converter.Length}");
            Assert.Equal(bare, auto);
        }
    }
}
