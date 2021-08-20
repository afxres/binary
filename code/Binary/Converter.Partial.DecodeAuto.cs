namespace Mikodev.Binary;

using Mikodev.Binary.Internal.Metadata;

public abstract partial class Converter<T>
{
    private enum DecodeOption { Constant, Variable, VariableOverride };

    private DecodeOption DecodeOptionInternal()
    {
        var length = this.length;
        if (length is not 0)
            return DecodeOption.Constant;
        var method = new DecodeDelegate<T>(DecodeWithLengthPrefix).Method;
        if (method.DeclaringType == typeof(Converter<T>))
            return DecodeOption.Variable;
        else
            return DecodeOption.VariableOverride;
    }
}
