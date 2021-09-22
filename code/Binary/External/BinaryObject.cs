namespace Mikodev.Binary.External;

using Mikodev.Binary.External.Contexts;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

internal static class BinaryObject
{
    internal const int ItemLimits = 7;

    internal const int DataFallback = -1;

    internal const int LongDataLimits = sizeof(long);

    internal static ByteViewDictionary<int>? Create(ImmutableArray<ReadOnlyMemory<byte>> items)
    {
        Debug.Assert(items.Any());
        if (items.Length <= ItemLimits && items.All(x => x.Length <= LongDataLimits))
            return CreateLongDataDictionary(items);
        else
            return CreateHashCodeDictionary(items.Select(KeyValuePair.Create).ToImmutableArray(), DataFallback);
    }

    private static LongDataDictionary? CreateLongDataDictionary(ImmutableArray<ReadOnlyMemory<byte>> items)
    {
        static long Select(ReadOnlySpan<byte> span) => BinaryModule.GetLongData(ref MemoryMarshal.GetReference(span), span.Length);
        var records = items.Select(x => new LongDataSlot { Data = Select(x.Span), Size = x.Length }).ToArray();
        if (records.Select(x => (x.Data, x.Size)).Distinct().Count() != items.Length)
            return null;
        return new LongDataDictionary(records);
    }

    private static HashCodeDictionary<T>? CreateHashCodeDictionary<T>(ImmutableArray<KeyValuePair<ReadOnlyMemory<byte>, T>> items, T @default)
    {
        var free = 0;
        var records = new HashCodeSlot<T>[items.Length];
        var buckets = new int[BinaryModule.GetCapacity(records.Length)];
        Array.Fill(buckets, -1);

        foreach (var pair in items)
        {
            var buffer = pair.Key.ToArray();
            var length = buffer.Length;
#if NET5_0_OR_GREATER
            ref var source = ref MemoryMarshal.GetArrayDataReference(buffer);
#else
            ref var source = ref MemoryMarshal.GetReference(new Span<byte>(buffer));
#endif
            var hash = BinaryModule.GetHashCode(ref source, length);
            ref var next = ref buckets[(int)((uint)hash % (uint)buckets.Length)];
            while ((uint)next < (uint)records.Length)
            {
                ref var slot = ref records[next];
                if (hash == slot.Hash && BinaryModule.GetEquality(ref source, length, slot.Head))
                    return null;
                next = ref slot.Next;
            }
            records[free] = new HashCodeSlot<T> { Head = buffer, Hash = hash, Item = pair.Value, Next = -1 };
            next = free;
            free++;
        }

        Debug.Assert(records.Length == free);
        Debug.Assert(records.All(x => x.Head is not null));
        Debug.Assert(records.Select(x => x.Next).All(next => next is -1 || (uint)next < (uint)records.Length));
        return new HashCodeDictionary<T>(buckets, records, @default);
    }
}
