using System;
using Xunit;

namespace Mikodev.Binary.Tests
{
    public class AllocatorTests
    {
        [Theory(DisplayName = "Fake Anchor (hack)")]
        [InlineData(4)]
        [InlineData(5)]
        [InlineData(8)]
        [InlineData(1024)]
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
            var numberSpan = span.Slice(offset - 4);
            var number = PrimitiveHelper.DecodeNumber(ref numberSpan);
            Assert.Equal(Limits - offset, number);
        }

        [Theory(DisplayName = "Fake Anchor (hack, invalid offset)")]
        [InlineData(int.MinValue + 0)]
        [InlineData(int.MinValue + 1)]
        [InlineData(int.MinValue + 2)]
        [InlineData(int.MinValue + 3)]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public unsafe void FakeAnchorRange(int offset)
        {
            void Test()
            {
                var anchor = new AllocatorAnchor();
                ((int*)&anchor)[0] = offset;
                ((int*)&anchor)[1] = sizeof(int);
                Assert.Equal($"AllocatorAnchor(Offset: {offset}, Length: 4)", anchor.ToString());
                var allocator = new Allocator();
                Assert.Equal(0, allocator.Length);
                AllocatorHelper.AppendLengthPrefix(ref allocator, anchor);
            }

            var error = Assert.Throws<ArgumentException>(() => Test());
            var message = "Invalid length prefix anchor or allocator modified.";
            Assert.Equal(message, error.Message);
        }
    }
}
