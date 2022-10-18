namespace Mikodev.Binary.Tests.Converters;

using Mikodev.Binary.Tests.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
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
        var type = typeof(IConverter).Assembly.GetTypes().Single(x => x.Name is "BigIntegerConverter");
        var field = ReflectionExtensions.GetFieldNotNull(type, "EncodeFunction", BindingFlags.Static | BindingFlags.NonPublic);
        var functor = field.GetValue(null) as AllocatorWriter<BigInteger>;
        Assert.NotNull(functor);

        var error = Assert.Throws<InvalidOperationException>(() => functor.Invoke(new Span<byte>(new byte[length]), data));
        Assert.Equal("Try write bytes failed.", error.Message);
    }
}
