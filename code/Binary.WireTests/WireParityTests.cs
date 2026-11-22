namespace Mikodev.Binary.WireTests;

using System.Collections.Generic;
using System.Linq;
using Xunit;

public class WireParityTests
{
    public static IEnumerable<object[]> Rows => WireFixture.Rows.Select(x => new object[] { x.Name });

    // Does not read the table: a stale fixture must not be able to hide a split.
    [Theory(DisplayName = "Reflection and source generator agree")]
    [MemberData(nameof(Rows))]
    public void PathsAgree(string name)
    {
        var item = WireEncoder.Cases[name];
        foreach (var auto in new[] { false, true })
        {
            var reflection = WireFixture.ToHex(WireEncoder.Encode(WireEncoder.Reflection, item, auto));
            var generated = WireFixture.ToHex(WireEncoder.Encode(WireEncoder.Aot, item, auto));
            Assert.Equal(reflection, generated);
        }
    }

    [Fact(DisplayName = "EncodeAuto is never shorter than Encode")]
    public void AutoIsNeverShorter()
    {
        // Auto adds boundaries rather than always prepending a header, so it can never get shorter.
        foreach (var row in WireFixture.Rows)
        {
            var bare = WireFixture.ParseHex(row.Bare).Length;
            foreach (var form in row.AutoForms)
            {
                var auto = WireFixture.ParseHex(form).Length;
                Assert.True(auto >= bare, $"{row.Name}: auto {auto} bytes < bare {bare} bytes");
            }
        }
    }
}
