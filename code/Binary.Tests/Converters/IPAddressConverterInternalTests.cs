namespace Mikodev.Binary.Tests.Converters;

using Mikodev.Binary.Tests.Internal;
using System;
using System.Collections.Generic;
using System.Net;
using Xunit;

public class IPAddressConverterInternalTests
{
    public static readonly IEnumerable<object?[]> DataNotEnoughSpace = new List<object?[]>
    {
        new object?[] { 3, IPAddress.Loopback },
        new object?[] { 15, IPAddress.IPv6Loopback },
    };

    [Theory(DisplayName = "Not Enough Space For Writing")]
    [MemberData(nameof(DataNotEnoughSpace))]
    public void NotEnoughSpace(int length, IPAddress? data)
    {
        var functor = ReflectionExtensions.CreateDelegate<AllocatorWriter<IPAddress?>>(x => x.FullName!.EndsWith(".IPAddressConverter+Functions"), "Encode");
        var error = Assert.Throws<InvalidOperationException>(() => functor.Invoke(new Span<byte>(new byte[length]), data));
        Assert.Equal("Try write bytes failed.", error.Message);
    }
}
