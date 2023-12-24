namespace Mikodev.Binary.Tests.Contexts;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Xunit;

public class AllocatorUnderlyingTests
{
    private sealed class ManagedArrayAllocator : IAllocator
    {
        public readonly List<byte[]> AllocatedArrays = [];

        public ref byte Resize(int length)
        {
            var currentArray = new byte[length];
            var previousArray = this.AllocatedArrays.LastOrDefault().AsSpan();
            this.AllocatedArrays.Add(currentArray);
            previousArray.CopyTo(currentArray);
            return ref MemoryMarshal.GetArrayDataReference(currentArray);
        }
    }

    [Fact(DisplayName = "Allocator with Underlying Allocator Integration Test")]
    public void AllocatorWithUnderlyingAllocator()
    {
        var underlyingAllocator = new ManagedArrayAllocator();
        var allocator = new Allocator(underlyingAllocator);
        Assert.Equal(0, allocator.Length);
        Assert.Equal(0, allocator.Capacity);
        Assert.Equal(int.MaxValue, allocator.MaxCapacity);

        var random = new Random();
        var a = new byte[100];
        var b = new byte[300];
        var c = new byte[500];
        random.NextBytes(a);
        random.NextBytes(b);
        random.NextBytes(c);

        Allocator.Append(ref allocator, a);
        Assert.Equal(100, allocator.Length);
        Assert.Equal(256, allocator.Capacity);

        Allocator.Append(ref allocator, b);
        Assert.Equal(400, allocator.Length);
        Assert.Equal(512, allocator.Capacity);

        Allocator.Append(ref allocator, c);
        Assert.Equal(900, allocator.Length);
        Assert.Equal(1024, allocator.Capacity);

        var allocated = underlyingAllocator.AllocatedArrays;
        Assert.Equal(3, allocated.Count);

        Assert.Equal(a, allocated[0].AsSpan(0, 100));
        Assert.Equal([.. a, .. b], allocated[1].AsSpan(0, 400));
        Assert.Equal([.. a, .. b, .. c], allocated[2].AsSpan(0, 900));

        Assert.True(Unsafe.AreSame(ref MemoryMarshal.GetArrayDataReference(allocated[2]), ref MemoryMarshal.GetReference(allocator.AsSpan())));
        Assert.Equal([.. a, .. b, .. c], allocator.AsSpan());

        Assert.All(allocated[0].AsSpan(100).ToArray(), x => Assert.Equal(0, x));
        Assert.All(allocated[1].AsSpan(400).ToArray(), x => Assert.Equal(0, x));
        Assert.All(allocated[2].AsSpan(900).ToArray(), x => Assert.Equal(0, x));
    }

    [Fact(DisplayName = "Allocator with Underlying Allocator and Max Capacity Integration Test")]
    public void AllocatorWithUnderlyingAllocatorMaxCapacity()
    {
        var underlyingAllocator = new ManagedArrayAllocator();
        var allocator = new Allocator(underlyingAllocator, maxCapacity: 999);
        Assert.Equal(0, allocator.Length);
        Assert.Equal(0, allocator.Capacity);
        Assert.Equal(999, allocator.MaxCapacity);

        var random = new Random();
        var a = new byte[222];
        var b = new byte[666];
        random.NextBytes(a);
        random.NextBytes(b);

        Allocator.Append(ref allocator, a);
        Assert.Equal(222, allocator.Length);
        Assert.Equal(256, allocator.Capacity);

        Allocator.Append(ref allocator, b);
        Assert.Equal(888, allocator.Length);
        Assert.Equal(999, allocator.Capacity);

        var allocated = underlyingAllocator.AllocatedArrays;
        Assert.Equal(2, allocated.Count);

        Assert.Equal(a, allocated[0].AsSpan(0, 222));
        Assert.Equal([.. a, .. b], allocated[1].AsSpan(0, 888));

        Assert.True(Unsafe.AreSame(ref MemoryMarshal.GetArrayDataReference(allocated[1]), ref MemoryMarshal.GetReference(allocator.AsSpan())));
        Assert.Equal([.. a, .. b], allocator.AsSpan());

        Assert.All(allocated[0].AsSpan(222).ToArray(), x => Assert.Equal(0, x));
        Assert.All(allocated[1].AsSpan(888).ToArray(), x => Assert.Equal(0, x));
    }
}
