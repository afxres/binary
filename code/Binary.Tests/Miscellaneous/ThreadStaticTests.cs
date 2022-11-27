namespace Mikodev.Binary.Tests.Miscellaneous;

using Mikodev.Binary.Tests.Internal;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Xunit;

public class ThreadStaticTests
{
    private delegate void EncodeAction<T>(ref Allocator allocator, T item);

    private sealed class ReferenceEqualityComparer<T> : IEqualityComparer<T> where T : class
    {
        public static readonly ReferenceEqualityComparer<T> Instance = new ReferenceEqualityComparer<T>();

        private ReferenceEqualityComparer() { }

        public bool Equals(T? x, T? y) => ReferenceEquals(x, y);

        public int GetHashCode(T obj) => RuntimeHelpers.GetHashCode(obj);
    }

    private sealed class FakeVariableConverter<T> : Converter<T>
    {
        private readonly EncodeAction<T> encodeAction;

        public FakeVariableConverter(EncodeAction<T> encodeAction) : base(0) => this.encodeAction = encodeAction;

        public override void Encode(ref Allocator allocator, T? item) => this.encodeAction.Invoke(ref allocator, Assert.IsAssignableFrom<T>(item));

        public override T Decode(in ReadOnlySpan<byte> span) => throw new NotSupportedException();
    }

    [Fact(DisplayName = "Multi Threads Encode (thread static)")]
    public void MultiThreadsEncodeAsync()
    {
        const int ThreadCount = 4;
        var dictionary = new ConcurrentDictionary<int, byte[]>();
        var handle = new ManualResetEvent(false);
        var bufferHelperType = typeof(IConverter).Assembly.GetTypes().Single(x => x.Name == "BufferModule");
        var threadStaticField = bufferHelperType.GetFieldNotNull("ThreadStaticInstance", BindingFlags.Static | BindingFlags.NonPublic);
        var bufferField = bufferHelperType.GetFieldNotNull("buffer", BindingFlags.Instance | BindingFlags.NonPublic);
        var attribute = threadStaticField.GetCustomAttributes(false).SingleOrDefault(x => x is ThreadStaticAttribute);
        Assert.NotNull(attribute);

        var threads = Enumerable.Range(0, ThreadCount).Select(id => new Thread(() =>
        {
            _ = handle.WaitOne();
            _ = new FakeVariableConverter<int>((ref Allocator allocator, int item) =>
            {
                var bufferHelper = threadStaticField.GetValue(null);
                var buffer = Assert.IsAssignableFrom<byte[]>(bufferField.GetValue(bufferHelper));
                ref var head = ref MemoryMarshal.GetReference(allocator.AsSpan());
                ref var data = ref MemoryMarshal.GetReference(new Span<byte>(buffer));
                Assert.True(Unsafe.AreSame(ref head, ref data));
                dictionary[id] = buffer;
                Converter.EncodeWithLengthPrefix(ref allocator, BitConverter.GetBytes(id));
            })
            .Encode(0);
        }))
        .ToList();
        threads.ForEach(x => x.Start());
        _ = handle.Set();
        threads.ForEach(x => x.Join());

        var set = new HashSet<byte[]>(dictionary.Values, ReferenceEqualityComparer<byte[]>.Instance);
        Assert.Equal(ThreadCount, set.Count);
        Assert.Equal(ThreadCount, dictionary.Count);
        Assert.All(dictionary, x =>
        {
            var buffer = x.Value;
            Assert.Equal(65536, buffer.Length);
            var span = new ReadOnlySpan<byte>(buffer);
            var result = BitConverter.ToInt32(Converter.DecodeWithLengthPrefix(ref span));
            Assert.Equal(x.Key, result);
        });
    }

