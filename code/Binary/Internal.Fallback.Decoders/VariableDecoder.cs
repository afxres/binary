using System;

namespace Mikodev.Binary.Internal.Fallback.Decoders
{
    internal sealed class VariableDecoder<T> : FallbackDecoder<T>
    {
        private readonly Converter<T> converter;

        public VariableDecoder(Converter<T> converter) => this.converter = converter;

        public override T DecodeAuto(ref ReadOnlySpan<byte> span)
        {
            return this.converter.Decode(Converter.DecodeWithLengthPrefix(ref span));
        }
    }
}
