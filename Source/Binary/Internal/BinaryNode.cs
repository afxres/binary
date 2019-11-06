namespace Mikodev.Binary.Internal
{
    internal sealed class BinaryNode<T>
    {
        public long Index;

        public BinaryNode<T>[] Nodes;

        public bool HasValue;

        public T Value;
    }
}
