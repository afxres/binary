using System.Collections.Generic;

namespace Mikodev.Binary.Internal.Sequence.Counters
{
    internal sealed class DictionaryCounter<K, V> : SequenceCounter<Dictionary<K, V>>
    {
        public override int Invoke(Dictionary<K, V> item) => item.Count;
    }
}
