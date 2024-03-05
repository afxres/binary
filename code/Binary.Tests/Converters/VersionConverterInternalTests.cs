namespace Mikodev.Binary.Tests.Converters;

using Mikodev.Binary.Tests.Internal;
using System;
using System.Collections.Generic;
using Xunit;

public class VersionConverterInternalTests
{
    public static readonly IEnumerable<object?[]> DataNotEnoughSpace =
    [
        [0, null],
        [15, null],
        [0, new Version()],
        [11, new Version(1, 3)],
        [13, new Version(1, 2, 3)],
        [15, new Version(2, 4, 8, 16)],
    ];

    [Theory(DisplayName = "Not Enough Space For Writing")]
    [MemberData(nameof(DataNotEnoughSpace))]
    public void NotEnoughSpace(int length, Version? data)
    {
        var functor = ReflectionExtensions.CreateDelegate<AllocatorWriter<Version?>>(x => x.Name is "VersionConverter", x => x.Name.Contains("Invoke"));
        var error = Assert.Throws<InvalidOperationException>(() => functor.Invoke(new Span<byte>(new byte[length]), data));
        Assert.Equal("Try write bytes failed.", error.Message);
    }
}
