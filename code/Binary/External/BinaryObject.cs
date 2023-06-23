namespace Mikodev.Binary.External;

using Mikodev.Binary.External.Contexts;
using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

internal static class BinaryObject
{
    internal const int ItemLimits = 7;

    internal const int LongDataLimits = 15;

    internal static ByteViewList? Create(ImmutableArray<ReadOnlyMemory<byte>> items)
    {
        Debug.Assert(items.Any());
        if (items.Length <= ItemLimits && items.All(x => x.Length <= LongDataLimits))
            return CreateLongDataList(items);
        else
            return CreateHashCodeList(items);
    }

    private static LongDataList? CreateLongDataList(ImmutableArray<ReadOnlyMemory<byte>> items)
    {
        static LongDataSlot Invoke(ReadOnlySpan<byte> span) => BinaryModule.GetLongData(ref MemoryMarshal.GetReference(span), span.Length);
        var records = items.Select(x => Invoke(x.Span)).ToArray();
        if (records.DistinctBy(x => (x.Head, x.Tail)).Count() != items.Length)
            return null;
        return new LongDataList(records);
    }

    private static HashCodeList? CreateHashCodeList(ImmutableArray<ReadOnlyMemory<byte>> items)
    {
        var records = new HashCodeSlot[items.Length];
        var buckets = new int[BinaryModule.GetCapacity(records.Length)];
        Array.Fill(buckets, -1);

        for (var i = 0; i < items.Length; i++)
        {
            var buffer = items[i].ToArray();
            var length = buffer.Length;
            ref var source = ref MemoryMarshal.GetArrayDataReference(buffer);
            var hash = BinaryModule.GetHashCode(ref source, length);
            ref var next = ref buckets[(int)(hash % (uint)buckets.Length)];
            while ((uint)next < (uint)records.Length)
            {
                ref var slot = ref records[next];
                if (hash == slot.Hash && BinaryModule.GetEquality(ref source, length, slot.Head))
                    return null;
                next = ref slot.Next;
            }
            records[i] = new HashCodeSlot { Head = buffer, Hash = hash, Next = -1 };
            next = i;
        }

        Debug.Assert(records.Select(x => x.Next).All(next => next is -1 || (uint)next < (uint)records.Length));
        return new HashCodeList(buckets, records);
    }
}
