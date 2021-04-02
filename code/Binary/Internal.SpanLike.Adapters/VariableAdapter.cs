using System;

namespace Mikodev.Binary.Internal.SpanLike.Adapters
{
    internal sealed class VariableAdapter<T> : SpanLikeAdapter<T>
    {
        private readonly Converter<T> converter;

        public VariableAdapter(Converter<T> converter) => this.converter = converter;

        public override void Encode(ref Allocator allocator, ReadOnlySpan<T> item)
        {
            var converter = this.converter;
            foreach (var i in item)
                converter.EncodeAuto(ref allocator, i);
        }

        public override MemoryResult<T> Decode(ReadOnlySpan<byte> span)
        {
            if (span.Length is 0)
                return new MemoryResult<T>(Array.Empty<T>(), 0);
            const int Initial = 8;
            var memory = new MemoryBuffer<T>(Initial);
            var body = span;
            var converter = this.converter;
            while (body.Length is not 0)
                memory.Append(converter.DecodeAuto(ref body));
            return memory.Result();
        }
    }
}
