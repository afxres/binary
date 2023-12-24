namespace Mikodev.Binary.Tests.Contexts;

using Mikodev.Binary.Tests.Internal;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Xunit;

public class AllocatorTests
{
    private delegate int Anchor(ref Allocator allocator, int length);

    private delegate void Resize(ref Allocator allocator, int expand);

    private delegate void FinishAnchor(ref Allocator allocator, int anchor);

    [Fact(DisplayName = "Resize Capacity (hack)")]
    public unsafe void ResizeCapacity()
    {
        var buffer = new byte[256];
        buffer.AsSpan().Fill(0x80);
        var allocator = new Allocator(buffer);
        Assert.Equal(0, allocator.Length);
        Assert.Equal(256, allocator.Capacity);
        Allocator.Append(ref allocator, Enumerable.Repeat((byte)0x7F, 128).ToArray());
        Assert.Equal(128, allocator.Length);
        Assert.Equal(256, allocator.Capacity);
        var source = allocator.AsSpan();
        fixed (byte* srcptr = &MemoryMarshal.GetReference(source))
        {
            for (var i = 0; i < 128; i++)
                Assert.Equal((byte)0x7F, srcptr[i]);
            for (var i = 128; i < 256; i++)
                Assert.Equal((byte)0x80, srcptr[i]);
        }
        Allocator.Append(ref allocator, 512, default(object), (a, b) => { });
        Assert.Equal(640, allocator.Length);
        Assert.Equal(1024, allocator.Capacity);
        var target = allocator.AsSpan();
        var head = target.Slice(0, 128);
        var tail = target.Slice(128);
        Assert.Equal(128, head.Length);
        Assert.Equal(512, tail.Length);
        Assert.All(head.ToArray(), x => Assert.Equal((byte)0x7F, x));
        Assert.All(tail.ToArray(), x => Assert.Equal((byte)0x00, x));
    }

