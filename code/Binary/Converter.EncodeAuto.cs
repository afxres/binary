namespace Mikodev.Binary;

using Mikodev.Binary.Internal.Metadata;

public abstract partial class Converter<T>
{
    private enum EncodeOption { Constant, Variable, VariableOverride };

    private EncodeOption EncodeOptionInternal()
    {
        var length = this.length;
        if (length is not 0)
            return EncodeOption.Constant;
        var method = new EncodeDelegate<T>(EncodeWithLengthPrefix).Method;
        if (method.DeclaringType == typeof(Converter<T>))
            return EncodeOption.Variable;
        else
            return EncodeOption.VariableOverride;
    }

    private void EncodeAutoInternal(ref Allocator allocator, T item)
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
