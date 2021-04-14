using Mikodev.Binary.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Mikodev.Binary.External
{
    internal sealed partial class BinaryDictionary<T>
    {
        private readonly T content;

        private readonly bool iterate;

        private readonly int[] buckets;

        private readonly Slot[] records;

        private BinaryDictionary(int[] buckets, Slot[] records, T content)
        {
            this.content = content;
            this.buckets = buckets;
            this.records = records;
            this.iterate = records.Length < 7;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool GetEqual(ref byte source, int length, ref Slot slot, int hash)
        {
            return hash == slot.Hash && BinaryHelper.GetEquality(ref source, length, slot.Head);
        }

        private T GetValueSmall(ref byte source, int length)
        {
            Debug.Assert(this.iterate is true);
            Debug.Assert(this.records.Length < 7);
            var records = this.records;
            var hash = BinaryHelper.GetHashCode(ref source, length);
            var next = 0;
            while ((uint)next < (uint)records.Length)
            {
                ref var slot = ref records[next];
                if (GetEqual(ref source, length, ref slot, hash))
                    return slot.Item;
                next++;
            }
            return this.content;
        }

        private T GetValueLarge(ref byte source, int length)
        {
            var buckets = this.buckets;
            var records = this.records;
            var hash = BinaryHelper.GetHashCode(ref source, length);
            var next = buckets[(int)((uint)hash % (uint)buckets.Length)];
            while ((uint)next < (uint)records.Length)
            {
                ref var slot = ref records[next];
                if (GetEqual(ref source, length, ref slot, hash))
                    return slot.Item;
                next = slot.Next;
            }
            return this.content;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal T GetValue(ref byte source, int length)
        {
            if (this.iterate)
                return GetValueSmall(ref source, length);
            else
                return GetValueLarge(ref source, length);
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
                    if (GetEqual(ref source, length, ref slot, hash))
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
