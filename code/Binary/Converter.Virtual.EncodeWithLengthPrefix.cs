namespace Mikodev.Binary;

public abstract partial class Converter<T>
{
    public virtual void EncodeWithLengthPrefix(ref Allocator allocator, T item)
    {
        var length = this.length;
        if (length is not 0)
            EncodeWithLengthPrefixConstant(ref allocator, item);
        else
            EncodeWithLengthPrefixVariable(ref allocator, item);
    }
}
