using BenchmarkDotNet.Attributes;

namespace Mikodev.Binary.Benchmarks.PrimitiveTests
{
    [MemoryDiagnoser]
    public class PrimitiveBenchmarks
    {
        private byte[] buffer;

        private int number1;

        private int number1025;

        private int number1048575;

        [GlobalSetup]
        public void Setup()
        {
            this.buffer = new byte[1024];
            this.number1 = 1;
            this.number1025 = 1025;
            this.number1048575 = 1048575;
        }

        [Benchmark(Description = "Encode Number (size: 1)")]
        public void E01()
        {
            var allocator = new Allocator(this.buffer);
            PrimitiveHelper.EncodeNumber(ref allocator, this.number1);
        }

        [Benchmark(Description = "Encode Number (size: 2)")]
        public void E02()
        {
            var allocator = new Allocator(this.buffer);
            PrimitiveHelper.EncodeNumber(ref allocator, this.number1025);
        }

        [Benchmark(Description = "Encode Number (size: 4)")]
        public void E03()
        {
            var allocator = new Allocator(this.buffer);
            PrimitiveHelper.EncodeNumber(ref allocator, this.number1048575);
        }
    }
}
