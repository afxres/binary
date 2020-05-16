namespace Mikodev.Binary.Creators.Generics
{
    internal sealed class GenericsVariableEncoder<T, R> : GenericsAbstractEncoder<T>
    {
        private readonly GenericsAdapter<T, R> adapter;

        public GenericsVariableEncoder(GenericsAdapter<T, R> adapter)
        {
            this.adapter = adapter;
        }

        public override void EncodeWithLengthPrefix(ref Allocator allocator, T item)
        {
            var anchor = Allocator.Anchor(ref allocator, sizeof(int));
            adapter.Encode(ref allocator, item);
            Allocator.AppendLengthPrefix(ref allocator, anchor, reduce: true);
        }
    }
}
