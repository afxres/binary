using Mikodev.Binary.Internal.Contexts;

namespace Mikodev.Binary.Internal.Sequence.Encoders
{
    internal sealed class DelegateEncoder<T> : SequenceEncoder<T>
    {
        private readonly ContextCollectionEncoder<T> encoder;

        public DelegateEncoder(ContextCollectionEncoder<T> encoder) => this.encoder = encoder;

        public override void Encode(ref Allocator allocator, T item)
        {
            if (item is null)
                return;
            this.encoder.Invoke(ref allocator, item);
        }
    }
}
