namespace Mikodev.Binary;

public abstract partial class Converter<T>
{
    public virtual void EncodeAuto(ref Allocator allocator, T item)
    {
        var option = this.encode;
        if (option is EncodeOption.Constant)
            Encode(ref allocator, item);
        else if (option is EncodeOption.Variable)
            EncodeWithLengthPrefixVariable(ref allocator, item);
        else
            EncodeWithLengthPrefix(ref allocator, item);
    }
}
