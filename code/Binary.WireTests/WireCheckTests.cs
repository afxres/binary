namespace Mikodev.Binary.WireTests;

using System.Buffers.Binary;
using System.Linq;
using Xunit;

// Set membership over the listed forms: a form that decodes correctly but is not
// listed must still fail.
public class WireCheckTests
{
    // A row with two listed forms, so the cases below can use one and reject the other.
    private const string Name = "vec_i32_5";

    private static WireCase Item => WireEncoder.Cases[Name];

    private static WireRow Row => WireFixture.Rows.Single(x => x.Name == Name);

    private static WireRow WithAuto(string auto) => Row with { Auto = auto };

    private static string Produced() => WireFixture.ToHex(WireEncoder.Encode(WireEncoder.Reflection, Item, auto: true));

    private static string Widened(string hex)
    {
        var payload = WireFixture.ParseHex(hex);
        var result = new byte[4 + payload.Length];
        BinaryPrimitives.WriteUInt32BigEndian(result, (uint)payload.Length | 0x8000_0000U);
        payload.CopyTo(result, 4);
        return WireFixture.ToHex(result);
    }

    [Fact(DisplayName = "The exact table row passes")]
    public void ExactRowPasses()
    {
        foreach (var generator in new[] { WireEncoder.Reflection, WireEncoder.Aot })
        {
            var failure = WireCheck.AutoMismatch(generator, Item, Row);
            Assert.True(failure is null, failure);
        }
    }

    [Fact(DisplayName = "Alternate forms of a row differ only in the header")]
    public void AlternateFormsShareThePayload()
    {
        var rows = WireFixture.Rows.Where(x => x.AutoForms.Count > 1).ToArray();
        Assert.NotEmpty(rows);
        foreach (var row in rows)
        {
            var payload = WireFixture.ParseHex(row.Bare);
            foreach (var form in row.AutoForms)
            {
                var bytes = WireFixture.ParseHex(form);
                Assert.True(bytes.Length > payload.Length, $"{row.Name}: {form} carries no header");
                Assert.Equal(WireFixture.ToHex(payload), WireFixture.ToHex(bytes[^payload.Length..]));
            }
        }
    }

    [Fact(DisplayName = "A row with its own header is decoded in both widths")]
    public void HeaderRowsAreDecodedInBothWidths()
    {
        foreach (var name in new[] { "string_empty", "string_20zh", "vec_i32_5", "named_person" })
        {
            var row = WireFixture.Rows.Single(x => x.Name == name);
            Assert.Equal(2, WireFixture.DecodeForms(row).Count);
        }

        // A tuple object writes its members' prefixes but no header of its own: neither it nor a fixed length converter can be widened.
        foreach (var name in new[] { "bool_true", "tuple_i32_str", "matrix3x2" })
        {
            var row = WireFixture.Rows.Single(x => x.Name == name);
            Assert.Equal(row.AutoForms, WireFixture.DecodeForms(row));
        }

        var widened = WireFixture.Rows.Count(x => WireFixture.DecodeForms(x).Count is 2);
        Assert.True(widened > 40, $"only {widened} rows exercise both header widths");
    }

    [Fact(DisplayName = "An unlisted header width is rejected: no decode fallback")]
    public void UnlistedWidthIsRejected()
    {
        var produced = Produced();
        var other = Row.AutoForms.First(x => x != produced);
        var failure = WireCheck.AutoMismatch(WireEncoder.Reflection, Item, WithAuto(other));
        Assert.NotNull(failure);
    }

    [Fact(DisplayName = "A changed payload is rejected")]
    public void ChangedPayloadIsRejected()
    {
        var payload = WireFixture.ParseHex(Row.Bare);
        payload[^1] ^= 1;
        var failure = WireCheck.AutoMismatch(WireEncoder.Reflection, Item, WithAuto(Widened(WireFixture.ToHex(payload))));
        Assert.NotNull(failure);
    }

    [Fact(DisplayName = "A header that overstates the payload is rejected")]
    public void OverstatedLengthIsRejected()
    {
        var payload = WireFixture.ParseHex(Row.Bare);
        var bytes = new byte[4 + payload.Length];
        BinaryPrimitives.WriteUInt32BigEndian(bytes, (uint)(payload.Length + 4) | 0x8000_0000U);
        payload.CopyTo(bytes, 4);
        var failure = WireCheck.AutoMismatch(WireEncoder.Reflection, Item, WithAuto(WireFixture.ToHex(bytes)));
        Assert.NotNull(failure);
    }

    [Fact(DisplayName = "Truncated bytes are rejected")]
    public void TruncatedPayloadIsRejected()
    {
        var payload = WireFixture.ParseHex(Row.Bare)[..^4];
        var failure = WireCheck.AutoMismatch(WireEncoder.Reflection, Item, WithAuto(Widened(WireFixture.ToHex(payload))));
        Assert.NotNull(failure);
    }
}
