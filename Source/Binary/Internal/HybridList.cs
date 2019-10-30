using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Mikodev.Binary.Internal
{
    internal readonly struct HybridList
    {
        private const int ItemLimits = 16;

        private const int BuckSize = 17;

        private readonly HybridBuck[][] bucks;

        private readonly Dictionary<string, int> pairs;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetHashCode(in ReadOnlySpan<byte> span)
        {
            var byteCount = span.Length;
            if (byteCount == 0)
                return byteCount;
            ref var location = ref MemoryMarshal.GetReference(span);
            if (byteCount == 1)
                return byteCount ^ location;
            return (byteCount ^ location) | (Unsafe.Add(ref location, byteCount - 1) << 8);
        }

        private static HybridBuck[][] GetBucks(IReadOnlyList<byte[]> items)
        {
            var bucks = new List<HybridBuck>[BuckSize];
            for (var i = 0; i < items.Count; i++)
            {
                var item = items[i];
                var hash = GetHashCode(item);
                var buck = new HybridBuck(i, item);
                ref var list = ref bucks[(uint)hash % BuckSize];
                if (list == null)
                    list = new List<HybridBuck>();
                list.Add(buck);
            }
            Debug.Assert(bucks.Any(x => x == null));
            Debug.Assert(bucks.Any(x => x != null));
            return bucks.Select(x => x?.ToArray()).ToArray();
        }

        public HybridList(KeyValuePair<string, byte[]>[] keys)
        {
            Debug.Assert(keys != null && keys.Length > 0);
            Debug.Assert(keys.All(x => x.Key != null && x.Value != null));
            var condition = keys.Length > ItemLimits;
            pairs = condition ? keys.Select((x, i) => new KeyValuePair<string, int>(x.Key, i)).ToDictionary(x => x.Key, x => x.Value) : null;
            bucks = condition ? null : GetBucks(keys.Select(x => x.Value).ToList());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Get(in ReadOnlySpan<byte> span)
        {
            const int NotFound = -1;
            if (pairs != null)
                return pairs.TryGetValue(Converter.Encoding.GetString(in span), out var index) ? index : NotFound;
            var hash = GetHashCode(in span);
            var data = bucks[(uint)hash % BuckSize];
            if (data != null)
                for (var i = 0; i < data.Length; i++)
                    if (MemoryExtensions.SequenceEqual(span, data[i].Bytes))
                        return data[i].Index;
            return NotFound;
        }
    }
}
