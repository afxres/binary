using System;

namespace Mikodev.Binary.External
{
    internal sealed class BinaryNode<T>
    {
        public readonly long Index;

        public readonly BinaryNode<T>[] Nodes;

        public readonly bool HasValue;

        public readonly T Value;

        public BinaryNode(BinaryNode<T>[] nodes, long index, bool hasValue, T value)
        {
            this.Nodes = nodes ?? throw new ArgumentNullException(nameof(nodes));
            this.Index = index;
            this.HasValue = hasValue;
            this.Value = value;
        }
    }
}
