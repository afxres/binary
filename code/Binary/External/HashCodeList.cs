namespace Mikodev.Binary.External;

using Mikodev.Binary.External.Contexts;

internal sealed class HashCodeList(int[] buckets, HashCodeSlot[] records) : ByteViewList
{
    private readonly int[] buckets = buckets;

    private readonly HashCodeSlot[] records = records;

    public override int Invoke(ref byte source, int length)
    {
        var buckets = this.buckets;
        var records = this.records;
        var hash = BinaryModule.GetHashCode(ref source, length);
        var next = buckets[(int)(hash % (uint)buckets.Length)];
        while ((uint)next < (uint)records.Length)
        {
            ref readonly var slot = ref records[next];
            if (hash == slot.Hash && BinaryModule.GetEquality(ref source, length, slot.Head))
                return next;
            next = slot.Next;
        }
        return -1;
    }
}
