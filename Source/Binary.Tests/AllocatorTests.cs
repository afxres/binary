using System;
using Xunit;

namespace Mikodev.Binary.Tests
{
    public class AllocatorTests
    {
        [Fact(DisplayName = "Fake Length Prefix Anchor (hack)")]
        public unsafe void FakeAnchor()
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
                return allocator.ToArray();
            }

            for (var i = 0; i < 4; i++)
            {
                var error = Assert.Throws<ArgumentException>(() => Test(i));
                var message = "Invalid length prefix anchor or allocator modified.";
                Assert.Equal(message, error.Message);
            }

            for (var i = 4; i < 8; i++)
            {
                var buffer = Test(i);
                var span = new ReadOnlySpan<byte>(buffer);
                var numberSpan = span.Slice(i - 4);
                var number = PrimitiveHelper.DecodeNumber(ref numberSpan);
                Assert.Equal(Limits - i, number);
            }
        }
    }
}
