namespace Mikodev.Binary.External
{
    internal abstract class ByteViewDictionary<T>
    {
        public abstract T GetValue(ref byte source, int length);
    }
}
