namespace Mikodev.Binary.Tests.Converters;

using Mikodev.Binary.Tests.Internal;
using System;
using System.Collections.Generic;
using System.Numerics;
using Xunit;

public class BigIntegerConverterInternalTests
{
    public static readonly IEnumerable<object?[]> DataNotEnoughSpace = new List<object?[]>
    {
        new object?[] { 0, default(BigInteger) },
        new object?[] { 0, new BigInteger() },
        new object?[] { 1, new BigInteger(1024) },
        new object?[] { 3, new BigInteger(int.MaxValue) },
    };

    [Theory(DisplayName = "Not Enough Space For Writing")]
    [MemberData(nameof(DataNotEnoughSpace))]
    public void NotEnoughSpace(int length, BigInteger data)
    {
        var functor = ReflectionExtensions.CreateDelegate<AllocatorWriter<BigInteger>>(x => x.FullName!.EndsWith(".BigIntegerConverter+Functions"), "Encode");
        var error = Assert.Throws<InvalidOperationException>(() => functor.Invoke(new Span<byte>(new byte[length]), data));
        Assert.Equal("Try write bytes failed.", error.Message);
    }
}
