using System;
using Xunit;

namespace Mikodev.Binary.Tests
{
    public class AllocatorTests
    {
        [Theory(DisplayName = "Fake Length Prefix Anchor (hack)")]
        [InlineData(4)]
        [InlineData(5)]
        [InlineData(8)]
        [InlineData(1024)]
        public unsafe void FakeAnchor(int offset)
        {
            const int Limits = 1024;

            static byte[] Test(int i)
            {
                var anchor = new AllocatorAnchor();
                *(int*)&anchor = i;
                Assert.Equal($"AllocatorAnchor(Offset: {i})", anchor.ToString());
                var allocator = new Allocator();
                _ = AllocatorHelper.Allocate(ref allocator, Limits);
                Assert.Equal(Limits, allocator.Length);
                AllocatorHelper.AppendLengthPrefix(ref allocator, anchor);
                return allocator.AsSpan().ToArray();
            }

            var buffer = Test(offset);
            var span = new ReadOnlySpan<byte>(buffer);
            var numberSpan = span.Slice(offset - 4);
            var number = PrimitiveHelper.DecodeNumber(ref numberSpan);
            Assert.Equal(Limits - offset, number);
        }

        [Theory(DisplayName = "Fake Length Prefix Anchor (hack)")]
        [InlineData(int.MinValue)]
        [InlineData(-4)]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public unsafe void FakeAnchorRange(int offset)
        {
            const int Limits = 100;

            static byte[] Test(int i)
            {
                var anchor = new AllocatorAnchor();
                *(int*)&anchor = i;
                Assert.Equal($"AllocatorAnchor(Offset: {i})", anchor.ToString());
                var allocator = new Allocator();
                _ = AllocatorHelper.Allocate(ref allocator, Limits);
                Assert.Equal(Limits, allocator.Length);
                AllocatorHelper.AppendLengthPrefix(ref allocator, anchor);
                return allocator.AsSpan().ToArray();
            }

            var error = Assert.Throws<ArgumentException>(() => Test(offset));
            var message = "Invalid length prefix anchor or allocator modified.";
            Assert.Equal(message, error.Message);
        }
    }
}
