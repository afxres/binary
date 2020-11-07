namespace Mikodev.Binary.Internal.Fallback
{
    internal abstract class FallbackEncoder<T>
    {
        public abstract void EncodeAuto(ref Allocator allocator, T item);
    }
}