    [Fact(DisplayName = "Multi Threads Invoke (thread static)")]
    public void MultiThreadsInvokeAsync()
    {
        const int ThreadCount = 4;
        var dictionary = new ConcurrentDictionary<int, byte[]>();
        var handle = new ManualResetEvent(false);
        var bufferHelperType = typeof(IConverter).Assembly.GetTypes().Where(x => x.Name == "BufferModule").Single();
        var threadStaticField = bufferHelperType.GetFieldNotNull("ThreadStaticInstance", BindingFlags.Static | BindingFlags.NonPublic);
        var bufferField = bufferHelperType.GetFieldNotNull("buffer", BindingFlags.Instance | BindingFlags.NonPublic);
        var attribute = threadStaticField.GetCustomAttributes(false).Where(x => x is ThreadStaticAttribute).SingleOrDefault();
        Assert.NotNull(attribute);

        var threads = Enumerable.Range(0, ThreadCount).Select(id => new Thread(() =>
        {
            _ = handle.WaitOne();
            _ = Allocator.Invoke(-1, (ref Allocator allocator, int item) =>
            {
                var bufferHelper = threadStaticField.GetValue(null);
                var buffer = Assert.IsAssignableFrom<byte[]>(bufferField.GetValue(bufferHelper));
                ref var head = ref MemoryMarshal.GetReference(allocator.AsSpan());
                ref var data = ref MemoryMarshal.GetReference(new Span<byte>(buffer));
                Assert.True(Unsafe.AreSame(ref head, ref data));
                dictionary[id] = buffer;
                Allocator.AppendWithLengthPrefix(ref allocator, id.ToString(), Encoding.UTF8);
            });
        }))
        .ToList();
        threads.ForEach(x => x.Start());
        _ = handle.Set();
        threads.ForEach(x => x.Join());

        var set = new HashSet<byte[]>(dictionary.Values, ReferenceEqualityComparer<byte[]>.Instance);
        Assert.Equal(ThreadCount, set.Count);
        Assert.Equal(ThreadCount, dictionary.Count);
        Assert.All(dictionary, x =>
        {
            var buffer = x.Value;
            Assert.Equal(65536, buffer.Length);
            var span = new ReadOnlySpan<byte>(buffer);
            var result = Encoding.UTF8.GetString(Converter.DecodeWithLengthPrefix(ref span));
            Assert.Equal(x.Key.ToString(), result);
        });
    }

    [Fact(DisplayName = "Nested Invoke")]
    public void NestedInvoke()
    {
        static IEnumerable<bool> TestGroup()
        {
            var buffer01 = default(byte[]);
            var buffer02 = default(byte[]);
            var buffer03 = default(byte[]);

            buffer01 = Allocator.Invoke(0x11223344, (ref Allocator allocator01, int item) =>
            {
                Assert.Equal(0, allocator01.Length);
                Assert.Equal(65536, allocator01.Capacity);
                Assert.Equal(int.MaxValue, allocator01.MaxCapacity);
                Allocator.Append(ref allocator01, BitConverter.GetBytes(item));

                buffer02 = Allocator.Invoke(0x33445566, (ref Allocator allocator02, int item) =>
                {
                    Assert.Equal(0, allocator02.Length);
                    Assert.Equal(0, allocator02.Capacity);
                    Assert.Equal(int.MaxValue, allocator02.MaxCapacity);
                    Allocator.Append(ref allocator02, BitConverter.GetBytes(item));

                    buffer03 = Allocator.Invoke(0x55667788, (ref Allocator allocator03, int item) =>
                    {
                        Assert.Equal(0, allocator03.Length);
                        Assert.Equal(0, allocator03.Capacity);
                        Assert.Equal(int.MaxValue, allocator03.MaxCapacity);
                        Allocator.Append(ref allocator03, BitConverter.GetBytes(item));
                    });
                });
            });

            var result01 = BitConverter.ToInt32(Assert.IsAssignableFrom<byte[]>(buffer01), 0);
            var result02 = BitConverter.ToInt32(Assert.IsAssignableFrom<byte[]>(buffer02), 0);
            var result03 = BitConverter.ToInt32(Assert.IsAssignableFrom<byte[]>(buffer03), 0);

            yield return result01 == 0x11223344;
            yield return result02 == 0x33445566;
            yield return result03 == 0x55667788;
        }

        const int Limits = 16;
        var result = Enumerable.Range(0, Limits).SelectMany(x => TestGroup()).ToList();
        Assert.Equal(Limits * 3, result.Count);
        Assert.All(result, Assert.True);
    }

