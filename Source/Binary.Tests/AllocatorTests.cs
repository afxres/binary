using System;
using Xunit;

namespace Mikodev.Binary.Tests
{
    public class AllocatorTests
    {
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

            var error = Assert.Throws<ArgumentException>(() => Test());
            var message = "Invalid length prefix anchor or allocator modified.";
            Assert.Equal(message, error.Message);
        }

        [Theory(DisplayName = "Fake Length Prefix Anchor (hack, invalid offset)")]
        [InlineData(-8, 7)]
        [InlineData(int.MinValue, int.MinValue + 15)]
        [InlineData(int.MaxValue - 15, int.MaxValue)]
        public unsafe void FakeAnchorRange(int from, int to)
        {
            static void Test(int offset)
            {
                var anchor = new AllocatorAnchor();
                ((int*)&anchor)[0] = offset;
                ((int*)&anchor)[1] = sizeof(int);
                Assert.Equal($"AllocatorAnchor(Offset: {offset}, Length: 4)", anchor.ToString());
                var allocator = new Allocator();
                Assert.Equal(0, allocator.Length);
                AllocatorHelper.AppendLengthPrefix(ref allocator, anchor);
            }

            var loop = 0;
            for (var i = (long)from; i <= to; i++)
            {
                var error = Assert.Throws<ArgumentException>(() => Test((int)i));
                var message = "Invalid length prefix anchor or allocator modified.";
                Assert.Equal(message, error.Message);
                loop++;
            }
            Assert.Equal(16, loop);
        }

        [Theory(DisplayName = "Fake Anchor (hack, invalid)")]
        [InlineData(0, -1)]
        [InlineData(-1, 0)]
        [InlineData(512, 1024)]
        public unsafe void FakeAnchorThenAppend(int offset, int length)
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
                AllocatorHelper.Append(ref allocator, anchor, default(object), (a, b) => throw new NotSupportedException());
            }

            var error = Assert.Throws<ArgumentOutOfRangeException>(() => Test());
            Assert.DoesNotContain("anchor", error.Message, StringComparison.InvariantCultureIgnoreCase);
            Assert.DoesNotContain("allocator", error.Message, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
