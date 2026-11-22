namespace Mikodev.Binary.WireTests;

using System;
using System.Linq;
using Xunit;

public class TupleShapeTests
{
    [Fact(DisplayName = "Wire: a nested tuple is the same bytes as the flat one")]
    public void NestedTupleEqualsFlatTuple()
    {
        // Same values as Rust `wire.rs:test_tuple_flatten_equals_nested`.
        var nested = ((1712L, 500), ("amy", 7), "login");
        var flat = (1712L, 500, "amy", 7, "login");
        const string Expected = "b006000000000000" + "f4010000" + "03616d79" + "07000000" + "6c6f67696e";

        Assert.Equal(Expected, Hex(nested));
        Assert.Equal(Expected, Hex(flat));
    }

    [Fact(DisplayName = "Wire: a ten member tuple is its members back to back")]
    public void LongTupleIsFlat()
    {
        var ten = ((sbyte)-128, short.MinValue, int.MaxValue, long.MinValue, Int128.MinValue, UInt128.MaxValue, -0.0D, (Half)1.5F, 3.5F, 1.5M);
        var split = (((sbyte)-128, short.MinValue, int.MaxValue, long.MinValue, Int128.MinValue, UInt128.MaxValue, -0.0D), ((Half)1.5F, 3.5F, 1.5M));

        var row = WireFixture.Rows.Single(x => x.Name == "tuple_ten_mixed");
        Assert.Equal(77, WireEncoder.Reflection.GetConverter(ten.GetType()).Length);
        Assert.Equal(row.Bare, Hex(ten));
        Assert.Equal(row.Bare, Hex(split));
    }

    // Reflection only: the AOT builder has no converter for a tuple shape that was not listed in the generator context.
    private static string Hex(object value)
    {
        return WireFixture.ToHex(WireEncoder.EncodeValue(WireEncoder.Reflection, value.GetType(), value, auto: false));
    }
}
