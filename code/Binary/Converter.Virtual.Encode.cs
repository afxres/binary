namespace Mikodev.Binary;

public abstract partial class Converter<T>
{
    public virtual byte[] Encode(T item)
    {
        var length = this.length;
        if (length is not 0)
            return EncodeConstant(item);
        else
            return EncodeVariable(item);
    }
}
