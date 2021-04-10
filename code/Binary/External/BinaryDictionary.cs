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

        internal T GetValueOrDefault(ref byte source, int length, T @default)
        {
            var buckets = this.buckets;
            var records = this.records;
            var hash = BinaryHelper.GetHashCode(ref source, length);
            var next = buckets[(int)((uint)hash % (uint)buckets.Length)];
            while (next is not -1)
            {
                ref var slot = ref records[next];
                if (hash == slot.Hash && BinaryHelper.GetEquality(slot.Head, ref source, length))
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
                var buffer = pair.Key.ToArray();
                var length = buffer.Length;
                ref var source = ref MemoryMarshal.GetReference(new ReadOnlySpan<byte>(buffer));
                var hash = BinaryHelper.GetHashCode(ref source, length);
                ref var next = ref buckets[(int)((uint)hash % (uint)buckets.Length)];
                while (next is not -1)
                {
                    ref var slot = ref records[next];
                    if (hash == slot.Hash && BinaryHelper.GetEquality(slot.Head, ref source, length))
                        return null;
                    next = ref slot.Next;
                }
                records[free] = new Slot { Head = buffer, Hash = hash, Item = pair.Value, Next = -1 };
                next = free;
                free++;
            }

            Debug.Assert(records.Length == free);
            Debug.Assert(records.All(x => x.Head is not null));
            return new BinaryDictionary<T>(buckets, records);
        }
    }
}
