using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Mikodev.Binary.Internal
{
    internal readonly struct HybridList
    {
        private readonly byte[][] collection;

        private readonly Dictionary<string, int> dictionary;

        public HybridList(KeyValuePair<string, byte[]>[] keys)
        {
            const int Limits = 12;
            Debug.Assert(keys != null && keys.Length > 0);
            Debug.Assert(keys.All(x => x.Key != null && x.Value != null));
            var condition = keys.Length > Limits;
            collection = condition ? null : keys.Select(x => x.Value).ToArray();
            dictionary = condition ? keys.Select((x, i) => new KeyValuePair<string, int>(x.Key, i)).ToDictionary(x => x.Key, x => x.Value) : null;
            Debug.Assert((dictionary != null && dictionary.Count > Limits) ^ (collection != null && collection.Length <= Limits));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Get(in ReadOnlySpan<byte> span)
        {
            const int NotFound = -1;
            if (dictionary != null)
                return dictionary.TryGetValue(Format.GetText(in span), out var index) ? index : NotFound;
            var itemCount = collection.Length;
            for (var i = 0; i < itemCount; i++)
                if (MemoryExtensions.SequenceEqual(collection[i], span))
                    return i;
            return NotFound;
        }
    }
}
