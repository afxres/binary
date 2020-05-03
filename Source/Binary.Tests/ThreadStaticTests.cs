using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Mikodev.Binary.Tests
{
    public class ThreadStaticTests
    {
        private delegate void EncodeAction<T>(ref Allocator allocator, T item);

        private sealed class FakeVariableConverter<T> : Converter<T>
        {
            private readonly EncodeAction<T> encodeAction;

            public FakeVariableConverter(EncodeAction<T> encodeAction) : base(0) => this.encodeAction = encodeAction;

            public override void Encode(ref Allocator allocator, T item) => encodeAction.Invoke(ref allocator, item);

            public override T Decode(in ReadOnlySpan<byte> span) => throw new NotSupportedException();
        }

        [Fact(DisplayName = "Multi Threads (encode, thread static)")]
        public async Task MultiThreadsTestAsync()
        {
            const int TaskCount = 16;
            const int LoopCount = 4096;
            var generator = Generator.CreateDefault();
            var funcs = Enumerable.Range(0, TaskCount).Select(x => new Action(() =>
            {
                var model = Enumerable.Range(0, 1024).Select(_ => Guid.NewGuid().ToString()).ToArray();
                for (var i = 0; i < LoopCount; i++)
                {
                    var bytes = generator.Encode(model);
                    Assert.True(bytes.Length < (1 << 16));
                    var value = generator.Decode(bytes, anonymous: model);
                    Assert.Equal<string>(model, value);
                }
            })).ToList();
            var tasks = funcs.Select(Task.Run).ToList();
            await Task.WhenAll(tasks);
        }

        [Fact(DisplayName = "Multi Threads (invoke, thread static)")]
        public async Task MultiThreadsInvokeAsync()
        {
            const int TaskCount = 16;
            const int LoopCount = 4096;
            var generator = Generator.CreateDefault();
            var converter = generator.GetConverter<string[]>();
            var funcs = Enumerable.Range(0, TaskCount).Select(x => new Action(() =>
            {
                var model = Enumerable.Range(0, 1024).Select(_ => Guid.NewGuid().ToString()).ToArray();
                for (var i = 0; i < LoopCount; i++)
                {
                    var bytes = AllocatorHelper.Invoke(model, converter.Encode);
                    Assert.True(bytes.Length < (1 << 16));
                    var value = converter.Decode(bytes);
                    Assert.Equal<string>(model, value);
                }
            })).ToList();
            var tasks = funcs.Select(Task.Run).ToList();
            await Task.WhenAll(tasks);
        }

        [Fact(DisplayName = "Nested Call (invoke)")]
        public void Nested()
        {
            static IEnumerable<bool> TestGroup()
            {
                var buffer01 = default(byte[]);
                var buffer02 = default(byte[]);
                var buffer03 = default(byte[]);

                buffer01 = AllocatorHelper.Invoke(0x11223344, (ref Allocator allocator01, int item) =>
                {
                    Assert.Equal(0, allocator01.Length);
                    Assert.Equal(65536, allocator01.Capacity);
                    Assert.Equal(int.MaxValue, allocator01.MaxCapacity);
                    AllocatorHelper.Append(ref allocator01, BitConverter.GetBytes(item));

                    buffer02 = AllocatorHelper.Invoke(0x33445566, (ref Allocator allocator02, int item) =>
                    {
                        Assert.Equal(0, allocator02.Length);
                        Assert.Equal(0, allocator02.Capacity);
                        Assert.Equal(int.MaxValue, allocator02.MaxCapacity);
                        AllocatorHelper.Append(ref allocator02, BitConverter.GetBytes(item));

                        buffer03 = AllocatorHelper.Invoke(0x55667788, (ref Allocator allocator03, int item) =>
                        {
                            Assert.Equal(0, allocator03.Length);
                            Assert.Equal(0, allocator03.Capacity);
                            Assert.Equal(int.MaxValue, allocator03.MaxCapacity);
                            AllocatorHelper.Append(ref allocator03, BitConverter.GetBytes(item));
                        });
                    });
                });

                var result01 = BitConverter.ToInt32(buffer01, 0);
                var result02 = BitConverter.ToInt32(buffer02, 0);
                var result03 = BitConverter.ToInt32(buffer03, 0);

                yield return result01 == 0x11223344;
                yield return result02 == 0x33445566;
                yield return result03 == 0x55667788;
            }

            const int Limits = 16;
            var result = Enumerable.Range(0, Limits).SelectMany(x => TestGroup()).ToList();
            Assert.Equal(Limits * 3, result.Count);
            Assert.All(result, x => Assert.True(x));
        }

        [Fact(DisplayName = "Nested Call (encode)")]
        public void NestedV2()
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
                    AllocatorHelper.Append(ref allocator01, BitConverter.GetBytes(item01));

                    buffer02 = new FakeVariableConverter<uint>((ref Allocator allocator02, uint item02) =>
                    {
                        Assert.Equal(0, allocator02.Length);
                        Assert.Equal(0, allocator02.Capacity);
                        Assert.Equal(int.MaxValue, allocator02.MaxCapacity);
                        AllocatorHelper.Append(ref allocator02, BitConverter.GetBytes(item02));

                        buffer03 = new FakeVariableConverter<uint>((ref Allocator allocator03, uint item03) =>
                        {
                            Assert.Equal(0, allocator03.Length);
                            Assert.Equal(0, allocator03.Capacity);
                            Assert.Equal(int.MaxValue, allocator03.MaxCapacity);
                            AllocatorHelper.Append(ref allocator03, BitConverter.GetBytes(item03));
                        })
                        .Encode(0xDDEEFF00);
                    })
                    .Encode(0xBBCCDDEE);
                })
                .Encode(0x99AABBCC);

                var result01 = BitConverter.ToUInt32(buffer01, 0);
                var result02 = BitConverter.ToUInt32(buffer02, 0);
                var result03 = BitConverter.ToUInt32(buffer03, 0);

                yield return result01 == 0x99AABBCC;
                yield return result02 == 0xBBCCDDEE;
                yield return result03 == 0xDDEEFF00;
            }

            const int Limits = 12;
            var result = Enumerable.Range(0, Limits).SelectMany(x => TestGroup()).ToList();
            Assert.Equal(Limits * 3, result.Count);
            Assert.All(result, x => Assert.True(x));
        }

        [Fact(DisplayName = "Exception Call (invoke)")]
        public void ExceptionCall()
        {
            static IEnumerable<bool> TestGroup()
            {
                var error = Assert.Throws<NotSupportedException>(() => AllocatorHelper.Invoke(0U, (ref Allocator allocator01, uint _) =>
                {
                    Assert.Equal(0, allocator01.Length);
                    Assert.Equal(65536, allocator01.Capacity);
                    Assert.Equal(int.MaxValue, allocator01.MaxCapacity);
                    throw new NotSupportedException("Test Message Alpha");
                }));

                var buffer = AllocatorHelper.Invoke(0x778899AAU, (ref Allocator allocator02, uint item) =>
                {
                    Assert.Equal(0, allocator02.Length);
                    Assert.Equal(65536, allocator02.Capacity);
                    Assert.Equal(int.MaxValue, allocator02.MaxCapacity);
                    AllocatorHelper.Append(ref allocator02, BitConverter.GetBytes(item));
                });

                var message = error.Message;
                var result = BitConverter.ToInt32(buffer, 0);
                yield return message == "Test Message Alpha";
                yield return result == 0x778899AA;
            }

            const int Limits = 8;
            var result = Enumerable.Range(0, Limits).SelectMany(x => TestGroup()).ToList();
            Assert.Equal(Limits * 2, result.Count);
            Assert.All(result, x => Assert.True(x));
        }

        [Fact(DisplayName = "Exception Call (encode)")]
        public void ExceptionCallV2()
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
                    AllocatorHelper.Append(ref allocator02, BitConverter.GetBytes(item02));
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
            Assert.All(result, x => Assert.True(x));
        }
    }
}
