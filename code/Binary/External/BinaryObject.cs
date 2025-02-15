namespace Mikodev.Binary.External;

using Mikodev.Binary.External.Contexts;
using Mikodev.Binary.Internal;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

internal static class BinaryObject
{
    internal static ByteViewList? Create(ImmutableArray<ReadOnlyMemory<byte>> items, out int error)
    {
        Debug.Assert(items.Any());
        var view = CreateLongDataList(items, out error);
        if (view is null && error is -1)
            return CreateHashCodeList(items, out error);
        return view;
    }

    private static LongDataList? CreateLongDataList(ImmutableArray<ReadOnlyMemory<byte>> items, out int error)
    {
        error = -1;
        if (items.Length > BinaryDefine.LongDataListItemCountLimits)
            return null;
        var records = new List<LongDataSlot>();
        for (var i = 0; i < items.Length; i++)
        {
            var span = items[i].Span;
            if (span.Length > BinaryDefine.LongDataListItemBytesLimits)
                return null;
            var slot = BinaryModule.GetLongData(ref MemoryMarshal.GetReference(span), span.Length);
            if (records.Any(x => x.Head == slot.Head && x.Tail == slot.Tail))
            {
                error = i;
                return null;
            }
            records.Add(slot);
        }
        return new LongDataList([.. records]);
    }

    private static HashCodeList? CreateHashCodeList(ImmutableArray<ReadOnlyMemory<byte>> items, out int error)
    {
        error = -1;
        var records = new HashCodeSlot[items.Length];
        var buckets = new int[DetectHashCodeListBucketLength(records.Length)];
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
                {
                    error = i;
                    return null;
                }
                next = ref slot.Next;
            }
            records[i] = new HashCodeSlot { Head = buffer, Hash = hash, Next = -1 };
            next = i;
        }

        Debug.Assert(records.Select(x => x.Next).All(next => next is -1 || (uint)next < (uint)records.Length));
        return new HashCodeList(buckets, records);
    }

    private static int DetectHashCodeListBucketLength(int capacity)
    {
        var result = BinaryDefine.HashCodeListPrimes.FirstOrDefault(x => x >= capacity);
        if (result is 0)
            ThrowHelper.ThrowMaxCapacityOverflow();
        return result;
    }
}
