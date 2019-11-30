using System;
using System.Linq;
using System.Runtime.InteropServices;
using Xunit;

namespace Mikodev.Binary.Tests
{
    public class AllocatorTests
    {
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

        [Fact(DisplayName = "Expand Capacity")]
        public unsafe void ExpandCapacity()
        {
            var buffer = new byte[256];
            Array.Fill(buffer, (byte)0x80);
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
    }
}
