namespace Mikodev.Binary.Tests.Internal.Contexts;

using System;
using System.Buffers;
using System.Reflection;
using System.Runtime.CompilerServices;
using Xunit;

public class ArrayPoolAllocatorInternalTests
{
    private class FakeConverter<T>(AllocatorAction<T?> action) : Converter<T>
    {
        public override void Encode(ref Allocator allocator, T? item)
        {
            action.Invoke(ref allocator, item);
        }

        public override T Decode(in ReadOnlySpan<byte> span)
        {
            throw new NotSupportedException();
        }
    }

    private static void ArrayPoolAllocatorInternalDisposableTest(IAllocator? underlying)
    {
        Assert.NotNull(underlying);
        var arraysField = underlying.GetType().GetField("arrays", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(arraysField);
        Assert.Equal("ArrayPoolAllocator", underlying.GetType().Name);
        Assert.Equal(ArrayPool<byte>.Shared, arraysField.GetValue(underlying), ReferenceEquals);
        var error = Assert.Throws<ObjectDisposedException>(() => underlying.Resize(-1));
        Assert.Equal(underlying.GetType().FullName, error.ObjectName);
    }

    private static AllocatorAction<T> GetGetUnderlyingAllocatorAllocatorAction<T>(StrongBox<IAllocator> box)
    {
        return (ref allocator, _) => box.Value = Unsafe.As<Allocator, IAllocator>(ref allocator);
    }

    [Fact(DisplayName = "Allocator Invoke Internal Array Pool Allocator Usage Test")]
    public void AllocatorInvokeInternalArrayPoolAllocatorUsageTest()
    {
        var box = new StrongBox<IAllocator>();
        var buffer = Allocator.Invoke(null, GetGetUnderlyingAllocatorAllocatorAction<object?>(box));
        Assert.Empty(buffer);
        ArrayPoolAllocatorInternalDisposableTest(box.Value);
    }

    [Fact(DisplayName = "Converter Encode Internal Array Pool Allocator Usage Test")]
    public void ConverterEncodeInternalArrayPoolAllocatorUsageTest()
    {
        var box = new StrongBox<IAllocator>();
        var converter = new FakeConverter<object?>(GetGetUnderlyingAllocatorAllocatorAction<object?>(box));
        var buffer = converter.Encode(null);
        Assert.Empty(buffer);
        ArrayPoolAllocatorInternalDisposableTest(box.Value);
    }

    [Fact(DisplayName = "Converter Encode Brotli Internal Array Pool Allocator Usage Test")]
    public void ConverterEncodeBrotliInternalArrayPoolAllocatorUsageTest()
    {
        var box = new StrongBox<IAllocator>();
        var converter = new FakeConverter<object?>(GetGetUnderlyingAllocatorAllocatorAction<object?>(box));
        var buffer = converter.EncodeBrotli(null);
        Assert.NotEmpty(buffer);
        ArrayPoolAllocatorInternalDisposableTest(box.Value);
    }
}
