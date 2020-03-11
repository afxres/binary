using System.Collections.Generic;

namespace Mikodev.Binary.External
{
    internal sealed class BinaryMutableNode<T>
    {
        public long Index;

        public readonly List<BinaryMutableNode<T>> Nodes = new List<BinaryMutableNode<T>>();

        public bool HasValue;

        public T Value;
    }
}
