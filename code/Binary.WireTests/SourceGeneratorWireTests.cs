namespace Mikodev.Binary.WireTests;

using System.Collections.Generic;
using System.Linq;
using Xunit;

public class SourceGeneratorWireTests
{
    public static IEnumerable<object[]> Rows => WireFixture.Rows.Select(x => new object[] { x.Name });

    [Theory(DisplayName = "Source generator wire")]
    [MemberData(nameof(Rows))]
    public void SourceGeneratorWire(string name)
    {
        var row = WireFixture.Rows.Single(x => x.Name == name);
        var item = WireEncoder.Cases[name];

        var bare = WireFixture.ToHex(WireEncoder.Encode(WireEncoder.Aot, item, auto: false));
        Assert.Equal(row.Bare, bare);
    }

    [Theory(DisplayName = "Source generator wire (auto)")]
    [MemberData(nameof(Rows))]
    public void SourceGeneratorWireAuto(string name)
    {
        var row = WireFixture.Rows.Single(x => x.Name == name);
        var item = WireEncoder.Cases[name];

        var failure = WireCheck.AutoMismatch(WireEncoder.Aot, item, row);
        Assert.True(failure is null, failure);
    }

    [Fact(DisplayName = "Source generator produces a converter for every object type in the corpus")]
    public void EveryObjectTypeIsGenerated()
    {
        string[] objectTypes = ["named_tiny", "named_person", "named_mid", "named_with_opt_none", "named_with_opt_some", "tupleobj_two_int", "tupleobj_int_str", "enum_green", "vec_enum"];
        foreach (var name in objectTypes)
        {
            var item = WireEncoder.Cases[name];
            _ = WireEncoder.Aot.GetConverter(item.Type);
        }
    }
}
