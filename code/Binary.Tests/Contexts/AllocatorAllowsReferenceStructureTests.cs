#if NET9_0_OR_GREATER
namespace Mikodev.Binary.Tests.Contexts;

using System;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Text;
using Xunit;

public class AllocatorAllowsReferenceStructureTests
{
    private ref struct TestReferenceStructure<T>
    {
        public ref T Location;
    }

    [Theory(DisplayName = "Append Test (known length)")]
    [InlineData(0xa)]
    [InlineData(0xbeef)]
    public void AppendTest(int data)
    {
        static void Invoke(ref Allocator allocator, int data)
        {
            var testStructure = new TestReferenceStructure<int> { Location = ref data };
            Allocator.Append(ref allocator, sizeof(int), testStructure, static (span, status) =>
            {
                BinaryPrimitives.WriteInt32LittleEndian(span, status.Location);
            });
            var result = BinaryPrimitives.ReadInt32LittleEndian(allocator.AsSpan());
            Assert.Equal(data, result);
        }
        var allocator = new Allocator();
        Invoke(ref allocator, data);
    }

    [Theory(DisplayName = "Append Test (unknown length)")]
    [InlineData("")]
    [InlineData("babe")]
    public void AppendTestUnknownLength(string data)
    {
        static void Invoke(ref Allocator allocator, string data)
        {
            var testStructure = new TestReferenceStructure<string> { Location = ref data };
            Allocator.Append(ref allocator, Encoding.UTF8.GetMaxByteCount(data.Length), testStructure, static (span, status) =>
            {
                return Encoding.UTF8.GetBytes(status.Location, span);
            });
            var result = Encoding.UTF8.GetString(allocator.AsSpan());
            Assert.Equal(data, result);
        }
        var allocator = new Allocator();
        Invoke(ref allocator, data);
    }

    [Theory(DisplayName = "Append With Length Prefix Test (allocator writer)")]
    [InlineData("")]
    [InlineData("dead")]
    public void AppendWithLengthPrefixTest(string data)
    {
        static void Invoke(ref Allocator allocator, string data)
        {
            var testStructure = new TestReferenceStructure<string> { Location = ref data };
            Allocator.AppendWithLengthPrefix(ref allocator, Encoding.UTF8.GetMaxByteCount(data.Length), testStructure, static (span, status) =>
            {
                return Encoding.UTF8.GetBytes(status.Location, span);
            });
            var buffer = allocator.AsSpan();
            var length = Converter.Decode(ref buffer);
            Assert.Equal(length, buffer.Length);
            var result = Encoding.UTF8.GetString(buffer);
            Assert.Equal(data, result);
        }
        var allocator = new Allocator();
        Invoke(ref allocator, data);
    }

    [Theory(DisplayName = "Append With Length Prefix Test (allocator action)")]
    [InlineData("")]
    [InlineData("cafe")]
    public void AppendWithLengthPrefixAllocatorActionTest(string data)
    {
        static void Invoke(ref Allocator allocator, string data)
        {
            var testStructure = new TestReferenceStructure<string> { Location = ref data };
            Allocator.AppendWithLengthPrefix(ref allocator, testStructure, static (ref Allocator allocator, scoped TestReferenceStructure<string> status) =>
            {
                Allocator.Append(ref allocator, MemoryMarshal.AsBytes(status.Location.AsSpan()));
            });
            var buffer = allocator.AsSpan();
            var length = Converter.Decode(ref buffer);
            Assert.Equal(length, buffer.Length);
            var result = MemoryMarshal.Cast<byte, char>(buffer).ToString();
            Assert.Equal(data, result);
        }
        var allocator = new Allocator();
        Invoke(ref allocator, data);
    }

    [Theory(DisplayName = "Invoke Test")]
    [InlineData("")]
    [InlineData("cafe")]
    public void InvokeTest(string data)
    {
        var testStructure = new TestReferenceStructure<string> { Location = ref data };
        var buffer = Allocator.Invoke(testStructure, static (ref Allocator allocator, scoped TestReferenceStructure<string> status) =>
        {
            Allocator.Append(ref allocator, status.Location, Encoding.UTF32);
        });
        var result = Encoding.UTF32.GetString(buffer);
        Assert.Equal(data, result);
    }
}
#endif
