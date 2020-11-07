using System;

namespace Mikodev.Binary.Internal.Fallback.Decoders
{
    internal sealed class ConstantDecoder<T> : FallbackDecoder<T>
    {
        private readonly Converter<T> converter;

        public ConstantDecoder(Converter<T> converter) => this.converter = converter;

        public override T DecodeAuto(ref ReadOnlySpan<byte> span)
        {
            var converter = this.converter;
            var length = converter.Length;
            var buffer = MemoryHelper.EnsureLengthReturnBuffer(ref span, length);
            var result = converter.Decode(in buffer);
            return result;
        }
    }
}
