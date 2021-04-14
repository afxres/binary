using Mikodev.Binary.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Mikodev.Binary.External
{
    internal sealed partial class BinaryDictionary<T>
    {
        private readonly T content;

        private readonly int[] buckets;

        private readonly Slot[] records;

        private BinaryDictionary(int[] buckets, Slot[] records, T content)
        {
            this.content = content;
            this.buckets = buckets;
            this.records = records;
        }

        internal T GetValue(ref byte source, int length)
        {
            var buckets = this.buckets;
            var records = this.records;
            var hash = BinaryHelper.GetHashCode(ref source, length);
            var next = buckets[(int)((uint)hash % (uint)buckets.Length)];
            while ((uint)next < (uint)records.Length)
            {
                ref var slot = ref records[next];
                if (hash == slot.Hash && BinaryHelper.GetEquality(ref source, length, slot.Head))
                    return slot.Item;
                next = slot.Next;
            }
            return this.content;
        }

        internal static BinaryDictionary<T> Create(IReadOnlyCollection<KeyValuePair<ReadOnlyMemory<byte>, T>> items, T @default)
        {
            var free = 0;
            var records = new Slot[items.Count];
            var buckets = new int[BinaryHelper.GetCapacity(records.Length)];
            Array.Fill(buckets, -1);

            foreach (var pair in items)
            {
                var buffer = pair.Key.ToArray();
                var length = buffer.Length;
                ref var source = ref SharedHelper.GetArrayDataReference(buffer);
                var hash = BinaryHelper.GetHashCode(ref source, length);
                ref var next = ref buckets[(int)((uint)hash % (uint)buckets.Length)];
                while ((uint)next < (uint)records.Length)
                {
                    ref var slot = ref records[next];
                    if (hash == slot.Hash && BinaryHelper.GetEquality(ref source, length, slot.Head))
                        return null;
                    next = ref slot.Next;
                }
                records[free] = new Slot { Head = buffer, Hash = hash, Item = pair.Value, Next = -1 };
                next = free;
                free++;
            }

            Debug.Assert(records.Length == free);
            Debug.Assert(records.All(x => x.Head is not null));
            Debug.Assert(records.Select(x => x.Next).All(next => next is -1 || (uint)next < (uint)records.Length));
            return new BinaryDictionary<T>(buckets, records, @default);
        }
    }
}
