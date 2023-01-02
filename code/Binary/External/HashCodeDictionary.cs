namespace Mikodev.Binary.External;

using Mikodev.Binary.External.Contexts;

internal sealed class HashCodeDictionary<T> : ByteViewDictionary<T>
{
    private readonly T content;

    private readonly int[] buckets;

    private readonly HashCodeSlot<T>[] records;

    public HashCodeDictionary(int[] buckets, HashCodeSlot<T>[] records, T content)
    {
        this.content = content;
        this.buckets = buckets;
        this.records = records;
    }

    public override T GetValue(ref byte source, int length)
    {
        var buckets = this.buckets;
        var records = this.records;
        var hash = BinaryModule.GetHashCode(ref source, length);
        var next = buckets[(int)(hash % (uint)buckets.Length)];
        while ((uint)next < (uint)records.Length)
        {
            ref readonly var slot = ref records[next];
            if (hash == slot.Hash && BinaryModule.GetEquality(ref source, length, slot.Head))
                return slot.Item;
            next = slot.Next;
        }
        return this.content;
    }
}
