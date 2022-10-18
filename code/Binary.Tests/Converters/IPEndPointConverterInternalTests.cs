namespace Mikodev.Binary.Tests.Converters;

using Mikodev.Binary.Tests.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using Xunit;

public class IPEndPointConverterInternalTests
{
    public static readonly IEnumerable<object?[]> DataNotEnoughSpace = new List<object?[]>
    {
        new object?[] { 5, new IPEndPoint(IPAddress.Loopback, IPEndPoint.MaxPort) },
        new object?[] { 17, new IPEndPoint(IPAddress.IPv6Loopback, IPEndPoint.MaxPort) },
        new object?[] { 5, new IPEndPoint(IPAddress.Any, IPEndPoint.MinPort) },
        new object?[] { 17, new IPEndPoint(IPAddress.IPv6Any, IPEndPoint.MinPort) },
    };

    [Theory(DisplayName = "Not Enough Space For Writing")]
    [MemberData(nameof(DataNotEnoughSpace))]
    public void NotEnoughSpace(int length, IPEndPoint? data)
    {
        var type = typeof(IConverter).Assembly.GetTypes().Single(x => x.Name is "IPEndPointConverter");
        var field = ReflectionExtensions.GetFieldNotNull(type, "EncodeFunction", BindingFlags.Static | BindingFlags.NonPublic);
        var functor = field.GetValue(null) as AllocatorWriter<IPEndPoint?>;
        Assert.NotNull(functor);

        var error = Assert.Throws<InvalidOperationException>(() => functor.Invoke(new Span<byte>(new byte[length]), data));
        Assert.Equal("Try write bytes failed.", error.Message);
    }
}
