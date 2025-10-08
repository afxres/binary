namespace Mikodev.Binary.Tests.Converters;

using Mikodev.Binary.Tests.Internal;
using System;
using System.Collections.Generic;
using System.Net;
using Xunit;

public class IPAddressConverterInternalTests
{
    public static readonly IEnumerable<object?[]> DataNotEnoughSpace =
    [
        [3, IPAddress.Loopback],
        [15, IPAddress.IPv6Loopback],
    ];

    [Theory(DisplayName = "Not Enough Space For Writing")]
    [MemberData(nameof(DataNotEnoughSpace))]
    public void NotEnoughSpace(int length, IPAddress? data)
    {
        var functor = ReflectionExtensions.CreateDelegate<AllocatorWriter<IPAddress?>>(x => x.FullName?.EndsWith("IPAddressConverter+Functions") is true, x => x.Name is "Append");
        var error = Assert.Throws<InvalidOperationException>(() => functor.Invoke(new Span<byte>(new byte[length]), data));
        Assert.Equal("Try write bytes failed.", error.Message);
    }

    [Fact(DisplayName = "Append Null Value")]
    public void AppendNullValue()
    {
        var functor = ReflectionExtensions.CreateDelegate<AllocatorWriter<IPAddress?>>(x => x.FullName?.EndsWith("IPAddressConverter+Functions") is true, x => x.Name is "Append");
        var actual = functor.Invoke(new Span<byte>(), null);
        Assert.Equal(0, actual);
    }

    public static IEnumerable<object?[]> DataMaxLength()
    {
        yield return [null, 0];
        yield return [IPAddress.Any, 16];
        yield return [IPAddress.IPv6Any, 16];
    }

    [Theory(DisplayName = "Max Length")]
    [MemberData(nameof(DataMaxLength))]
    public void MaxLengthTest(IPAddress? item, int maxLengthExpected)
    {
        var functor = ReflectionExtensions.CreateDelegate<Func<IPAddress?, int>>(x => x.FullName?.EndsWith("IPAddressConverter+Functions") is true, x => x.Name is "Limits");
        var actual = functor.Invoke(item);
        Assert.Equal(maxLengthExpected, actual);
    }
}
