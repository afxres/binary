using System;

namespace Mikodev.Binary.Internal.Fallback.Decoders
{
    internal sealed class VariableOverriddenDecoder<T> : FallbackDecoder<T>
    {
        private readonly Converter<T> converter;

        public VariableOverriddenDecoder(Converter<T> converter) => this.converter = converter;

        public override T DecodeAuto(ref ReadOnlySpan<byte> span)
        {
            return this.converter.DecodeWithLengthPrefix(ref span);
        }
    }
}
