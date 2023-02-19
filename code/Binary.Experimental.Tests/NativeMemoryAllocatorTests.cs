namespace Mikodev.Binary.Experimental.Tests;

using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

public class NativeMemoryAllocatorTests
{
    [Fact(DisplayName = "Dispose Multiple Times")]
    public void DisposeMultipleTimes()
    {
        var allocator = new NativeMemoryAllocator();
        allocator.Dispose();
        allocator.Dispose();
        allocator.Dispose();
    }

    [Fact(DisplayName = "Dispose Then Allocate")]
    public void DisposeThenAllocate()
    {
        var allocator = new NativeMemoryAllocator();
        allocator.Dispose();
        var error = Assert.Throws<ObjectDisposedException>(() => allocator.Allocate(1));
        Assert.NotNull(error.ObjectName);
    }

    public static IEnumerable<object[]> LargeArrayData()
    {
        var alpha = Enumerable.Range(0, 65536).ToList();
        var bravo = Enumerable.Range(0, 32768).Select(x => x.ToString()).ToList();
        yield return new object[] { alpha };
        yield return new object[] { bravo };
    }

    [Theory(DisplayName = "Encode Large Array")]
    [MemberData(nameof(LargeArrayData))]
    public void EncodeLargeArray<T>(T data)
    {
        var generator = Generator.CreateDefault();
        var converter = generator.GetConverter<T>();

        using var native = new NativeMemoryAllocator();
        var allocator = new Allocator(native);
        converter.Encode(ref allocator, data);
        var result = converter.Decode(allocator.AsSpan());
        Assert.Equal(data, result);
    }
}
