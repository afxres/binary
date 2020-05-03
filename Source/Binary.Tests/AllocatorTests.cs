using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Xunit;

namespace Mikodev.Binary.Tests
{
    public class AllocatorTests
    {
        private delegate int Anchor(ref Allocator allocator, int length);

        private delegate int Ensure(ref Allocator allocator, int expand);

        private delegate void Expand(ref Allocator allocator, int expand);

        private delegate void AppendLengthPrefix(ref Allocator allocator, int anchor, bool reduce);

        private static readonly string outofrange = new ArgumentOutOfRangeException().Message;

        [Theory(DisplayName = "Fake Length Prefix Anchor (hack)")]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(8)]
        [InlineData(1020)]
        public unsafe void FakeAnchor(int offset)
        {
            const int Limits = 1024;

            byte[] Test()
            {
                var anchor = new AllocatorAnchor();
                ((int*)&anchor)[0] = offset;
                ((int*)&anchor)[1] = sizeof(int);
                Assert.Equal($"AllocatorAnchor(Offset: {offset}, Length: 4)", anchor.ToString());
                var allocator = new Allocator();
                AllocatorHelper.Append(ref allocator, Limits, 0, (a, b) => { });
                Assert.Equal(Limits, allocator.Length);
                AllocatorHelper.AppendLengthPrefix(ref allocator, anchor);
                return allocator.AsSpan().ToArray();
            }

            var buffer = Test();
            var span = new ReadOnlySpan<byte>(buffer);
            var numberSpan = span.Slice(offset);
            var number = PrimitiveHelper.DecodeNumber(ref numberSpan);
            Assert.Equal(Limits - offset - sizeof(int), number);
        }

        [Theory(DisplayName = "Fake Length Prefix Anchor (hack, invalid length)")]
        [InlineData(0, -1)]
        [InlineData(1, 0)]
        [InlineData(8, 3)]
        [InlineData(1020, 5)]
        public unsafe void FakeAnchorLength(int offset, int length)
        {
            const int Limits = 1024;

            void Test()
            {
                var anchor = new AllocatorAnchor();
                ((int*)&anchor)[0] = offset;
                ((int*)&anchor)[1] = length;
                Assert.Equal($"AllocatorAnchor(Offset: {offset}, Length: {length})", anchor.ToString());
                var allocator = new Allocator();
                AllocatorHelper.Append(ref allocator, Limits, 0, (a, b) => { });
                Assert.Equal(Limits, allocator.Length);
                AllocatorHelper.AppendLengthPrefix(ref allocator, anchor);
            }

            var parameter = typeof(AllocatorHelper).GetMethod(nameof(AllocatorHelper.AppendLengthPrefix)).GetParameters().Last();
            var error = Assert.Throws<ArgumentOutOfRangeException>(() => Test());
            Assert.Contains(outofrange, error.Message);
            Assert.Equal("anchor", error.ParamName);
            Assert.Equal("anchor", parameter.Name);
        }

        [Theory(DisplayName = "Fake Length Prefix Anchor (hack, invalid offset)")]
        [InlineData(int.MinValue)]
        [InlineData(-1)]
        [InlineData(768)]
        [InlineData(int.MaxValue)]
        public unsafe void FakeAnchorRange(int offset)
        {
            const int Limits = 512;

            void Test()
            {
                var anchor = new AllocatorAnchor();
                ((int*)&anchor)[0] = offset;
                ((int*)&anchor)[1] = sizeof(int);
                Assert.Equal($"AllocatorAnchor(Offset: {offset}, Length: 4)", anchor.ToString());
                var allocator = new Allocator();
                AllocatorHelper.Append(ref allocator, new byte[Limits]);
                Assert.Equal(Limits, allocator.Length);
                Assert.Equal(1024, allocator.Capacity);
                AllocatorHelper.AppendLengthPrefix(ref allocator, anchor);
            }

            var error = Assert.Throws<ArgumentOutOfRangeException>(() => Test());
            Assert.Contains(outofrange, error.Message);
        }

