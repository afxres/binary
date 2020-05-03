using BenchmarkDotNet.Attributes;
using System;

namespace Mikodev.Binary.Benchmarks.AllocatorTests
{
    [MemoryDiagnoser]
    public class AllocatorBenchmarks
    {
        private byte[] buffer0;

        private byte[] buffer1;

        private byte[] buffer1024;

        private int maxCapacity512;

        private int maxCapacity2048;

        private AllocatorAction<byte[]> ignoreAction;

        private AllocatorAction<byte[]> appendAction;

        [GlobalSetup]
        public void Setup()
        {
            buffer0 = Array.Empty<byte>();
            buffer1 = new byte[] { 0x7F };
            buffer1024 = new byte[1024];
            maxCapacity512 = 512;
            maxCapacity2048 = 2048;
            ignoreAction = new AllocatorAction<byte[]>((ref Allocator allocator, byte[] data) => { });
            appendAction = new AllocatorAction<byte[]>((ref Allocator allocator, byte[] data) => AllocatorHelper.Append(ref allocator, data));
        }

        [Benchmark(Description = "Append (from bytes 1024, length: 0)")]
        public void A01()
        {
            var allocator = new Allocator(this.buffer1024);
            AllocatorHelper.Append(ref allocator, this.buffer0);
        }

        [Benchmark(Description = "Append (from bytes 1024, length: 1)")]
        public void A02()
        {
            var allocator = new Allocator(this.buffer1024);
            AllocatorHelper.Append(ref allocator, this.buffer1);
        }

        [Benchmark(Description = "Append (from bytes 1024 with max capacity 512, length: 0)")]
        public void A03()
        {
            var allocator = new Allocator(this.buffer1024, this.maxCapacity512);
            AllocatorHelper.Append(ref allocator, this.buffer0);
        }

        [Benchmark(Description = "Append (from bytes 1024 with max capacity 512, length: 1)")]
        public void A04()
        {
            var allocator = new Allocator(this.buffer1024, this.maxCapacity512);
            AllocatorHelper.Append(ref allocator, this.buffer1);
        }

        [Benchmark(Description = "Append (from bytes 1024 with max capacity 2048, length: 0)")]
        public void A05()
        {
            var allocator = new Allocator(this.buffer1024, this.maxCapacity2048);
            AllocatorHelper.Append(ref allocator, this.buffer0);
        }

        [Benchmark(Description = "Append (from bytes 1024 with max capacity 2048, length: 1)")]
        public void A06()
        {
            var allocator = new Allocator(this.buffer1024, this.maxCapacity2048);
            AllocatorHelper.Append(ref allocator, this.buffer1);
        }

        [Benchmark(Description = "Invoke (do nothing)")]
        public byte[] I00()
        {
            return AllocatorHelper.Invoke(null, this.ignoreAction);
        }

        [Benchmark(Description = "Invoke (append length: 0)")]
        public byte[] I01()
        {
            return AllocatorHelper.Invoke(this.buffer0, this.appendAction);
        }

        [Benchmark(Description = "Invoke (append length: 1)")]
        public byte[] I02()
        {
            return AllocatorHelper.Invoke(this.buffer1, this.appendAction);
        }
    }
}
