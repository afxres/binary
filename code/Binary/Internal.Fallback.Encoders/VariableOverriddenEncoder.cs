namespace Mikodev.Binary.Internal.Fallback.Encoders
{
    internal sealed class VariableOverriddenEncoder<T> : FallbackEncoder<T>
    {
        private readonly Converter<T> converter;

        public VariableOverriddenEncoder(Converter<T> converter) => this.converter = converter;

        public override void EncodeAuto(ref Allocator allocator, T item)
        {
            this.converter.EncodeWithLengthPrefix(ref allocator, item);
        }
    }
}