    [Fact(DisplayName = "Nested Encode")]
    public void NestedEncode()
    {
        static IEnumerable<bool> TestGroup()
        {
            var buffer01 = default(byte[]);
            var buffer02 = default(byte[]);
            var buffer03 = default(byte[]);

            buffer01 = new FakeVariableConverter<uint>((ref Allocator allocator01, uint item01) =>
            {
                Assert.Equal(0, allocator01.Length);
                Assert.Equal(65536, allocator01.Capacity);
                Assert.Equal(int.MaxValue, allocator01.MaxCapacity);
                Allocator.Append(ref allocator01, BitConverter.GetBytes(item01));

                buffer02 = new FakeVariableConverter<uint>((ref Allocator allocator02, uint item02) =>
                {
                    Assert.Equal(0, allocator02.Length);
                    Assert.Equal(0, allocator02.Capacity);
                    Assert.Equal(int.MaxValue, allocator02.MaxCapacity);
                    Allocator.Append(ref allocator02, BitConverter.GetBytes(item02));

                    buffer03 = new FakeVariableConverter<uint>((ref Allocator allocator03, uint item03) =>
                    {
                        Assert.Equal(0, allocator03.Length);
                        Assert.Equal(0, allocator03.Capacity);
                        Assert.Equal(int.MaxValue, allocator03.MaxCapacity);
                        Allocator.Append(ref allocator03, BitConverter.GetBytes(item03));
                    })
                    .Encode(0xDDEEFF00);
                })
                .Encode(0xBBCCDDEE);
            })
            .Encode(0x99AABBCC);

            var result01 = BitConverter.ToUInt32(Assert.IsAssignableFrom<byte[]>(buffer01), 0);
            var result02 = BitConverter.ToUInt32(Assert.IsAssignableFrom<byte[]>(buffer02), 0);
            var result03 = BitConverter.ToUInt32(Assert.IsAssignableFrom<byte[]>(buffer03), 0);

            yield return result01 == 0x99AABBCC;
            yield return result02 == 0xBBCCDDEE;
            yield return result03 == 0xDDEEFF00;
        }

        const int Limits = 12;
        var result = Enumerable.Range(0, Limits).SelectMany(x => TestGroup()).ToList();
        Assert.Equal(Limits * 3, result.Count);
        Assert.All(result, Assert.True);
    }

    [Fact(DisplayName = "Invoke With Exception")]
    public void ExceptionCallInvoke()
    {
        static IEnumerable<bool> TestGroup()
        {
            var error = Assert.Throws<NotSupportedException>(() => Allocator.Invoke(0U, (ref Allocator allocator01, uint _) =>
            {
                Assert.Equal(0, allocator01.Length);
                Assert.Equal(65536, allocator01.Capacity);
                Assert.Equal(int.MaxValue, allocator01.MaxCapacity);
                throw new NotSupportedException("Test Message Alpha");
            }));

            var buffer = Allocator.Invoke(0x778899AAU, (ref Allocator allocator02, uint item) =>
            {
                Assert.Equal(0, allocator02.Length);
                Assert.Equal(65536, allocator02.Capacity);
                Assert.Equal(int.MaxValue, allocator02.MaxCapacity);
                Allocator.Append(ref allocator02, BitConverter.GetBytes(item));
            });

            var message = error.Message;
            var result = BitConverter.ToInt32(buffer, 0);
            yield return message == "Test Message Alpha";
            yield return result == 0x778899AA;
        }

        const int Limits = 8;
        var result = Enumerable.Range(0, Limits).SelectMany(x => TestGroup()).ToList();
        Assert.Equal(Limits * 2, result.Count);
        Assert.All(result, Assert.True);
    }

    [Fact(DisplayName = "Encode With Exception")]
    public void ExceptionCallEncode()
    {
        static IEnumerable<bool> TestGroup()
        {
            var error = Assert.Throws<NotSupportedException>(() => new FakeVariableConverter<uint>((ref Allocator allocator01, uint item01) =>
            {
                Assert.Equal(0, allocator01.Length);
                Assert.Equal(65536, allocator01.Capacity);
                Assert.Equal(int.MaxValue, allocator01.MaxCapacity);
                throw new NotSupportedException("Test Message Bravo");
            })
            .Encode(0xFFEEDDCC));

            var buffer = new FakeVariableConverter<uint>((ref Allocator allocator02, uint item02) =>
            {
                Assert.Equal(0, allocator02.Length);
                Assert.Equal(65536, allocator02.Capacity);
                Assert.Equal(int.MaxValue, allocator02.MaxCapacity);
                Allocator.Append(ref allocator02, BitConverter.GetBytes(item02));
            })
            .Encode(0xBBAA9988);

            var message = error.Message;
            var result = BitConverter.ToUInt32(buffer, 0);
            yield return message == "Test Message Bravo";
            yield return result == 0xBBAA9988;
        }

        const int Limits = 6;
        var result = Enumerable.Range(0, Limits).SelectMany(x => TestGroup()).ToList();
        Assert.Equal(Limits * 2, result.Count);
        Assert.All(result, Assert.True);
    }
}
