namespace Mikodev.Binary.External
{
    internal sealed partial class BinaryDictionary<T>
    {
        private struct Slot
        {
            public byte[] Head;

            public int Hash;

            public int Next;

            public T Item;
        }
    }
}
