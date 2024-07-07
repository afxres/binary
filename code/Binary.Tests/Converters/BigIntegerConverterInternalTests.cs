namespace Mikodev.Binary.Tests.Converters;

using Mikodev.Binary.Tests.Internal;
using System;
using System.Collections.Generic;
using System.Numerics;
using Xunit;

public class BigIntegerConverterInternalTests
{
    public static readonly IEnumerable<object?[]> DataNotEnoughSpace =
    [
        [0, new BigInteger()],
        [1, new BigInteger(1024)],
        [3, new BigInteger(int.MaxValue)],
    ];

    [Theory(DisplayName = "Not Enough Space For Writing")]
    [MemberData(nameof(DataNotEnoughSpace))]
    public void NotEnoughSpace(int length, BigInteger data)
    {
        var functor = ReflectionExtensions.CreateDelegate<AllocatorWriter<BigInteger>>(x => x.FullName?.EndsWith("BigIntegerConverter+Functions") is true, x => x.Name is "Append");
        var error = Assert.Throws<InvalidOperationException>(() => functor.Invoke(new Span<byte>(new byte[length]), data));
        Assert.Equal("Try write bytes failed.", error.Message);
    }

    public static IEnumerable<object?[]> DataMaxLength()
    {
        yield return new object[] { new BigInteger() };
        yield return new object[] { new BigInteger(ulong.MaxValue) };
    }

    [Theory(DisplayName = "Max Length")]
    [MemberData(nameof(DataMaxLength))]
    public void MaxLengthTest(BigInteger data)
    {
        var functor = ReflectionExtensions.CreateDelegate<Func<BigInteger, int>>(x => x.FullName?.EndsWith("BigIntegerConverter+Functions") is true, x => x.Name is "Limits");
        var expected = data.GetByteCount();
        var actual = functor.Invoke(data);
        Assert.Equal(expected, actual);
    }
}
