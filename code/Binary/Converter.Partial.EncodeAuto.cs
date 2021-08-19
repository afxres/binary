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
}
