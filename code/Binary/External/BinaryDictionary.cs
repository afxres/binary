namespace Mikodev.Binary.External
{
    internal sealed class BinaryDictionary<T>
    {
        private readonly T content;

        private readonly int[] buckets;

        private readonly BinarySlot<T>[] records;

        public BinaryDictionary(int[] buckets, BinarySlot<T>[] records, T content)
        {
            this.content = content;
            this.buckets = buckets;
            this.records = records;
        }

        public T GetValue(ref byte source, int length)
        {
            var buckets = this.buckets;
            var records = this.records;
            var hash = BinaryHelper.GetHashCode(ref source, length);
            var next = buckets[(int)((uint)hash % (uint)buckets.Length)];
            while ((uint)next < (uint)records.Length)
            {
                ref readonly var slot = ref records[next];
                if (hash == slot.Hash && BinaryHelper.GetEquality(ref source, length, slot.Head))
                    return slot.Item;
                next = slot.Next;
            }
            return this.content;
        }
    }
}
