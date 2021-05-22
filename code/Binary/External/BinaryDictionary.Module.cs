using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace Mikodev.Binary.External
{
    internal static class BinaryDictionary
    {
        internal static BinaryDictionary<T> Create<T>(ImmutableArray<KeyValuePair<ReadOnlyMemory<byte>, T>> items, T @default)
        {
            var free = 0;
            var records = new BinarySlot<T>[items.Length];
            var buckets = new int[BinaryHelper.GetCapacity(records.Length)];
            Array.Fill(buckets, -1);

            foreach (var pair in items)
            {
                var buffer = pair.Key.ToArray();
                var length = buffer.Length;
                ref var source = ref MemoryMarshal.GetArrayDataReference(buffer);
                var hash = BinaryHelper.GetHashCode(ref source, length);
                ref var next = ref buckets[(int)((uint)hash % (uint)buckets.Length)];
                while ((uint)next < (uint)records.Length)
                {
                    ref var slot = ref records[next];
                    if (hash == slot.Hash && BinaryHelper.GetEquality(ref source, length, slot.Head))
                        return null;
                    next = ref slot.Next;
                }
                records[free] = new BinarySlot<T> { Head = buffer, Hash = hash, Item = pair.Value, Next = -1 };
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
