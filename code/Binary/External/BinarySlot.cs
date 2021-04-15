namespace Mikodev.Binary.External
{
    internal struct BinarySlot<T>
    {
        public byte[] Head;

        public int Hash;

        public int Next;

        public T Item;
    }
}