    [Theory(DisplayName = "Resize Capacity (hack, invalid)")]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-16)]
    [InlineData(int.MinValue)]
    public void ResizeCapacityInvalid(int expand)
    {
        var methodInfo = typeof(Allocator).GetMethodNotNull("Resize", BindingFlags.Static | BindingFlags.NonPublic);
        var method = (Resize)Delegate.CreateDelegate(typeof(Resize), methodInfo);
        var error = Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            var allocator = new Allocator();
            method.Invoke(ref allocator, expand);
        });
        Assert.Equal("length", error.ParamName);
    }

    [Fact(DisplayName = "Ensure Capacity (hack, zero)")]
    public void EnsureCapacityZero()
    {
        var capacity = 14;
        var buffer = new byte[capacity];
        var allocator = new Allocator(buffer);
        Assert.Equal(capacity, allocator.Capacity);
        Allocator.Ensure(ref allocator, 0);
        Assert.Equal(capacity, allocator.Capacity);
        Assert.Equal(0, allocator.Length);
    }

    [Theory(DisplayName = "Ensure Capacity (hack, invalid)")]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void EnsureCapacityInvalid(int expand)
    {
        var error = Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            var allocator = new Allocator();
            Allocator.Ensure(ref allocator, expand);
        });
        Assert.Equal("length", error.ParamName);
    }

    [Theory(DisplayName = "Ensure Capacity (hack, exactly)")]
    [InlineData(0, 4, 4)]
    [InlineData(2, 8, 10)]
    public void EnsureCapacityExactly(int offset, int expand, int capacity)
    {
        Assert.Equal(capacity, offset + expand);
        var buffer = new byte[capacity];
        var allocator = new Allocator(buffer);
        Assert.Equal(capacity, allocator.Capacity);
        Assert.Equal(0, allocator.Length);
        Allocator.Append(ref allocator, offset, 0, (a, b) => { });
        Assert.Equal(offset, allocator.Length);
        Allocator.Ensure(ref allocator, expand);
        Assert.Equal(capacity, allocator.Capacity);
        Assert.Equal(offset, allocator.Length);
    }

    [Fact(DisplayName = "Anchor (hack, zero)")]
    public void AnchorZero()
    {
        var methodInfo = typeof(Allocator).GetMethodNotNull("Anchor", BindingFlags.Static | BindingFlags.NonPublic);
        var anchor = (Anchor)Delegate.CreateDelegate(typeof(Anchor), methodInfo);
        var buffer = new byte[25];
        var allocator = new Allocator(buffer);
        var result = anchor.Invoke(ref allocator, 0);
        Assert.Equal(0, result);
        Assert.Equal(0, allocator.Length);
        Assert.Equal(buffer.Length, allocator.Capacity);
    }

    [Theory(DisplayName = "Anchor Then Append Length Prefix")]
    [InlineData(0, 4, 1)]
    [InlineData(1, 5, 5)]
    [InlineData(1, 6, 5)]
    [InlineData(1, 7, 5)]
    [InlineData(1, 8, 5)]
    [InlineData(1, 9, 5)]
    [InlineData(1, 10, 5)]
    [InlineData(1, 11, 5)]
    [InlineData(1, 12, 2)]
    [InlineData(1, 13, 2)]
    [InlineData(15, 19, 19)]
    [InlineData(15, 20, 16)]
    [InlineData(16, 20, 17)]
    [InlineData(17, 21, 21)]
    public void AnchorAppend(int length, int allocatorCapacity, int allocatorLength)
    {
        var anchorMethod = (Anchor)Delegate.CreateDelegate(typeof(Anchor), typeof(Allocator).GetMethodNotNull("Anchor", BindingFlags.Static | BindingFlags.NonPublic));
        var appendMethod = (FinishAnchor)Delegate.CreateDelegate(typeof(FinishAnchor), typeof(Allocator).GetMethodNotNull("FinishAnchor", BindingFlags.Static | BindingFlags.NonPublic));
        var buffer = new byte[length];
        var random = new Random();
        random.NextBytes(buffer);
        var allocator = new Allocator(new Span<byte>(new byte[allocatorCapacity]), maxCapacity: allocatorCapacity);
        var anchor = anchorMethod.Invoke(ref allocator, 4);
        Assert.Equal(4, allocator.Length);
        Assert.Equal(allocatorCapacity, allocator.Capacity);
        Allocator.Append(ref allocator, buffer);
        appendMethod.Invoke(ref allocator, anchor);
        Assert.Equal(allocatorLength, allocator.Length);
        Assert.Equal(allocatorCapacity, allocator.Capacity);
        var span = allocator.AsSpan();
        var number = Converter.Decode(ref span);
        Assert.Equal(length, number);
        Assert.Equal(length, span.Length);
        Assert.Equal(buffer, span.ToArray());
    }

    [Theory(DisplayName = "Append Length Prefix Invalid")]
    [InlineData(0, 0)]
    [InlineData(0, 3)]
    [InlineData(-1, 0)]
    [InlineData(-1, 4)]
    [InlineData(16, 15)]
    [InlineData(16, 19)]
    [InlineData(-32, 0)]
    [InlineData(-32, 4)]
    [InlineData(int.MaxValue, 0)]
    [InlineData(int.MaxValue, 16)]
    public void AnchorAppendInvalid(int anchor, int allocatorLength)
    {
        var appendMethod = (FinishAnchor)Delegate.CreateDelegate(typeof(FinishAnchor), typeof(Allocator).GetMethodNotNull("FinishAnchor", BindingFlags.Static | BindingFlags.NonPublic));
        var error = Assert.Throws<InvalidOperationException>(() =>
        {
            var allocator = new Allocator();
            Allocator.Append(ref allocator, allocatorLength, 0, (_a, _b) => { });
            Assert.Equal(allocatorLength, allocator.Length);
            appendMethod.Invoke(ref allocator, anchor);
        });
        var message = "Allocator has been modified unexpectedly!";
        Assert.Equal(message, error.Message);
    }

    [Fact(DisplayName = "Anchor Then Append Length Prefix (for loop all conditions)")]
    public void AnchorAppendRange()
    {
        const int Limits = 16;
        var anchorMethod = (Anchor)Delegate.CreateDelegate(typeof(Anchor), typeof(Allocator).GetMethodNotNull("Anchor", BindingFlags.Static | BindingFlags.NonPublic));
        var appendMethod = (FinishAnchor)Delegate.CreateDelegate(typeof(FinishAnchor), typeof(Allocator).GetMethodNotNull("FinishAnchor", BindingFlags.Static | BindingFlags.NonPublic));
        var random = new Random();
        for (var length = 0; length <= 64; length++)
        {
            for (var additional = 0; additional <= 16; additional++)
            {
                var capacity = length + additional + 4;
                var buffer = new byte[length];
                random.NextBytes(buffer);
                var allocator = new Allocator(new byte[capacity], maxCapacity: capacity);
                var anchor = anchorMethod.Invoke(ref allocator, 4);
                Assert.Equal(4, allocator.Length);
                Allocator.Append(ref allocator, buffer);
                Assert.Equal(length + 4, allocator.Length);
                appendMethod.Invoke(ref allocator, anchor);
                var lengthAlign8 = (length % 8) == 0 ? length : ((length >> 3) + 1) << 3;
                Assert.True(lengthAlign8 >= length && lengthAlign8 % 8 == 0);
                Assert.Equal(lengthAlign8 - length, (-length) & 7);
                var actualReduce = length <= Limits && capacity - 4 - lengthAlign8 >= 0;
                Assert.Equal(length + (actualReduce ? 1 : 4), allocator.Length);
                var span = allocator.AsSpan();
                var actualLength = Converter.Decode(ref span);
                Assert.Equal(length, actualLength);
                Assert.Equal(buffer, span.ToArray());
            }
        }
    }

    [Theory(DisplayName = "Append Byte Test")]
    [InlineData((byte)0, 0, 0, 256)]
    [InlineData((byte)1, 100, 256, 256)]
    [InlineData((byte)2, 255, 256, 256)]
    [InlineData((byte)3, 256, 256, 512)]
    [InlineData((byte)47, 513, 1024, 1024)]
    [InlineData((byte)73, 2048, 2048, 4096)]
    [InlineData((byte)255, 767, 768, 768)]
    public void AppendByteTest(byte data, int length, int capacity, int newCapacity)
    {
        var buffer = new byte[capacity];
        var allocator = new Allocator(buffer);
        Allocator.Append(ref allocator, length, 0, (_, _) => { });
        Assert.Equal(length, allocator.Length);
        Assert.Equal(capacity, allocator.Capacity);
        Allocator.Append(ref allocator, data);
        Assert.Equal(newCapacity, allocator.Capacity);
        Assert.Equal(length + 1, allocator.Length);
        Assert.Equal(data, allocator.AsSpan()[length]);
    }

    [Theory(DisplayName = "Append Byte Capacity Limited Test")]
    [InlineData((byte)0, 0, 0)]
    [InlineData((byte)1, 128, 128)]
    [InlineData((byte)2, 256, 256)]
    [InlineData((byte)255, 8192, 8192)]
    public void AppendByteCapacityLimitedTest(byte data, int length, int capacity)
    {
        var error = Assert.Throws<ArgumentException>(() =>
        {
            var allocator = new Allocator([], maxCapacity: capacity);
            Allocator.Append(ref allocator, length, 0, (_, _) => { });
            Assert.Equal(length, allocator.Length);
            Assert.Equal(capacity, allocator.Capacity);
            Assert.Equal(capacity, allocator.MaxCapacity);
            Allocator.Append(ref allocator, data);
        });
        Assert.Null(error.ParamName);
        Assert.Equal("Maximum capacity has been reached.", error.Message);
    }
}
