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

        private delegate void Ensure(ref Allocator allocator, int expand);

        private delegate void Expand(ref Allocator allocator, int expand);

        private delegate void MemoryMoveForward3(ref byte source, uint length);

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
            ensure.Invoke(ref allocator, 0);
            Assert.Equal(capacity, allocator.Capacity);
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
                ensure.Invoke(ref allocator, expand);
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
            ensure.Invoke(ref allocator, expand);
            Assert.Equal(capacity, allocator.Capacity);
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

        [Fact(DisplayName = "Memory Move Forward 3 (hack, from 1 to 31)")]
        public void MemoryMove()
        {
            var methodInfo = typeof(Converter).Assembly.GetTypes().Single(x => x.Name == "BufferHelper").GetMethod("MemoryMoveForward3", BindingFlags.Static | BindingFlags.NonPublic);
            var action = (MemoryMoveForward3)Delegate.CreateDelegate(typeof(MemoryMoveForward3), methodInfo);

            var random = new Random();
            var origin = new byte[256];
            random.NextBytes(origin);

            for (var offset = 0; offset < 64; offset++)
            {
                for (var length = 1; length <= 31; length++)
                {
                    var buffer = origin.AsSpan().ToArray();
                    var source = buffer.AsSpan().Slice(offset + 4, length).ToArray();
                    action.Invoke(ref buffer[offset + 4], (uint)length);
                    var target = buffer.AsSpan().Slice(offset + 1, length).ToArray();
                    Assert.Equal(source, target);

                    var originHead = origin.AsSpan().Slice(0, offset).ToArray();
                    var bufferHead = buffer.AsSpan().Slice(0, offset).ToArray();
                    Assert.Equal(originHead, bufferHead);
                    var originTail = origin.AsSpan().Slice(offset + 1 + length).ToArray();
                    var bufferTail = buffer.AsSpan().Slice(offset + 1 + length).ToArray();
                    Assert.Equal(originTail, bufferTail);
                }
            }
        }

        [Fact(DisplayName = "Memory Move Forward 3 (hack, zero)")]
        public void MemoryMoveZero()
        {
            var methodInfo = typeof(Converter).Assembly.GetTypes().Single(x => x.Name == "BufferHelper").GetMethod("MemoryMoveForward3", BindingFlags.Static | BindingFlags.NonPublic);
            var action = (MemoryMoveForward3)Delegate.CreateDelegate(typeof(MemoryMoveForward3), methodInfo);

            var random = new Random();
            var origin = new byte[256];
            random.NextBytes(origin);

            for (var offset = 0; offset < 64; offset++)
            {
                var buffer = origin.AsSpan().ToArray();
                action.Invoke(ref buffer[offset + 4], 0);
                Assert.Equal(origin, buffer);
            }
        }
    }
}
