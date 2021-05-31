namespace Mikodev.Binary.External.Contexts
{
    internal struct HashCodeSlot<T>
    {
        public byte[] Head;

        public int Hash;

        public int Next;

        public T Item;
    }
}