        [Theory(DisplayName = "Fake Anchor (hack, invalid)")]
        [InlineData(0, -1)]
        [InlineData(-1, 0)]
        [InlineData(510, 4)]
        [InlineData(768, 32)]
        public unsafe void FakeAnchorThenAppend(int offset, int length)
        {
            const int Limits = 512;

            void Test()
            {
                var anchor = new AllocatorAnchor();
                ((int*)&anchor)[0] = offset;
                ((int*)&anchor)[1] = length;
                Assert.Equal($"AllocatorAnchor(Offset: {offset}, Length: {length})", anchor.ToString());
                var allocator = new Allocator();
                AllocatorHelper.Append(ref allocator, new byte[Limits]);
                Assert.Equal(Limits, allocator.Length);
                Assert.Equal(1024, allocator.Capacity);
                AllocatorHelper.Append(ref allocator, anchor, default(object), (a, b) => throw new NotSupportedException());
            }

            var error = Assert.Throws<ArgumentOutOfRangeException>(() => Test());
            Assert.Contains(outofrange, error.Message);
        }

        [Fact(DisplayName = "Expand Capacity (hack)")]
        public unsafe void ExpandCapacity()
        {
            var buffer = new byte[256];
            buffer.AsSpan().Fill(0x80);
            var allocator = new Allocator(buffer);
            Assert.Equal(0, allocator.Length);
            Assert.Equal(256, allocator.Capacity);
            AllocatorHelper.Append(ref allocator, Enumerable.Repeat((byte)0x7F, 128).ToArray());
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
            AllocatorHelper.Append(ref allocator, 512, default(object), (a, b) => { });
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

        [Theory(DisplayName = "Expand Capacity (hack, invalid)")]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-16)]
        [InlineData(int.MinValue)]
        public void ExpandCapacityInvalid(int expand)
        {
            var methodInfo = typeof(Allocator).GetMethod("Expand", BindingFlags.Static | BindingFlags.NonPublic);
            var method = (Expand)Delegate.CreateDelegate(typeof(Expand), methodInfo);
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
            var methodInfo = typeof(Allocator).GetMethod("Ensure", BindingFlags.Static | BindingFlags.NonPublic);
            var ensure = (Ensure)Delegate.CreateDelegate(typeof(Ensure), methodInfo);
            var capacity = 14;
            var buffer = new byte[capacity];
            var allocator = new Allocator(buffer);
            Assert.Equal(capacity, allocator.Capacity);
            var result = ensure.Invoke(ref allocator, 0);
            Assert.Equal(capacity, allocator.Capacity);
            Assert.Equal(0, result);
        }

        [Theory(DisplayName = "Ensure Capacity (hack, invalid)")]
        [InlineData(-1)]
        [InlineData(int.MinValue)]
        public void EnsureCapacityInvalid(int expand)
        {
            var methodInfo = typeof(Allocator).GetMethod("Ensure", BindingFlags.Static | BindingFlags.NonPublic);
            var ensure = (Ensure)Delegate.CreateDelegate(typeof(Ensure), methodInfo);
            var error = Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                var allocator = new Allocator();
                _ = ensure.Invoke(ref allocator, expand);
            });
            Assert.Equal("length", error.ParamName);
        }

        [Theory(DisplayName = "Ensure Capacity (hack, exactly)")]
        [InlineData(0, 4, 4)]
        [InlineData(2, 8, 10)]
        public void EnsureCapacityExactly(int offset, int expand, int capacity)
        {
            var methodInfo = typeof(Allocator).GetMethod("Ensure", BindingFlags.Static | BindingFlags.NonPublic);
            var ensure = (Ensure)Delegate.CreateDelegate(typeof(Ensure), methodInfo);
            Assert.Equal(capacity, offset + expand);
            var buffer = new byte[capacity];
            var allocator = new Allocator(buffer);
            Assert.Equal(capacity, allocator.Capacity);
            Assert.Equal(0, allocator.Length);
            AllocatorHelper.Append(ref allocator, offset, 0, (a, b) => { });
            Assert.Equal(offset, allocator.Length);
            var result = ensure.Invoke(ref allocator, expand);
            Assert.Equal(capacity, allocator.Capacity);
            Assert.Equal(offset, result);
        }

        [Fact(DisplayName = "Anchor (hack, zero)")]
        public void AnchorZero()
        {
            var methodInfo = typeof(Allocator).GetMethod("Anchor", BindingFlags.Static | BindingFlags.NonPublic);
            var anchor = (Anchor)Delegate.CreateDelegate(typeof(Anchor), methodInfo);
            var buffer = new byte[25];
            var allocator = new Allocator(buffer);
            var result = anchor.Invoke(ref allocator, 0);
            Assert.Equal(0, result);
            Assert.Equal(0, allocator.Length);
            Assert.Equal(buffer.Length, allocator.Capacity);
        }

        [Theory(DisplayName = "Anchor & Append Length Prefix")]
        [InlineData(true, 0, 4, 1)]
        [InlineData(true, 1, 5, 5)]
        [InlineData(true, 1, 6, 5)]
        [InlineData(true, 1, 7, 5)]
        [InlineData(true, 1, 8, 5)]
        [InlineData(true, 1, 9, 5)]
        [InlineData(true, 1, 10, 5)]
        [InlineData(true, 1, 11, 5)]
        [InlineData(true, 1, 12, 2)]
        [InlineData(true, 1, 13, 2)]
        [InlineData(true, 15, 19, 19)]
        [InlineData(true, 15, 20, 16)]
        [InlineData(true, 16, 20, 17)]
        [InlineData(true, 17, 21, 21)]
        [InlineData(false, 17, 21, 21)]
        [InlineData(false, 0, 4, 4)]
        public void AnchorAppend(bool reduce, int length, int allocatorCapacity, int allocatorLength)
        {
            var anchorMethod = (Anchor)Delegate.CreateDelegate(typeof(Anchor), typeof(Allocator).GetMethod("Anchor", BindingFlags.Static | BindingFlags.NonPublic));
            var appendMethod = (AppendLengthPrefix)Delegate.CreateDelegate(typeof(AppendLengthPrefix), typeof(Allocator).GetMethod("AppendLengthPrefix", BindingFlags.Static | BindingFlags.NonPublic));
            var buffer = new byte[length];
            var random = new Random();
            random.NextBytes(buffer);
            var allocator = new Allocator(new Span<byte>(new byte[allocatorCapacity]), maxCapacity: allocatorCapacity);
            var anchor = anchorMethod.Invoke(ref allocator, 4);
            Assert.Equal(4, allocator.Length);
            Assert.Equal(allocatorCapacity, allocator.Capacity);
            AllocatorHelper.Append(ref allocator, buffer);
            appendMethod.Invoke(ref allocator, anchor, reduce);
            Assert.Equal(allocatorLength, allocator.Length);
            Assert.Equal(allocatorCapacity, allocator.Capacity);
            var span = allocator.AsSpan();
            var number = PrimitiveHelper.DecodeNumber(ref span);
            Assert.Equal(length, number);
            Assert.Equal(length, span.Length);
            Assert.Equal(buffer, span.ToArray());
        }

        [Fact(DisplayName = "Anchor & Append Length Prefix (for loop all conditions)")]
        public void AnchorAppendRange()
        {
            const int Limits = 16;
            var anchorMethod = (Anchor)Delegate.CreateDelegate(typeof(Anchor), typeof(Allocator).GetMethod("Anchor", BindingFlags.Static | BindingFlags.NonPublic));
            var appendMethod = (AppendLengthPrefix)Delegate.CreateDelegate(typeof(AppendLengthPrefix), typeof(Allocator).GetMethod("AppendLengthPrefix", BindingFlags.Static | BindingFlags.NonPublic));
            var random = new Random();
            foreach (var reduce in new[] { true, false })
            {
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
                        AllocatorHelper.Append(ref allocator, buffer);
                        Assert.Equal(length + 4, allocator.Length);
                        appendMethod.Invoke(ref allocator, anchor, reduce);
                        var lengthAlign8 = (length % 8) == 0 ? length : ((length >> 3) + 1) << 3;
                        Assert.True(lengthAlign8 >= length && lengthAlign8 % 8 == 0);
                        Assert.Equal(lengthAlign8 - length, (-length) & 7);
                        var actualReduce = reduce && length <= Limits && capacity - 4 - lengthAlign8 >= 0;
                        Assert.Equal(length + (actualReduce ? 1 : 4), allocator.Length);
                        var span = allocator.AsSpan();
                        var actualLength = PrimitiveHelper.DecodeNumber(ref span);
                        Assert.Equal(length, actualLength);
                        Assert.Equal(buffer, span.ToArray());
                    }
                }
            }
        }
    }
}
