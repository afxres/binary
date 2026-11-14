#if NET11_0_OR_GREATER

namespace Mikodev.Binary.Tests.Converters.Primitive;

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using Xunit;

public class GenericComplexWireTests
{
    public static IEnumerable<object[]> WireData =>
    [
        [0x04, new Complex<Half>((Half)1.5F, (Half)(-2.25F)), "003E80C0"],
        [0x04, new Complex<BFloat16>(BFloat16.Parse("1.5"), BFloat16.Parse("-2.25")), "C03F10C0"],
        [0x08, new Complex<float>(1.5F, -2.25F), "0000C03F000010C0"],
        [0x08, new Complex<Decimal32>(Decimal32.Parse("1.5"), Decimal32.Parse("-2.25")), "0F000032E10080B1"],
        [0x10, new Complex<double>(1.5, -2.25), "000000000000F83F00000000000002C0"],
        [0x10, new Complex<Decimal64>(Decimal64.Parse("1.5"), Decimal64.Parse("-2.25")), "0F0000000000A031E1000000000080B1"],
        [0x20, new Complex<Decimal128>(Decimal128.Parse("1.5"), Decimal128.Parse("-2.25")), "0F000000000000000000000000003E30E1000000000000000000000000003CB0"],
    ];

    [Theory(DisplayName = "Wire")]
    [MemberData(nameof(WireData))]
    public void Wire<T>(int length, T item, string expected) where T : unmanaged
    {
        var generator = Generator.CreateDefault();
        var converter = generator.GetConverter<T>();
        Assert.Equal("RepeatLittleEndianConverter`2", converter.GetType().Name);
        Assert.Equal(length, converter.Length);
        Assert.Equal(length, Unsafe.SizeOf<T>());

        var buffer = converter.Encode(item);
        Assert.Equal(expected, Convert.ToHexString(buffer));
        Assert.Equal(item, converter.Decode(buffer));
    }

    [Fact(DisplayName = "Wire Compatible With Non Generic Complex")]
    public void WireCompatibleWithNonGenericComplex()
    {
        var generator = Generator.CreateDefault();
        var expected = generator.Encode(new Complex(1.5, -2.25));
        var actual = generator.Encode(new Complex<double>(1.5, -2.25));
        Assert.Equal(expected, actual);
    }
}

#endif
