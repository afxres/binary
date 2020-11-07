namespace Mikodev.Binary.Internal.Fallback.Encoders
{
    internal sealed class VariableEncoder<T> : FallbackEncoder<T>
    {
        private readonly Converter<T> converter;

        public VariableEncoder(Converter<T> converter) => this.converter = converter;

        public override void EncodeAuto(ref Allocator allocator, T item)
        {
            var anchor = Allocator.Anchor(ref allocator, sizeof(int));
            this.converter.Encode(ref allocator, item);
            Allocator.AppendLengthPrefix(ref allocator, anchor);
        }
    }
}
