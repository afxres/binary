using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace Mikodev.Binary.External
{
    internal sealed partial class BinaryDictionary<T>
    {
        private readonly int[] buckets;

        private readonly Slot[] records;

        private BinaryDictionary(int[] buckets, Slot[] records)
        {
            this.buckets = buckets;
            this.records = records;
        }

        internal T GetValueOrDefault(ReadOnlySpan<byte> span, T @default)
        {
            var buckets = this.buckets;
            var records = this.records;
            var hash = BinaryHelper.GetHashCode(ref MemoryMarshal.GetReference(span), span.Length);
            var next = buckets[(int)((uint)hash % (uint)buckets.Length)];
            while (next is not -1)
            {
                ref var slot = ref records[next];
                if (hash == slot.Hash && BinaryHelper.GetEquality(span, slot.Head))
                    return slot.Item;
                next = slot.Next;
            }
            return @default;
        }

        internal static BinaryDictionary<T> Create(IReadOnlyCollection<KeyValuePair<ReadOnlyMemory<byte>, T>> items)
        {
            var free = 0;
            var records = new Slot[items.Count];
            var buckets = new int[BinaryHelper.GetCapacity(records.Length)];
            Array.Fill(buckets, -1);

            foreach (var pair in items)
            {
                var head = pair.Key.ToArray();
                var item = pair.Value;
                var span = new ReadOnlySpan<byte>(head);
                var hash = BinaryHelper.GetHashCode(ref MemoryMarshal.GetReference(span), span.Length);
                ref var next = ref buckets[(int)((uint)hash % (uint)buckets.Length)];
                while (next is not -1)
                {
                    ref var slot = ref records[next];
                    if (hash == slot.Hash && BinaryHelper.GetEquality(span, slot.Head))
                        return null;
                    next = ref slot.Next;
                }
                records[free] = new Slot { Head = head, Hash = hash, Item = item, Next = -1 };
                next = free;
                free++;
            }

            Debug.Assert(records.Length == free);
            Debug.Assert(records.All(x => x.Head is not null));
            return new BinaryDictionary<T>(buckets, records);
        }
    }
}
