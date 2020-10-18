using BenchmarkDotNet.Attributes;

namespace Mikodev.Binary.Benchmarks.ObjectConverterTests
{
    public class ObjectConverterBenchmarks
    {
        private byte[] buffer;

        private IGenerator generator;

        private Converter<object> objectConverter;

        [Params(1024, "string")]
        public object Item;

        [GlobalSetup]
        public void Setup()
        {
            this.buffer = new byte[65536];
            this.generator = Generator.CreateDefault();
            this.objectConverter = this.generator.GetConverter<object>();
        }

        [Benchmark(Description = "Encode (object converter)")]
        public void A01()
        {
            var item = this.Item;
            var converter = this.objectConverter;
            var allocator = new Allocator(this.buffer);
            converter.Encode(ref allocator, item);
        }

        [Benchmark(Description = "Encode (get converter via type)")]
        public void A02()
        {
            var item = this.Item;
            var converter = this.generator.GetConverter(item.GetType());
            var allocator = new Allocator(this.buffer);
            converter.Encode(ref allocator, item);
        }
    }
}
