namespace Mikodev.Binary.Tests.Converters;

using Mikodev.Binary.Tests.Internal;
using System;
using System.Collections.Generic;
using System.Net;
using Xunit;

public class IPEndPointConverterInternalTests
{
    public static readonly IEnumerable<object?[]> DataNotEnoughSpace =
    [
        [5, new IPEndPoint(IPAddress.Loopback, IPEndPoint.MaxPort)],
        [17, new IPEndPoint(IPAddress.IPv6Loopback, IPEndPoint.MaxPort)],
        [5, new IPEndPoint(IPAddress.Any, IPEndPoint.MinPort)],
        [17, new IPEndPoint(IPAddress.IPv6Any, IPEndPoint.MinPort)],
    ];

    [Theory(DisplayName = "Not Enough Space For Writing")]
    [MemberData(nameof(DataNotEnoughSpace))]
    public void NotEnoughSpace(int length, IPEndPoint? data)
    {
        var functor = ReflectionExtensions.CreateDelegate<AllocatorWriter<IPEndPoint?>>(x => x.FullName?.EndsWith("IPEndPointConverter+Functions") is true, x => x.Name is "Append");
        var error = Assert.Throws<InvalidOperationException>(() => functor.Invoke(new Span<byte>(new byte[length]), data));
        Assert.Equal("Try write bytes failed.", error.Message);
    }

    public static IEnumerable<object?[]> DataMaxLength()
    {
        yield return new object?[] { null, 0 };
        yield return new object?[] { new IPEndPoint(IPAddress.Any, IPEndPoint.MinPort), 18 };
        yield return new object?[] { new IPEndPoint(IPAddress.IPv6Any, IPEndPoint.MinPort), 18 };
    }

    [Theory(DisplayName = "Max Length")]
    [MemberData(nameof(DataMaxLength))]
    public void MaxLengthTest(IPEndPoint? item, int maxLengthExpected)
    {
        var functor = ReflectionExtensions.CreateDelegate<Func<IPEndPoint?, int>>(x => x.FullName?.EndsWith("IPEndPointConverter+Functions") is true, x => x.Name is "Limits");
        var actual = functor.Invoke(item);
        Assert.Equal(maxLengthExpected, actual);
    }
}
