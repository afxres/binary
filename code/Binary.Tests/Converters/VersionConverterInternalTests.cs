namespace Mikodev.Binary.Tests.Converters;

using Mikodev.Binary.Tests.Internal;
using System;
using System.Collections.Generic;
using Xunit;

public class VersionConverterInternalTests
{
    public static readonly IEnumerable<object?[]> DataNotEnoughSpace =
    [
        [0, new Version()],
        [11, new Version(1, 3)],
        [13, new Version(1, 2, 3)],
        [15, new Version(2, 4, 8, 16)],
    ];

    [Theory(DisplayName = "Not Enough Space For Writing")]
    [MemberData(nameof(DataNotEnoughSpace))]
    public void NotEnoughSpace(int length, Version? data)
    {
        var functor = ReflectionExtensions.CreateDelegate<AllocatorWriter<Version?>>(x => x.FullName?.EndsWith("VersionConverter+Functions") is true, x => x.Name is "Append");
        var error = Assert.Throws<InvalidOperationException>(() => functor.Invoke(new Span<byte>(new byte[length]), data));
        Assert.Equal("Try write bytes failed.", error.Message);
    }

    [Fact(DisplayName = "Append Null Value")]
    public void AppendNullValue()
    {
        var functor = ReflectionExtensions.CreateDelegate<AllocatorWriter<Version?>>(x => x.FullName?.EndsWith("VersionConverter+Functions") is true, x => x.Name is "Append");
        var actual = functor.Invoke(new Span<byte>(), null);
        Assert.Equal(0, actual);
    }

    public static IEnumerable<object?[]> DataMaxLength()
    {
        yield return new object?[] { null, 0 };
        yield return new object?[] { new Version(), 16 };
        yield return new object?[] { new Version(9, 8), 16 };
        yield return new object?[] { new Version(9, 8, 7), 16 };
        yield return new object?[] { new Version(9, 8, 7, 6), 16 };
    }

    [Theory(DisplayName = "Max Length")]
    [MemberData(nameof(DataMaxLength))]
    public void MaxLengthTest(Version? item, int maxLengthExpected)
    {
        var functor = ReflectionExtensions.CreateDelegate<Func<Version?, int>>(x => x.FullName?.EndsWith("VersionConverter+Functions") is true, x => x.Name is "Limits");
        var actual = functor.Invoke(item);
        Assert.Equal(maxLengthExpected, actual);
    }
}
