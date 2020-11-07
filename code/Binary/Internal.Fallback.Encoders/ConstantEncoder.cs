namespace Mikodev.Binary.Internal.Fallback.Encoders
{
    internal sealed class ConstantEncoder<T> : FallbackEncoder<T>
    {
        private readonly Converter<T> converter;

        public ConstantEncoder(Converter<T> converter) => this.converter = converter;

        public override void EncodeAuto(ref Allocator allocator, T item)
        {
            this.converter.Encode(ref allocator, item);
        }
    }
}
