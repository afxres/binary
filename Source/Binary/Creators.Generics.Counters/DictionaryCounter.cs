using System.Collections.Generic;

namespace Mikodev.Binary.Creators.Generics.Counters
{
    internal sealed class DictionaryCounter<K, V> : GenericsCounter<Dictionary<K, V>>
    {
        public override int Invoke(Dictionary<K, V> item) => item.Count;
    }
}
