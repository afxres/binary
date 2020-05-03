using System.Collections.Generic;

namespace Mikodev.Binary.External
{
    internal sealed class NodeTreeVariable<T>
    {
        public long Header;

        public readonly List<NodeTreeVariable<T>> Values = new List<NodeTreeVariable<T>>();

        public bool Exists;

        public T Intent;
    }
}
