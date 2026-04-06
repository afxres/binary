namespace Mikodev.Binary.Tests.Internal.Contexts;

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using Xunit;

public class ArrayPoolAllocatorTests
{
    private static IAllocator CreateInstance(ArrayPool<byte> arrays)
    {
        var type = typeof(IConverter).Assembly.GetTypes().Single(x => x.Name is "ArrayPoolAllocator");
        var instance = Activator.CreateInstance(type, args: [arrays]);
        return Assert.IsType<IAllocator>(instance, exactMatch: false);
    }

    private abstract class FakeAbstractArrayPool<T> : ArrayPool<T>
    {
        public List<int> RentedMinimumLengths { get; } = [];

        public List<T[]> RentedArrays { get; } = [];

        public List<T[]> ReturnedArrays { get; } = [];

        public abstract T[] CreateArray(int minimumLength);

        public sealed override T[] Rent(int minimumLength)
        {
            var result = CreateArray(minimumLength);
            RentedMinimumLengths.Add(minimumLength);
            RentedArrays.Add(result);
            return result;
        }

        public sealed override void Return(T[] array, bool clearArray = false)
        {
            Assert.False(clearArray);
            // always clear array, prevent reuse after return
            Array.Clear(array);
            ReturnedArrays.Add(array);
        }
    }

    private class TestArrayPool<T> : FakeAbstractArrayPool<T>
    {
        public override T[] CreateArray(int minimumLength)
        {
            return new T[minimumLength];
        }
    }

    private class TestInvalidSizeReturnsArrayPool<T>(int actualLength) : FakeAbstractArrayPool<T>
    {
        public override T[] CreateArray(int minimumLength)
        {
            return new T[actualLength];
        }
    }

    private class TestInvalidNullReturnsArrayPool<T>() : FakeAbstractArrayPool<T>
    {
        public override T[] CreateArray(int minimumLength)
        {
            return null!;
        }
    }

    [Fact(DisplayName = "Rent Return Test")]
    public void DoNothingTest()
    {
        var arrays = new TestArrayPool<byte>();
        var allocator = CreateInstance(arrays);
        ((IDisposable)allocator).Dispose();
        Assert.Empty(arrays.RentedArrays);
        Assert.Empty(arrays.ReturnedArrays);
    }

    [Theory(DisplayName = "Rent Return Test")]
    [InlineData([new int[] { 1, 4 }, new int[] { 64 * 1024 }])]
    [InlineData([new int[] { 32 * 1024, 128 * 1024 }, new int[] { 64 * 1024, 128 * 1024 }])]
    public void RentReturnTest(int[] wanted, int[] actual)
    {
        var arrays = new TestArrayPool<byte>();
        var allocator = CreateInstance(arrays);
        foreach (var i in wanted)
            _ = allocator.Resize(i);
        ((IDisposable)allocator).Dispose();
        Assert.Equal(actual, arrays.RentedArrays.Select(x => x.Length));
        Assert.Equal(actual, arrays.ReturnedArrays.Select(x => x.Length));
    }

    [Theory(DisplayName = "Invalid Size Returns Array Pool Rent Returns Test")]
    [InlineData(1, 0)]
    [InlineData(65536, 32768)]
    [InlineData(65536, 65535)]
    public void InvalidSizeReturnsArrayPoolRentReturnsTest(int wanted, int actual)
    {
        var arrays = new TestInvalidSizeReturnsArrayPool<byte>(actual);
        var allocator = CreateInstance(arrays);
        var error = Assert.Throws<InvalidOperationException>(() => allocator.Resize(wanted));
        var message = $"Invalid array pool implementation detected";
        Assert.Equal(message, error.Message);
    }

    [Theory(DisplayName = "Invalid Null Returns Array Pool Rent Returns Test")]
    [InlineData(1)]
    [InlineData(65536)]
    [InlineData(128 * 1024)]
    public void InvalidNullReturnsArrayPoolRentReturnsTest(int wanted)
    {
        var arrays = new TestInvalidNullReturnsArrayPool<byte>();
        var allocator = CreateInstance(arrays);
        var error = Assert.Throws<InvalidOperationException>(() => allocator.Resize(wanted));
        var message = $"Invalid array pool implementation detected";
        Assert.Equal(message, error.Message);
    }
}
